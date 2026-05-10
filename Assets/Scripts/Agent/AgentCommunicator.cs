using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;


namespace Echoesphere.Runtime.Agent {
    [Serializable]
    public class JsonMessage {
        public string type; // text, image, command, register
        public string data; // 文本内容或base64编码数据
        [JsonProperty("client_type")] public string clientType; // 发送者身份: echoagent, unity, mediapipe, raspberry_pi
        [JsonProperty("request_id")] public string requestId; // 请求标识UUID
        [JsonProperty("relay_to")] public string relayTo; // 直接转发目标客户端
    }
    
    [DefaultExecutionOrder(-98)] 
    public class AgentCommunicator : MonoBehaviour {
        [Header("服务器设置")] public string host = "127.0.0.1";
        [Header("服务器端口")] public int port = 65432;

        public event Action<string> OnMessageReceived;
        public event Action<string> OnImageReceived;
        public event Action<JsonMessage> OnCommandReceived;
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

        private void OnEnable() {
            OnCommandReceived += HandleScreenshot;
        }

        private void OnDisable() {
            OnCommandReceived -= HandleScreenshot;
        }

        private void HandleScreenshot(JsonMessage msg) {
            if (msg.data == "request:screenshot" && !string.IsNullOrEmpty(msg.requestId)) {
                StartCoroutine(SendScreenshot(msg.requestId));
            }
        }

        private async Task ConnectAsync() {
            try {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port);
                _stream = _tcpClient.GetStream();
                _isConnected = true;
                Debug.Log($"[客户端] 已连接至 {host}:{port}");

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
                clientType = "unity",
                data = "这是来自Unity客户端的注册消息"
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

                    var data = new byte[totalLength];
                    bytesRead = 0;
                    while (bytesRead < totalLength) {
                        int read = await _stream.ReadAsync(data, bytesRead, totalLength - bytesRead, _cts.Token);
                        if (read == 0) throw new Exception("连接关闭");
                        bytesRead += read;
                    }

                    string jsonStr = Encoding.UTF8.GetString(data);
                    Debug.Log($"[收到] {jsonStr}");

                    var msg = JsonConvert.DeserializeObject<JsonMessage>(jsonStr);
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
                                Debug.Log($"[收到图片] request_id={msg.requestId}, data长度={msg.data.Length}");
                                _mainThreadContext.Post(_ => OnImageReceived?.Invoke(msg.data), null);
                            }

                            break;
                        case "command":
                            _mainThreadContext.Post(_ => OnCommandReceived?.Invoke(msg), null);
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
            string json = JsonConvert.SerializeObject(msg);
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
            }
            finally {
                _sendLock.Release();
            }
        }

        public Task SendText(string message) {
            var msg = new JsonMessage { type = "text", data = message };
            return SendJson(msg);
        }

        private Task SendImage(string base64Image, string requestId = null) {
            var msg = new JsonMessage {
                type = "image",
                data = base64Image,
                requestId = string.IsNullOrEmpty(requestId) ? null : requestId
            };
            return SendJson(msg);
        }

        public Task SendCommand(string command, string requestId = null, string relayTo = null) {
            var msg = new JsonMessage {
                type = "command",
                data = command,
                requestId = string.IsNullOrEmpty(requestId) ? null : requestId,
                relayTo = string.IsNullOrEmpty(relayTo) ? null : relayTo
            };
            return SendJson(msg);
        }

        /// <summary> 发送截图，携带 request_id 响应截图请求 </summary>
        public IEnumerator SendScreenshot(string requestId) {
            yield return new WaitForEndOfFrame();
            var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            var jpgBytes = screenshot.EncodeToJPG();
            var base64Image = Convert.ToBase64String(jpgBytes);

            // 保存截图到本地缓存（异步IO避免卡顿）
            SaveScreenshotCache(jpgBytes, requestId);

            var sendTask = SendImage(base64Image, requestId);
            yield return new WaitUntil(() => sendTask.IsCompleted);
            if (sendTask.Exception != null) {
                Debug.LogError($"[截图异常] {sendTask.Exception}");
            } else {
                Debug.Log($"[截图] 发送完成, request_id={requestId}, base64长度={base64Image.Length}");
            }
        }

        private static async void SaveScreenshotCache(byte[] jpgBytes, string requestId) {
            try {
                var cacheDir = Application.persistentDataPath + "/ScreenshotCache";
                if (!Directory.Exists(cacheDir)) {
                    Directory.CreateDirectory(cacheDir);
                }

                var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}_{requestId ?? "nocid"}.jpg";
                var filePath = cacheDir + "/" + fileName;
                await File.WriteAllBytesAsync(filePath, jpgBytes);
                Debug.Log($"[截图] 已缓存到本地: {filePath}");
            } catch (Exception e) {
                Debug.LogError($"[截图缓存异常] {e.Message}");
            }
        }

        private void Disconnect() {
            _isConnected = false;
            _stream?.Dispose();
            _tcpClient?.Dispose();
            _mainThreadContext.Post(_ => OnConnectionStatusChanged?.Invoke(false), null);
            Debug.Log("[客户端] 已断开连接");
        }

        private void OnDestroy() {
            _isConnected = false;
            _cts?.Cancel();
            _stream?.Dispose();
            _tcpClient?.Dispose();
            _sendLock?.Dispose();
        }
    }
}