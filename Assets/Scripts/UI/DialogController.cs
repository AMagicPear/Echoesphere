using DG.Tweening;
using TMPro;
using UnityEngine;

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
            dialogCanvas.alpha = 0f;
            dialogCanvas.blocksRaycasts = false;
        }

        public void Show(int dialogIndex) {
            if (dialogIndex < 0 || dialogIndex >= dialogSheet.dialogs.Length) return;
            var dialog = dialogSheet.dialogs[dialogIndex];
            _currentIndex = dialogIndex;
            _fadeTween?.Kill();
            _autoHideTween?.Kill();
            dialogText.text = dialog.text;
            _audioSource.clip = dialog.audioClip;
            _audioSource.Play();
            dialogCanvas.blocksRaycasts = true;
            _fadeTween = dialogCanvas.DOFade(1f, 0.3f).SetUpdate(true);
            if (dialog.autoHide && dialog.audioClip != null) {
                _autoHideTween = DOVirtual.DelayedCall(dialog.audioClip.length, Hide, false);
            }
        }

        public void Hide() {
            _fadeTween?.Kill();
            _autoHideTween?.Kill();
            _audioSource?.Stop();
            _fadeTween = dialogCanvas.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() => {
                dialogCanvas.blocksRaycasts = false;
                _currentIndex = -1;
            });
        }

        public bool IsShowing => _currentIndex >= 0 && dialogCanvas.alpha > 0f;
    }
}