using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Echoesphere.Runtime.Agent {
    // 定义设备的状态布局
    public struct VirtualAgentState : IInputStateTypeInfo {
        // 这是一个唯一的格式标识符
        public FourCC format => new FourCC('V', 'A', 'G', 'T');

        // 定义按键位（对应你的 input:submit, input:cancel 等）
        [InputControl(name = "submit", layout = "Button", bit = 0)]
        [InputControl(name = "cancel", layout = "Button", bit = 1)]
        [InputControl(name = "navigate", layout = "Button", bit = 2)]
        public uint buttons;
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    // 告诉系统这个设备使用我们刚才定义的 State
    [InputControlLayout(stateType = typeof(VirtualAgentState))]
    public class VirtualAgentDevice : InputDevice {
        static VirtualAgentDevice() {
            // 自动注册设备布局
            InputSystem.RegisterLayout<VirtualAgentDevice>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void initialize() => InputSystem.RegisterLayout<VirtualAgentDevice>();

        // 方便在代码中快速找到我们的虚拟按键
        public ButtonControl submit_button { get; private set; }
        public ButtonControl cancel_button { get; private set; }

        protected override void FinishSetup() {
            base.FinishSetup();
            submit_button = GetChildControl<ButtonControl>("submit");
            cancel_button = GetChildControl<ButtonControl>("cancel");
        }
    }
}