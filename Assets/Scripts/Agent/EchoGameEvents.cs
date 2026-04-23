using System.Collections;
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
        // 供游戏内物体订阅的内部事件
        public event System.Action<GameInternalEvent> OnGameplayEvent;
        // 智能体创建的虚拟按键
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
            Debug.Log($"[EventCenter] 接收到网络命令：{msg.data}");
            if (msg.data.StartsWith("input:")) {
                var actionName = msg.data.Split(':')[1];
                StartCoroutine(SimulatePress(actionName));
            }
        }

        public void DispatchInternalEvent(GameInternalEvent evt) {
            Debug.Log($"[EventCenter] 分发内部事件：{evt}");
            OnGameplayEvent?.Invoke(evt);
        }

        private IEnumerator SimulatePress(string actionName) {
            UpdateButtonState(actionName, true);
            InputSystem.QueueStateEvent(_virtualDevice, _currentState);
            yield return new WaitForSeconds(0.1f);
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