using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Echoesphere.Runtime.RaspberryPi {
    public class RaspberryPiCommunicator : MonoBehaviour {
        [Header("服务器设置")] public int port = 65432;

        public event Action<string> OnMessageReceived;

        private TcpListener _tcpListener;
        private bool _isRunning;
        private CancellationTokenSource _cts;
        private SynchronizationContext _mainThreadContext;
        private readonly ConcurrentDictionary<string, ClientHandler> _clients = new();

        private async void Start() {
            _mainThreadContext = SynchronizationContext.Current;
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();
            _isRunning = true;
            _cts = new CancellationTokenSource();
            Debug.Log($"[服务器] 启动于端口 {port}");

            await AcceptClientsAsync();
        }

        private async Task AcceptClientsAsync() {
            while (_isRunning) {
                try {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    var handler = new ClientHandler(tcpClient, this, _cts.Token);
                    string clientId = tcpClient.Client.RemoteEndPoint.ToString();
                    if (_clients.TryAdd(clientId, handler)) {
                        Debug.Log($"[连接] {clientId} 已连接");
                        _ = handler.StartAsync();
                    }
                } catch (Exception ex) when (_isRunning) {
                    Debug.LogError($"[接受错误] {ex.Message}");
                }
            }
        }

        private void RemoveClient(string clientId) {
            if (_clients.TryRemove(clientId, out var handler)) {
                Debug.Log($"[断开] {clientId}");
            }
        }

        /// <summary> 向所有客户端发送文本消息 </summary>
        public async Task SendToAll(string message) {
            foreach (var handler in _clients.Values) {
                try {
                    await handler.SendText(message);
                } catch (Exception ex) {
                    Debug.LogError($"[发送失败] 给 {handler.ClientId}: {ex.Message}");
                }
            }

            Debug.Log($"[广播] {message}");
        }

        /// <summary> 向所有客户端广播截图（协程） </summary>
        public IEnumerator BroadcastScreenshot() {
            Debug.Log($"[截图] 开始广播，客户端数: {_clients.Count}");
            if (_clients.IsEmpty) yield break;
            foreach (var handler in _clients.Values) {
                yield return handler.CaptureAndSendScreenshot();
                Debug.Log($"[截图] 已发送给 {handler.ClientId}");
            }
        }

        public ClientHandler GetClientHandler(string clientId) => _clients.GetValueOrDefault(clientId);

        public async Task SendToClient(string clientId, string message) {
            if (_clients.TryGetValue(clientId, out var handler)) {
                try {
                    await handler.SendText(message);
                    Debug.Log($"[发送] 给 {clientId}: {message}");
                } catch (Exception ex) {
                    Debug.LogError($"[发送失败] 给 {clientId}: {ex.Message}");
                }
            } else {
                Debug.LogWarning($"[警告] 客户端 {clientId} 不存在");
            }
        }

        private void OnDestroy() {
            _isRunning = false;
            _cts?.Cancel();
            _tcpListener?.Stop();
            foreach (var handler in _clients.Values)
                handler.Dispose();
            _clients.Clear();
        }

        // 消息类型枚举
        private enum MessageType : byte {
            Text = 0x00,
            Image = 0x01
        }

        public class ClientHandler : IDisposable {
            private readonly TcpClient _tcpClient;
            private readonly NetworkStream _stream;
            private readonly RaspberryPiCommunicator _server;
            private readonly CancellationToken _cancellationToken;
            private readonly string _clientId;
            private readonly SemaphoreSlim _sendLock = new(1, 1);

            public string ClientId => _clientId;

            public ClientHandler(TcpClient tcpClient, RaspberryPiCommunicator server,
                CancellationToken cancellationToken) {
                _tcpClient = tcpClient;
                _stream = tcpClient.GetStream();
                _server = server;
                _cancellationToken = cancellationToken;
                _clientId = tcpClient.Client.RemoteEndPoint.ToString();
            }

            public async Task StartAsync() {
                var lengthBuffer = new byte[4];
                try {
                    while (_tcpClient.Connected) {
                        // 读取总长度
                        int bytesRead = 0;
                        while (bytesRead < 4) {
                            int read = await _stream.ReadAsync(lengthBuffer, bytesRead, 4 - bytesRead,
                                _cancellationToken);
                            if (read == 0) throw new Exception("连接关闭");
                            bytesRead += read;
                        }

                        int totalLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                        // 读取类型+数据
                        var typeAndData = new byte[totalLength];
                        bytesRead = 0;
                        while (bytesRead < totalLength) {
                            int read = await _stream.ReadAsync(typeAndData, bytesRead, totalLength - bytesRead,
                                _cancellationToken);
                            if (read == 0) throw new Exception("连接关闭");
                            bytesRead += read;
                        }

                        MessageType msgType = (MessageType)typeAndData[0];
                        var payload = new byte[totalLength - 1];
                        Array.Copy(typeAndData, 1, payload, 0, totalLength - 1);

                        switch (msgType) {
                            case MessageType.Text:
                                string message = Encoding.UTF8.GetString(payload);
                                Debug.Log($"[收到] 来自 {_clientId}: {message}");
                                _server._mainThreadContext.Post(_ => _server.OnMessageReceived?.Invoke(message), null);
                                break;
                            case MessageType.Image:
                                Debug.Log($"[收到] 来自 {_clientId} 的图像，大小={payload.Length}字节");
                                // 可扩展图像处理事件
                                break;
                            default:
                                Debug.LogWarning($"[未知] 消息类型 {(byte)msgType} 来自 {_clientId}");
                                break;
                        }
                    }
                } catch (Exception ex) {
                    Debug.LogError($"[接收错误] {_clientId}: {ex.Message}");
                }
                finally {
                    _server.RemoveClient(_clientId);
                    _tcpClient?.Close();
                }
            }

            private async Task SendAsync(MessageType type, byte[] data) {
                await _sendLock.WaitAsync(_cancellationToken);
                try {
                    var lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(1 + data.Length));
                    await _stream.WriteAsync(lengthPrefix, 0, 4, _cancellationToken);
                    await _stream.WriteAsync(new[] { (byte)type }, 0, 1, _cancellationToken);
                    await _stream.WriteAsync(data, 0, data.Length, _cancellationToken);
                    Debug.Log($"[发送] 类型={type}, 长度={data.Length}");
                }
                finally {
                    _sendLock.Release();
                }
            }

            public Task SendText(string message) => SendAsync(MessageType.Text, Encoding.UTF8.GetBytes(message));

            public Task SendImage(byte[] imageBytes) => SendAsync(MessageType.Image, imageBytes);

            public IEnumerator CaptureAndSendScreenshot() {
                yield return new WaitForEndOfFrame();

                // 截图并编码（兼容旧版 Unity，若使用新版可用 ScreenCapture.CaptureScreenshotAsTexture）
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

            public void Dispose() {
                _tcpClient?.Close();
                _stream?.Dispose();
                _sendLock?.Dispose();
            }
        }
    }
}