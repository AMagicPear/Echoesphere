using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RaspberryPi {
    public class RaspberryPiCommunicator : MonoBehaviour {
        [Header("服务器设置")] public int port = 65432;

        public event Action<string> OnMessageReceived;

        private TcpListener _tcpListener;
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isRunning;
        private CancellationTokenSource _cts;
        private SynchronizationContext _mainThreadContext;

        private async void Start() {
            _mainThreadContext = SynchronizationContext.Current;
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();
            _isRunning = true;
            _cts = new CancellationTokenSource();
            Debug.Log($"TCP服务器启动于端口 {port}");

            await AcceptClientAsync();
        }

        private async Task AcceptClientAsync() {
            while (_isRunning) {
                try {
                    _client = await _tcpListener.AcceptTcpClientAsync();
                    _stream = _client.GetStream();
                    Debug.Log("客户端已连接");
                    _ = ReceiveMessagesAsync(); // 不等待
                } catch (Exception ex) {
                    if (_isRunning)
                        Debug.LogError($"接受客户端错误: {ex.Message}");
                }
            }
        }

        private async Task ReceiveMessagesAsync() {
            byte[] lengthBuffer = new byte[4];
            try {
                while (_client.Connected) {
                    // 读取消息长度（4字节）
                    int bytesRead = 0;
                    while (bytesRead < 4) {
                        int read = await _stream.ReadAsync(lengthBuffer, bytesRead, 4 - bytesRead, _cts.Token);
                        if (read == 0) throw new Exception("连接关闭");
                        bytesRead += read;
                    }

                    int msgLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                    // 读取消息内容
                    byte[] msgBuffer = new byte[msgLength];
                    bytesRead = 0;
                    while (bytesRead < msgLength) {
                        int read = await _stream.ReadAsync(msgBuffer, bytesRead, msgLength - bytesRead, _cts.Token);
                        if (read == 0) throw new Exception("连接关闭");
                        bytesRead += read;
                    }

                    string message = Encoding.UTF8.GetString(msgBuffer);
                    Debug.Log($"收到消息: {message}");
                    _mainThreadContext.Post(_ => OnMessageReceived?.Invoke(message), null);
                }
            } catch (Exception ex) {
                Debug.LogError($"接收错误: {ex.Message}");
            }
            finally {
                _client?.Close();
                _client = null;
                _stream = null;
            }
        }

        public async void SendData(string message) {
            if (_client == null || !_client.Connected) {
                Debug.LogWarning("没有客户端连接");
                return;
            }

            try {
                byte[] data = Encoding.UTF8.GetBytes(message);
                byte[] lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
                await _stream.WriteAsync(lengthPrefix, 0, 4);
                await _stream.WriteAsync(data, 0, data.Length);
                Debug.Log($"发送消息: {message}");
            } catch (Exception ex) {
                Debug.LogError($"发送错误: {ex.Message}");
            }
        }

        private void OnDestroy() {
            _isRunning = false;
            _cts?.Cancel();
            _tcpListener?.Stop();
            _client?.Close();
        }
    }
}