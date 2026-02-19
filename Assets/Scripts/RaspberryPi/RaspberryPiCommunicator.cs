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

namespace RaspberryPi {
    public class RaspberryPiCommunicator : MonoBehaviour {
        [Header("服务器设置")] public int port = 65432;

        public event Action<string> OnMessageReceived; // 收到消息的事件（来自任意客户端）

        private TcpListener _tcpListener;
        private bool _isRunning;
        private CancellationTokenSource _cts;
        private SynchronizationContext _mainThreadContext;

        // 存储所有已连接的客户端，使用连接ID作为键（这里用客户端的远程端点字符串）
        private readonly ConcurrentDictionary<string, ClientHandler> _clients = new();

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
        private void RemoveClient(string clientId) {
            if (_clients.TryRemove(clientId, out var handler)) {
                Debug.Log($"客户端 {clientId} 已断开");
            }
        }

        /// <summary>
        /// 广播消息给所有已连接的客户端
        /// </summary>
        public new async Task BroadcastMessage(string message) {
            foreach (var handler in _clients.Values) {
                try {
                    await handler.SendData(message);
                } catch (Exception ex) {
                    Debug.LogError($"发送消息给客户端 {handler.ClientId} 失败: {ex.Message}");
                }
            }

            Debug.Log($"广播消息: {message}");
        }

        /// <summary>
        /// 广播图像给所有已连接的客户端
        /// </summary>
        public IEnumerator BroadcastScreenshot() {
            Debug.Log($"[Broadcast] 开始广播截图，当前客户端数: {_clients.Count}");
            if (_clients.IsEmpty) yield break;
            foreach (var handler in _clients.Values) {
                yield return StartCoroutine(handler.CaptureAndSendScreenshot());
                Debug.Log($"广播截图给 {handler.ClientId}");
            }
        }

        public ClientHandler GetClientHandler(string clientId) {
            return _clients.GetValueOrDefault(clientId);
        }

        /// <summary>
        /// 发送消息给指定客户端（通过客户端ID）
        /// </summary>
        public async Task SendToClient(string clientId, string message) {
            if (_clients.TryGetValue(clientId, out var handler)) {
                try {
                    await handler.SendData(message);
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
        public class ClientHandler : IDisposable {
            private readonly TcpClient _tcpClient;
            private readonly NetworkStream _stream;
            private readonly RaspberryPiCommunicator _server;
            private readonly CancellationToken _cancellationToken;
            private readonly string _clientId;
            private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

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
                        // 读取消息总长度（包含类型字节）
                        int bytesRead = 0;
                        while (bytesRead < 4) {
                            int read = await _stream.ReadAsync(lengthBuffer, bytesRead, 4 - bytesRead,
                                _cancellationToken);
                            if (read == 0) throw new Exception("连接关闭");
                            bytesRead += read;
                        }

                        var totalLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                        // 读取类型+数据
                        var typeAndData = new byte[totalLength];
                        bytesRead = 0;
                        while (bytesRead < totalLength) {
                            int read = await _stream.ReadAsync(typeAndData, bytesRead, totalLength - bytesRead,
                                _cancellationToken);
                            if (read == 0) throw new Exception("连接关闭");
                            bytesRead += read;
                        }

                        byte msgType = typeAndData[0];
                        byte[] payload = new byte[totalLength - 1];
                        Array.Copy(typeAndData, 1, payload, 0, totalLength - 1);

                        if (msgType == 0x00) // 文本
                        {
                            string message = Encoding.UTF8.GetString(payload);
                            Debug.Log($"收到来自 {_clientId} 的消息: {message}");
                            _server._mainThreadContext.Post(_ => _server.OnMessageReceived?.Invoke(message), null);
                        } else if (msgType == 0x01) // 图像
                        {
                            Debug.Log($"收到来自 {_clientId} 的图像，大小={payload.Length}字节");
                            // 可根据需要扩展图像处理，例如触发事件
                        } else {
                            Debug.LogWarning($"未知消息类型: {msgType}");
                        }
                    }
                } catch (Exception ex) {
                    Debug.LogError($"客户端 {_clientId} 接收错误: {ex.Message}");
                }
                finally {
                    _server.RemoveClient(_clientId);
                    _tcpClient?.Close();
                }
            }

            /// <summary>
            /// 发送消息给客户端
            /// </summary>
            /// <param name="type">消息类型 0x00为文本消息 0x01为图像消息</param>
            /// <param name="data">消息数据</param>

            private async Task SendAsync(byte type, byte[] data) {
                await _sendLock.WaitAsync(_cancellationToken);
                try {
                    Debug.Log($"[SendAsync] 开始发送: 类型={type}, 数据长度={data.Length}");
                    var lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(1 + data.Length));
                    await _stream.WriteAsync(lengthPrefix, 0, 4, _cancellationToken);
                    await _stream.WriteAsync(new byte[] { type }, 0, 1, _cancellationToken);
                    await _stream.WriteAsync(data, 0, data.Length, _cancellationToken);
                    Debug.Log($"[SendAsync] 发送完成，总字节={1 + data.Length}");
                } finally {
                    _sendLock.Release();
                }
            }

            public async Task SendData(string message) => await SendAsync(0x00, Encoding.UTF8.GetBytes(message));

            public async Task SendImage(byte[] imageBytes) => await SendAsync(0x01, imageBytes);

            // 截图并发送的协程
            public IEnumerator CaptureAndSendScreenshot() {
                // 等待当前帧渲染结束，确保屏幕像素完整
                yield return new WaitForEndOfFrame();

                // 创建纹理并读取屏幕像素
                int width = Screen.width;
                int height = Screen.height;
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                // 编码为 JPG 字节数组（可选择 PNG）
                byte[] jpgBytes = tex.EncodeToJPG(75); // 质量 75，可调整
                Destroy(tex); // 释放纹理

                // 发送图像（异步任务）
                var sendTask = SendImage(jpgBytes);
                // 等待任务完成
                yield return new WaitUntil(() => sendTask.IsCompleted);
                if (sendTask.Exception != null) {
                    Debug.LogError($"[Capture] 发送任务异常: {sendTask.Exception}");
                } else {
                    Debug.Log("[Capture] 发送任务成功完成");
                }
            }

            public void Dispose() {
                _tcpClient?.Close();
                _stream?.Dispose();
            }
        }
    }
}