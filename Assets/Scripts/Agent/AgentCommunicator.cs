using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Echoesphere.Runtime.Agent {
    public class AgentCommunicator : MonoBehaviour {
        [Header("服务器设置")] public string host = "127.0.0.1";
        [Header("服务器端口")] public int port = 65432;

        public event Action<string> OnMessageReceived;
        public event Action<string> OnImageReceived;
        public event Action<bool> OnConnectionStatusChanged;

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

                // 发送注册消息
                await SendRegister();

                _mainThreadContext.Post(_ => OnConnectionStatusChanged?.Invoke(true), null);
                _ = ReceiveLoopAsync();
            } catch (Exception ex) {
                Debug.LogError($"[客户端] 连接失败: {ex.Message}");
                _mainThreadContext.Post(_ => OnConnectionStatusChanged?.Invoke(false), null);
                gameObject.SetActive(false);
            }
        }

        private async Task SendRegister() {
            var registerMsg = new JsonMessage {
                type = "register",
                client_type = "unity"
            };
            await SendJson(registerMsg);
            Debug.Log($"[客户端] 已发送注册消息");
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

                    var jsonBytes = new byte[totalLength];
                    bytesRead = 0;
                    while (bytesRead < totalLength) {
                        int read = await _stream.ReadAsync(jsonBytes, bytesRead, totalLength - bytesRead, _cts.Token);
                        if (read == 0) throw new Exception("连接关闭");
                        bytesRead += read;
                    }

                    string jsonString = Encoding.UTF8.GetString(jsonBytes);
                    Debug.Log($"[收到] {jsonString}");

                    var msg = JsonUtility.FromJson<JsonMessage>(jsonString);
                    if (msg == null) {
                        Debug.LogWarning("[解析] 无法解析消息");
                        continue;
                    }

                    switch (msg.type) {
                        case "text":
                            if (!string.IsNullOrEmpty(msg.request_id)) {
                                Debug.Log($"[收到文本响应] request_id={msg.request_id}, data={msg.data}");
                            } else {
                                Debug.Log($"[收到文本] {msg.data}");
                                _mainThreadContext.Post(_ => OnMessageReceived?.Invoke(msg.data), null);
                            }
                            break;
                        case "image":
                            Debug.Log($"[收到图片] request_id={msg.request_id}, data长度={msg.data?.Length ?? 0}");
                            _mainThreadContext.Post(_ => OnImageReceived?.Invoke(msg.data), null);
                            break;
                        case "command":
                            Debug.Log($"[收到命令] {msg.data}, request_id={msg.request_id}");
                            if (msg.data == "request_screenshot" && !string.IsNullOrEmpty(msg.request_id)) {
                                _ = SendScreenshot(msg.request_id);
                            }
                            break;
                        case "register":
                            Debug.Log($"[收到注册] client_type={msg.client_type}");
                            break;
                        default:
                            Debug.LogWarning($"[未知] 消息类型 {msg.type}");
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

        private async Task SendJson(JsonMessage msg) {
            if (!_isConnected || _stream == null) return;
            await _sendLock.WaitAsync(_cts.Token);
            try {
                string jsonString = JsonUtility.ToJson(msg);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                var lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(jsonBytes.Length));
                await _stream.WriteAsync(lengthPrefix, 0, 4, _cts.Token);
                await _stream.WriteAsync(jsonBytes, 0, jsonBytes.Length, _cts.Token);
                Debug.Log($"[发送] {jsonString}");
            }
            catch (Exception ex) {
                Debug.LogError($"[发送错误] {ex.Message}");
            }
            finally {
                _sendLock.Release();
            }
        }

        public Task SendText(string message) {
            var msg = new JsonMessage {
                type = "text",
                data = message
            };
            return SendJson(msg);
        }

        public Task SendImage(string base64Image, string requestId = null) {
            var msg = new JsonMessage {
                type = "image",
                data = base64Image,
                request_id = requestId
            };
            return SendJson(msg);
        }

        public Task SendCommand(string command, string requestId) {
            var msg = new JsonMessage {
                type = "command",
                data = command,
                request_id = requestId
            };
            return SendJson(msg);
        }

        /// <summary> 发送截图，携带 request_id 响应截图请求 </summary>
        public async Task SendScreenshot(string requestId) {
            await Task.Delay(1); // 等待当前帧结束，确保 ScreenCapture 能获取正确画面
            var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            var base64Image = Convert.ToBase64String(screenshot.EncodeToJPG());
            try {
                await SendImage(base64Image, requestId);
                Debug.Log($"[截图] 发送完成, request_id={requestId}, base64长度={base64Image.Length}");
            } catch (Exception ex) {
                Debug.LogError($"[截图异常] {ex.Message}");
            }
        }

        private void Disconnect() {
            _isConnected = false;
            _stream?.Close();
            _tcpClient?.Close();
            _mainThreadContext.Post(_ => OnConnectionStatusChanged?.Invoke(false), null);
            Debug.Log("[客户端] 已断开连接");
        }

        private void OnDestroy() {
            _isConnected = false;
            _cts?.Cancel();
            _stream?.Dispose();
            _tcpClient?.Close();
            _sendLock?.Dispose();
        }

        // 统一JSON消息结构
        [Serializable]
        private class JsonMessage {
            public string type;       // text, image, command, register
            public string data;       // 文本内容或base64编码数据
            public string client_type; // 发送者身份: echoagent, unity, mediapipe, raspberry_pi
            public string request_id;  // 请求标识UUID
        }
    }
}