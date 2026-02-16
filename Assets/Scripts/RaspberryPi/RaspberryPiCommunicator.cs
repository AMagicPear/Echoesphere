using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RaspberryPi {
    public class RaspberryPiCommunicator : MonoBehaviour {
        [Header("Raspberry Pi Settings")] public string piWebSocketUri;

        public event Action<string> OnMessageReceived;

        private ClientWebSocket _webSocket;
        private bool _isConnected;
        private CancellationTokenSource _cancellationTokenSource;

        private void Start() {
            ConnectToPi();
        }

        private void OnDestroy() {
            Disconnect();
        }

        private void OnApplicationQuit() {
            Disconnect();
        }

        private async void ConnectToPi() {
            try {
                _webSocket = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();
                var uri = new Uri(piWebSocketUri);
                await _webSocket.ConnectAsync(uri, _cancellationTokenSource.Token);
                _isConnected = true;
                Debug.Log("WebSocket connected to: " + piWebSocketUri);
                await ReceiveMessages();
            } catch (Exception e) {
                Debug.LogError($"WebSocket connection error: {e.Message}");
            }
        }

        private async Task ReceiveMessages() {
            var buffer = new byte[1024];
            while (_isConnected) {
                try {
                    var result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cancellationTokenSource.Token
                    );
                    if (result.MessageType == WebSocketMessageType.Close) {
                        Debug.Log("Server closed the connection");
                        break;
                    }
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"Received: {message}");
                    OnMessageReceived?.Invoke(message);
                } catch (Exception e) {
                    if (_isConnected) {
                        Debug.LogError($"Message receive error: {e.Message}");
                    }
                    break;
                }
            }
            _isConnected = false;
            _webSocket?.Dispose();
            _webSocket = null;
        }

        public void SendData(string message) {
            if (!_isConnected || _webSocket == null) {
                Debug.LogWarning("WebSocket not connected, cannot send message");
                return;
            }
            var buffer = Encoding.UTF8.GetBytes(message);
            _webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                _cancellationTokenSource.Token
            );
            Debug.Log($"Sent: {message}");
        }

        private void Disconnect() {
            if (!_isConnected || _webSocket == null) return;
            _isConnected = false;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}