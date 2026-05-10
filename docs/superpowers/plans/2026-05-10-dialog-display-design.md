# Dialog 显示/隐藏功能实现计划

**目标：** 实现 DialogController 的显示/隐藏功能，支持 CanvasGroup 透明度渐变、音频播放和自动/手动隐藏。

**架构：** DialogController 挂载在场景中，引用 DialogSheet（ScriptableObject）作为数据源。通过 CanvasGroup 控制显隐和透明度，支持音频播放和自动隐藏计时器。

**技术栈：** Unity 6000.3.9f1, DOTween, TMPro

---

### 任务 1: 更新 DialogSheet 添加持续时间字段

**文件：**
- 修改: `Assets/Scripts/UI/DialogSheet.cs`

- [ ] **步骤 1: 添加 duration 字段到 Dialog record**

```csharp
[Serializable]
public record Dialog {
    public string text;
    [CanBeNull] public AudioClip audioClip;
    public float duration; // 新增：持续时间（秒），<= 0 表示不自动隐藏
}
```

- [ ] **步骤 2: 提交**

```bash
git add Assets/Scripts/UI/DialogSheet.cs
git commit -m "feat: add duration field to Dialog for auto-hide support"
```

---

### 任务 2: 实现 DialogController 显示功能

**文件：**
- 修改: `Assets/Scripts/UI/DialogController.cs`

- [ ] **步骤 1: 添加所需 using 语句和字段**

```csharp
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Echoesphere.Runtime.UI {
    public class DialogController : MonoBehaviour {
        public CanvasGroup dialogCanvas;
        public TMP_Text dialogText;
        public DialogSheet dialogSheet;

        private int _currentIndex = -1;
        private Tween _fadeTween;
        private Tween _autoHideTween;
        private AudioSource _audioSource;

        private void Awake() {
            _audioSource = gameObject.GetComponent<AudioSource>();
            if (_audioSource == null) {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            if (dialogCanvas != null) {
                dialogCanvas.alpha = 0f;
                dialogCanvas.blocksRaycasts = false;
            }
        }
    }
}
```

- [ ] **步骤 2: 实现 Show 方法**

```csharp
public void Show(int dialogIndex) {
    if (dialogSheet == null || dialogSheet.dialogs == null) return;
    if (dialogIndex < 0 || dialogIndex >= dialogSheet.dialogs.Length) return;

    var dialog = dialogSheet.dialogs[dialogIndex];
    _currentIndex = dialogIndex;

    _fadeTween?.Kill();
    _autoHideTween?.Kill();

    if (dialogText != null) {
        dialogText.text = dialog.text;
    }

    if (dialog.audioClip != null) {
        _audioSource.clip = dialog.audioClip;
        _audioSource.Play();
    }

    if (dialogCanvas != null) {
        dialogCanvas.blocksRaycasts = true;
        _fadeTween = dialogCanvas.DOFade(1f, 0.3f).SetUpdate(true);
    }

    if (dialog.duration > 0f) {
        _autoHideTween = DOVirtual.DelayedCall(dialog.duration, Hide, false);
    }
}
```

- [ ] **步骤 3: 实现 Hide 方法**

```csharp
public void Hide() {
    _fadeTween?.Kill();
    _autoHideTween?.Kill();
    _audioSource?.Stop();

    if (dialogCanvas != null) {
        _fadeTween = dialogCanvas.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() => {
            dialogCanvas.blocksRaycasts = false;
        });
    }

    _currentIndex = -1;
}
```

- [ ] **步骤 4: 实现 IsShowing 属性**

```csharp
public bool IsShowing => _currentIndex >= 0 && (dialogCanvas == null || dialogCanvas.alpha > 0f);
```

- [ ] **步骤 5: 提交**

```bash
git add Assets/Scripts/UI/DialogController.cs
git commit -m "feat: implement DialogController show/hide with fade animation"
```

---

### 任务 3: 更新 TheGrass 场景引用

**文件：**
- 修改: `Assets/Scenes/TheGrass.unity`（如果 DialogController 已挂载到场景，需要在 Unity Editor 中配置引用）

- [ ] 在 Unity Editor 中确保 DialogController 组件正确引用：
  - dialogCanvas: 拖拽 CanvasGroup 到 Inspector
  - dialogText: 拖拽 TMP_Text 到 Inspector
  - dialogSheet: 拖拽 DialogSheet Asset 到 Inspector

---

### 任务 4: 验证

- [ ] 在 Editor 中运行场景
- [ ] 测试 Show(0) 后 CanvasGroup alpha 从 0 渐变到 1
- [ ] 测试 duration > 0 时自动隐藏
- [ ] 测试 duration <= 0 时需要手动调用 Hide()
- [ ] 测试音频播放（如果有 AudioClip）