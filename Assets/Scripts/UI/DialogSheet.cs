using System;
using Echoesphere.Runtime.Configuration.Providers;
using JetBrains.Annotations;
using UnityEngine;

namespace Echoesphere.Runtime.UI {
    [Serializable]
    public record Dialog {
        public string text;
        [CanBeNull] public AudioClip audioClip;
        public float duration; // 新增：持续时间（秒），<= 0 表示不自动隐藏
    }

    [CreateAssetMenu(fileName = "Dialog", menuName = "Dialog", order = 0)]
    public class DialogSheet : ScriptableObject {
        public ChapterState chapterState;
        public Dialog[] dialogs;
    }
}