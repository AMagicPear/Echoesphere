using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace RaspberryPi {
    public enum CommunicateType {
        None,
        HitBlockColor,
        Init,
    }

    public class RaspberryPiCommunicator : MonoBehaviour {
        [Header("Raspberry Pi Settings")] public string piIpAddress;
        public ushort port;

        private TcpClient _client;
        private NetworkStream _stream;
        private bool _connected;
        private Thread _receiveThread;

        private void Awake() {
            ConnectToPi();
        }

        private void OnDestroy() {
            if (!_connected) return;
            _connected = false;
            _stream.Close();
            _client.Close();
            _receiveThread.Join();
        }

        private void Start() {
            SendData(CommunicateType.Init, $"Raspberry Pi {piIpAddress}:{port}");
        }

        private void ConnectToPi() {
            try {
                _client = new TcpClient();
                _client.Connect(piIpAddress, port);
                _stream = _client.GetStream();
                _connected = true;
                // 启动接收线程
                _receiveThread = new Thread(ReceiveData) {
                    IsBackground = true
                };
                _receiveThread.Start();
                Debug.Log("Connected to Raspberry Pi successfully!");
            } catch (SocketException e) {
                Debug.LogError($"Failed to connect to Raspberry Pi: {e.Message}");
            }
        }

        /// 接收玩家输入数据
        private void ReceiveData() {
            byte[] buffer = new byte[1024];
            while (_connected) {
                try {
                    if (_stream.DataAvailable) {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead <= 0) continue;
                        string receivedData = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Debug.Log($"Received data from Raspberry Pi: {receivedData}");
                    } else {
                        Thread.Sleep(10);
                    }
                } catch (SocketException e) {
                    Debug.LogError($"Failed to receive data from Raspberry Pi: {e.Message}");
                    _connected = false;
                }
            }
        }

        public void SendData(CommunicateType communicateType, object data) {
            if (!_connected) return;
            Debug.Log($"Sending data to Raspberry Pi: {communicateType}:{data}");
            var buffer = System.Text.Encoding.UTF8.GetBytes($"{{\"{communicateType}\":\"{data}\"}}");
            _stream.Write(buffer, 0, buffer.Length);
        }
    }
}