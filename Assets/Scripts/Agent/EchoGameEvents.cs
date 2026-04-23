using System.Collections;
using System.Globalization;
using Echoesphere.Runtime.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Echoesphere.Runtime.Agent {
    public enum GameInternalEvent {
        None,
        PlayNote_WaterDrop,
        PlayNote_Crossing,
        PlayNote_Tide,
        PlayNote_Breeze,
        ObtainNote_WaterDrop,
        ObtainNote_Crossing,
        ObtainNote_Tide,
        ObtainNote_Breeze,
    }

    public class EchoEventCenter : MonoBehaviour {
        private static readonly WaitForSeconds _waitForSeconds0_1 = new(0.1f);

        public event System.Action<GameInternalEvent> OnGameplayEvent;
        private VirtualAgentDevice _virtualDevice;
        private VirtualAgentState _currentState;

        void Awake() {
            _virtualDevice = InputSystem.AddDevice<VirtualAgentDevice>("VirtualAgent");
        }

        void OnEnable() {
            GameRoot.Instance.agentCommunicator.OnCommandReceived += HandleNetWorkCommand;
        }

        void OnDisable() {
            GameRoot.Instance.agentCommunicator.OnCommandReceived -= HandleNetWorkCommand;
        }

        private void HandleNetWorkCommand(JsonMessage msg) {
            if (!msg.data.StartsWith("input:")) return;

            string[] parts = msg.data.Split(':');
            if (parts.Length < 2) return;

            string actionName = parts[1];

            // 逻辑分支：处理摇杆
            if (actionName == "move" && parts.Length == 3) {
                HandleStickInput(parts[2]);
            }
            // 逻辑分支：处理普通按键
            else {
                StartCoroutine(SimulatePress(actionName));
            }
        }

        // 解析 x,y 坐标并更新摇杆状态
        private void HandleStickInput(string coordinates) {
            string[] axis = coordinates.Split(',');
            if (axis.Length == 2) {
                if (float.TryParse(axis[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(axis[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y)) {

                    _currentState.move = new Vector2(x, y);
                    InputSystem.QueueStateEvent(_virtualDevice, _currentState);
                    Debug.Log($"[EventCenter] 摇杆更新：X={x}, Y={y}");
                }
            }
        }

        public void DispatchInternalEvent(GameInternalEvent evt) {
            OnGameplayEvent?.Invoke(evt);
        }

        private IEnumerator SimulatePress(string actionName) {
            UpdateButtonState(actionName, true);
            InputSystem.QueueStateEvent(_virtualDevice, _currentState);
            yield return _waitForSeconds0_1;
            UpdateButtonState(actionName, false);
            InputSystem.QueueStateEvent(_virtualDevice, _currentState);
        }

        private void UpdateButtonState(string action, bool is_pressed) {
            uint bit_mask = action switch {
                "submit" => 1u << 0,
                "cancel" => 1u << 1,
                "navigate" => 1u << 2,
                _ => 0
            };

            if (is_pressed) _currentState.buttons |= bit_mask;
            else _currentState.buttons &= ~bit_mask;
        }

        private void OnDestroy() {
            if (_virtualDevice != null) {
                InputSystem.RemoveDevice(_virtualDevice);
            }
        }
    }
}