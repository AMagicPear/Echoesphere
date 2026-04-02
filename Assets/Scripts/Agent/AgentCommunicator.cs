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
        public event Action<byte[]> OnImageReceived;
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
            Debug.Log($"[客户端] 已发送注册消息: {JsonUtility.ToJson(registerMsg)}");
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

                    var data = new byte[totalLength];
                    bytesRead = 0;
                    while (bytesRead < totalLength) {
                        int read = await _stream.ReadAsync(data, bytesRead, totalLength - bytesRead, _cts.Token);
                        if (read == 0) throw new Exception("连接关闭");
                        bytesRead += read;
                    }

                    string jsonStr = Encoding.UTF8.GetString(data);
                    Debug.Log($"[收到] {jsonStr}");

                    var msg = JsonUtility.FromJson<JsonMessage>(jsonStr);
                    if (msg == null || string.IsNullOrEmpty(msg.type)) {
                        Debug.LogWarning("[收到] 无法解析的JSON消息");
                        continue;
                    }

                    switch (msg.type) {
                        case "text":
                            _mainThreadContext.Post(_ => OnMessageReceived?.Invoke(msg.data ?? jsonStr), null);
                            break;
                        case "image":
                            if (!string.IsNullOrEmpty(msg.data)) {
                                try {
                                    byte[] imageBytes = Convert.FromBase64String(msg.data);
                                    Debug.Log($"[收到] 图像，大小={imageBytes.Length}字节");
                                    _mainThreadContext.Post(_ => OnImageReceived?.Invoke(imageBytes), null);
                                } catch (Exception ex) {
                                    Debug.LogError($"[收到] Base64解码图像失败: {ex.Message}");
                                }
                            }
                            break;
                        case "command":
                            _mainThreadContext.Post(_ => OnMessageReceived?.Invoke(msg.data ?? jsonStr), null);
                            break;
                        case "request":
                            if (msg.cmd == "request_screenshot") {
                                StartCoroutine(SendScreenshot(msg.request_id));
                            }
                            break;
                        default:
                            Debug.LogWarning($"[收到] 未知消息类型: {msg.type}");
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
            string json = JsonUtility.ToJson(msg);
            byte[] data = Encoding.UTF8.GetBytes(json);
            await SendRawAsync(data);
        }

        private async Task SendRawAsync(byte[] data) {
            if (!_isConnected || _stream == null) return;
            await _sendLock.WaitAsync(_cts.Token);
            try {
                var lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
                await _stream.WriteAsync(lengthPrefix, 0, 4, _cts.Token);
                await _stream.WriteAsync(data, 0, data.Length, _cts.Token);
                Debug.Log($"[发送] 长度={data.Length}");
            }
            finally {
                _sendLock.Release();
            }
        }

        public Task SendText(string message) {
            var msg = new JsonMessage { type = "text", data = message };
            return SendJson(msg);
        }

        public Task SendImage(byte[] imageBytes) {
            var msg = new JsonMessage { type = "image", data = Convert.ToBase64String(imageBytes) };
            return SendJson(msg);
        }

        public Task SendCommand(string commandJson) {
            var msg = JsonUtility.FromJson<JsonMessage>(commandJson);
            if (msg != null) {
                msg.type = "command";
                return SendJson(msg);
            }
            var fallback = new JsonMessage { type = "command", data = commandJson };
            return SendJson(fallback);
        }

        /// <summary> 发送截图，可选带上 request_id 作为响应 </summary>
        public IEnumerator SendScreenshot(string requestId = null) {
            yield return new WaitForEndOfFrame();

            int width = Screen.width;
            int height = Screen.height;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            byte[] jpgBytes = tex.EncodeToJPG(75);
            Destroy(tex);

            Task sendTask;
            if (!string.IsNullOrEmpty(requestId)) {
                var msg = new JsonMessage {
                    type = "response",
                    request_id = requestId,
                    data = Convert.ToBase64String(jpgBytes)
                };
                sendTask = SendJson(msg);
                Debug.Log($"[截图响应] request_id={requestId}");
            } else {
                sendTask = SendImage(jpgBytes);
                Debug.Log("[截图] 发送完成");
            }

            yield return new WaitUntil(() => sendTask.IsCompleted);

            if (sendTask.Exception != null)
                Debug.LogError($"[截图异常] {sendTask.Exception}");
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
            public string type;          // text | image | command | register | request | response
            public string data;          // 文本内容或base64编码数据
            public string client_type;  // register 时使用
            public string request_id;    // request/response 时使用
            public string cmd;           // request 时使用
        }
    }
}