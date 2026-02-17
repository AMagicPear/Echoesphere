using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RaspberryPi {
    public class RaspberryPiCommunicator : MonoBehaviour {
        [Header("服务器设置")] public int port = 65432;

        public event Action<string> OnMessageReceived; // 收到消息的事件（来自任意客户端）

        private TcpListener _tcpListener;
        private bool _isRunning;
        private CancellationTokenSource _cts;
        private SynchronizationContext _mainThreadContext;

        // 存储所有已连接的客户端，使用连接ID作为键（这里用客户端的远程端点字符串）
        private ConcurrentDictionary<string, ClientHandler>
            _clients = new ConcurrentDictionary<string, ClientHandler>();

        private async void Start() {
            _mainThreadContext = SynchronizationContext.Current;
            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();
            _isRunning = true;
            _cts = new CancellationTokenSource();
            Debug.Log($"TCP服务器启动于端口 {port}");

            await AcceptClientsAsync();
        }

        private async Task AcceptClientsAsync() {
            while (_isRunning) {
                try {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    // 为每个客户端创建一个处理器
                    var handler = new ClientHandler(tcpClient, this, _cts.Token);
                    string clientId = tcpClient.Client.RemoteEndPoint.ToString();
                    if (_clients.TryAdd(clientId, handler)) {
                        Debug.Log($"客户端 {clientId} 已连接");
                        _ = handler.StartAsync(); // 启动接收循环，不等待
                    }
                } catch (Exception ex) {
                    if (_isRunning)
                        Debug.LogError($"接受客户端错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 移除客户端（由ClientHandler内部调用）
        /// </summary>
        public void RemoveClient(string clientId) {
            if (_clients.TryRemove(clientId, out var handler)) {
                Debug.Log($"客户端 {clientId} 已断开");
            }
        }

        /// <summary>
        /// 广播消息给所有已连接的客户端
        /// </summary>
        public async void BroadcastMessage(string message) {
            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));

            foreach (var handler in _clients.Values) {
                try {
                    await handler.SendAsync(data, lengthPrefix);
                } catch (Exception ex) {
                    Debug.LogError($"发送消息给客户端 {handler.ClientId} 失败: {ex.Message}");
                }
            }

            Debug.Log($"广播消息: {message}");
        }

        /// <summary>
        /// 发送消息给指定客户端（通过客户端ID）
        /// </summary>
        public async void SendToClient(string clientId, string message) {
            if (_clients.TryGetValue(clientId, out var handler)) {
                byte[] data = Encoding.UTF8.GetBytes(message);
                byte[] lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
                try {
                    await handler.SendAsync(data, lengthPrefix);
                    Debug.Log($"发送消息给 {clientId}: {message}");
                } catch (Exception ex) {
                    Debug.LogError($"发送给 {clientId} 失败: {ex.Message}");
                }
            } else {
                Debug.LogWarning($"客户端 {clientId} 不存在");
            }
        }

        private void OnDestroy() {
            _isRunning = false;
            _cts?.Cancel();
            _tcpListener?.Stop();
            foreach (var handler in _clients.Values) {
                handler.Dispose();
            }

            _clients.Clear();
        }

        // 内部类，处理单个客户端的通信
        private class ClientHandler : IDisposable {
            private readonly TcpClient _tcpClient;
            private readonly NetworkStream _stream;
            private readonly RaspberryPiCommunicator _server;
            private readonly CancellationToken _cancellationToken;
            private readonly string _clientId;

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
                byte[] lengthBuffer = new byte[4];
                try {
                    while (_tcpClient.Connected) {
                        // 读取消息长度
                        int bytesRead = 0;
                        while (bytesRead < 4) {
                            int read = await _stream.ReadAsync(lengthBuffer, bytesRead, 4 - bytesRead,
                                _cancellationToken);
                            if (read == 0) throw new Exception("连接关闭");
                            bytesRead += read;
                        }

                        int msgLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                        // 读取消息内容
                        byte[] msgBuffer = new byte[msgLength];
                        bytesRead = 0;
                        while (bytesRead < msgLength) {
                            int read = await _stream.ReadAsync(msgBuffer, bytesRead, msgLength - bytesRead,
                                _cancellationToken);
                            if (read == 0) throw new Exception("连接关闭");
                            bytesRead += read;
                        }

                        string message = Encoding.UTF8.GetString(msgBuffer);
                        Debug.Log($"收到来自 {_clientId} 的消息: {message}");

                        // 触发服务器的事件（在主线程）
                        _server._mainThreadContext.Post(_ => _server.OnMessageReceived?.Invoke(message), null);
                    }
                } catch (Exception ex) {
                    Debug.LogError($"客户端 {_clientId} 接收错误: {ex.Message}");
                }
                finally {
                    _server.RemoveClient(_clientId);
                    _tcpClient?.Close();
                }
            }

            public async Task SendAsync(byte[] data, byte[] lengthPrefix) {
                await _stream.WriteAsync(lengthPrefix, 0, 4);
                await _stream.WriteAsync(data, 0, data.Length);
            }

            public void Dispose() {
                _tcpClient?.Close();
                _stream?.Dispose();
            }
        }
    }
}