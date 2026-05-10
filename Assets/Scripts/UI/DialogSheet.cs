using System;
using Echoesphere.Runtime.Configuration.Providers;
using JetBrains.Annotations;
using UnityEngine;

namespace Echoesphere.Runtime.UI {
    [Serializable]
    public record Dialog {
        public string text;
        [CanBeNull] public AudioClip audioClip;
        public bool autoHide; // 是否在音频播放完后自动隐藏
    }

    [CreateAssetMenu(fileName = "Dialog", menuName = "Dialog", order = 0)]
    public class DialogSheet : ScriptableObject {
        public ChapterState chapterState;
        public Dialog[] dialogs;
    }
}