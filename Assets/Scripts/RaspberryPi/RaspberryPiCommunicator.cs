using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Echoesphere.Runtime.RaspberryPi {
    public class RaspberryPiCommunicator : MonoBehaviour {
        [Header("服务器设置")] public string host = "127.0.0.1";
        [Header("服务器端口")] public int port = 65432;

        public event Action<string> OnMessageReceived;
        public event Action<byte[]> OnImageReceived;

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;
        private SynchronizationContext _mainThreadContext;
        private bool _isConnected;
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        private void Start() {
            _mainThreadContext = SynchronizationContext.Current;
            _cts = new CancellationTokenSource();
            _ = ConnectAsync();
        }

        private async Task ConnectAsync() {
            try {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port);
                _stream = _tcpClient.GetStream();
                _isConnected = true;
                Debug.Log($"[客户端] 已连接至 {host}:{port}");
                _ = ReceiveLoopAsync();
            } catch (Exception ex) {
                Debug.LogError($"[客户端] 连接失败: {ex.Message}");
            }
        }

        private async Task ReceiveLoopAsync() {
            var lengthBuffer = new byte[4];
            try {
                while (_isConnected && _tcpClient.Connected) {
                    int bytesRead = 0;
                    while (bytesRead < 4) {
                        int read = await _stream.ReadAsync(lengthBuffer, bytesRead, 4 - bytesRead, _cts.Token);
                        if (read == 0) throw new Exception("连接关闭");
                        bytesRead += read;
                    }

                    int totalLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                    var typeAndData = new byte[totalLength];
                    bytesRead = 0;
                    while (bytesRead < totalLength) {
                        int read = await _stream.ReadAsync(typeAndData, bytesRead, totalLength - bytesRead, _cts.Token);
                        if (read == 0) throw new Exception("连接关闭");
                        bytesRead += read;
                    }

                    MessageType msgType = (MessageType)typeAndData[0];
                    var payload = new byte[totalLength - 1];
                    Array.Copy(typeAndData, 1, payload, 0, totalLength - 1);

                    switch (msgType) {
                        case MessageType.Text:
                            string message = Encoding.UTF8.GetString(payload);
                            Debug.Log($"[收到] {message}");
                            _mainThreadContext.Post(_ => OnMessageReceived?.Invoke(message), null);
                            break;
                        case MessageType.Image:
                            Debug.Log($"[收到] 图像，大小={payload.Length}字节");
                            _mainThreadContext.Post(_ => OnImageReceived?.Invoke(payload), null);
                            break;
                        default:
                            Debug.LogWarning($"[未知] 消息类型 {(byte)msgType}");
                            break;
                    }
                }
            } catch (Exception ex) when (_isConnected) {
                Debug.LogError($"[接收错误] {ex.Message}");
            }
            finally {
                Disconnect();
            }
        }

        private async Task SendAsync(MessageType type, byte[] data) {
            if (!_isConnected || _stream == null) return;
            await _sendLock.WaitAsync(_cts.Token);
            try {
                var lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(1 + data.Length));
                await _stream.WriteAsync(lengthPrefix, 0, 4, _cts.Token);
                await _stream.WriteAsync(new[] { (byte)type }, 0, 1, _cts.Token);
                await _stream.WriteAsync(data, 0, data.Length, _cts.Token);
                Debug.Log($"[发送] 类型={type}, 长度={data.Length}");
            }
            finally {
                _sendLock.Release();
            }
        }

        public Task SendText(string message) => SendAsync(MessageType.Text, Encoding.UTF8.GetBytes(message));

        public Task SendImage(byte[] imageBytes) => SendAsync(MessageType.Image, imageBytes);

        /// <summary> 发送截图 </summary>
        public IEnumerator SendScreenshot() {
            yield return new WaitForEndOfFrame();

            int width = Screen.width;
            int height = Screen.height;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            byte[] jpgBytes = tex.EncodeToJPG(75);
            Destroy(tex);

            var sendTask = SendImage(jpgBytes);
            yield return new WaitUntil(() => sendTask.IsCompleted);

            if (sendTask.Exception != null)
                Debug.LogError($"[截图发送异常] {sendTask.Exception}");
            else
                Debug.Log("[截图] 发送完成");
        }

        private void Disconnect() {
            _isConnected = false;
            _stream?.Close();
            _tcpClient?.Close();
            Debug.Log("[客户端] 已断开连接");
        }

        private void OnDestroy() {
            _isConnected = false;
            _cts?.Cancel();
            _stream?.Dispose();
            _tcpClient?.Close();
            _sendLock?.Dispose();
        }

        // 消息类型枚举
        private enum MessageType : byte {
            Text = 0x00,
            Image = 0x01
        }
    }
}