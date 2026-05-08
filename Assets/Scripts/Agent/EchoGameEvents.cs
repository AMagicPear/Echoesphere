using System.Collections;
using System.Globalization;
using Echoesphere.Runtime.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Echoesphere.Runtime.Agent {
    public enum GameInternalEvent {
        None,
        PlayNoteWaterDrop,
        PlayNoteCrossing,
        PlayNoteTide,
        PlayNoteBreeze,
        ObtainNoteWaterDrop,
        ObtainNoteCrossing,
        ObtainNoteTide,
        ObtainNoteBreeze,
    }

    public class EchoGameEvents : MonoBehaviour {
        private static readonly WaitForSeconds WaitForSeconds01 = new(0.1f);
        private static readonly WaitForSeconds WaitForSeconds02 = new(0.2f);

        private VirtualAgentDevice _virtualDevice;
        private VirtualAgentState _currentState;
        private float _lastMoveTime;

        private void Awake() {
            _virtualDevice = InputSystem.AddDevice<VirtualAgentDevice>("VirtualAgent");
        }

        private void OnEnable() {
            GameRoot.Instance.agentCommunicator.OnCommandReceived += HandleNetWorkCommand;
            StartCoroutine(CheckMoveTimeout());
        }

        private void OnDisable() {
            GameRoot.Instance.agentCommunicator.OnCommandReceived -= HandleNetWorkCommand;
            StopCoroutine(CheckMoveTimeout());
        }

        private IEnumerator CheckMoveTimeout() {
            while (true) {
                yield return WaitForSeconds02;
                if (Time.time - _lastMoveTime > 0.3f && _currentState.move != Vector2.zero) {
                    _currentState.move = Vector2.zero;
                    InputSystem.QueueStateEvent(_virtualDevice, _currentState);
                }
            }
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
            if (axis.Length != 2) return;
            if (!float.TryParse(axis[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
                !float.TryParse(axis[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) return;
            _currentState.move = new Vector2(x, y);
            _lastMoveTime = Time.time;
            InputSystem.QueueStateEvent(_virtualDevice, _currentState);
        }

        private IEnumerator SimulatePress(string actionName) {
            UpdateButtonState(actionName, true);
            InputSystem.QueueStateEvent(_virtualDevice, _currentState);
            yield return WaitForSeconds01;
            UpdateButtonState(actionName, false);
            InputSystem.QueueStateEvent(_virtualDevice, _currentState);
        }

        private void UpdateButtonState(string action, bool isPressed) {
            uint bitMask = action switch {
                "submit" => 1u << 0,
                "cancel" => 1u << 1,
                "navigate" => 1u << 2,
                _ => 0
            };

            if (isPressed) _currentState.buttons |= bitMask;
            else _currentState.buttons &= ~bitMask;
        }

        private void OnDestroy() {
            if (_virtualDevice != null) {
                InputSystem.RemoveDevice(_virtualDevice);
            }
        }
    }
}