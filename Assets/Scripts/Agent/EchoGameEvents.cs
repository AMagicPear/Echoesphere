using Echoesphere.Runtime.Configuration;
using UnityEngine;

namespace Echoesphere.Runtime.Agent {
    public enum GameInternalEvent {
        None,
        NoteWaterDrop,
        NoteCrossing,
        NoteTide,
        NoteBreeze,
    }

    public class EchoEventCenter: MonoBehaviour {
        // 供游戏内物体订阅的最终事件
        public event System.Action<GameInternalEvent> OnGameplayEvent;

        private void Start() {
            // 订阅网络端的原始命令
            GameRoot.Instance.agentCommunicator.OnCommandReceived += HandleNetWorkCommand;

            // 订阅本地按键（这里假设你已经通过 Input System 绑定了逻辑）
            // 例如：LocalInput.OnKey1Pressed += () => dispatch_internal_event(GameInternalEvent.EchoTitlePulse);
        }

        private void HandleNetWorkCommand(JsonMessage msg) {
            Debug.Log($"[EventCenter] 接收到网络命令：{msg.data}");
            if (msg.data == "debug4.22") {

            }
        }

        public void DispatchInternalEvent(GameInternalEvent evt) {
            Debug.Log($"[EventCenter] 分发内部事件：{evt}");
            OnGameplayEvent?.Invoke(evt);
        }
    }
}