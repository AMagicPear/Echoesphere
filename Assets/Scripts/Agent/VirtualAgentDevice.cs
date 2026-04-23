using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Echoesphere.Runtime.Agent {
    public struct VirtualAgentState : IInputStateTypeInfo {
        public FourCC format => new FourCC('V', 'A', 'G', 'T');

        [InputControl(name = "submit", layout = "Button", bit = 0)]
        [InputControl(name = "cancel", layout = "Button", bit = 1)]
        [InputControl(name = "navigate", layout = "Button", bit = 2)]
        public uint buttons;

        // 增加摇杆控制项，layout 设为 Stick
        [InputControl(name = "move", layout = "Stick", usage = "Primary2DMotion")]
        public Vector2 move; 
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    [InputControlLayout(stateType = typeof(VirtualAgentState))]
    public class VirtualAgentDevice : InputDevice {
        static VirtualAgentDevice() {
            InputSystem.RegisterLayout<VirtualAgentDevice>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void initialize() => InputSystem.RegisterLayout<VirtualAgentDevice>();

        public ButtonControl submit_button { get; private set; }
        public ButtonControl cancel_button { get; private set; }
        // 增加摇杆引用
        public StickControl move_stick { get; private set; }

        protected override void FinishSetup() {
            base.FinishSetup();
            submit_button = GetChildControl<ButtonControl>("submit");
            cancel_button = GetChildControl<ButtonControl>("cancel");
            move_stick = GetChildControl<StickControl>("move");
        }
    }
}