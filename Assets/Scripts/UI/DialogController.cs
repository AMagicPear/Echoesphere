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
            if (_audioSource == null) {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            if (dialogCanvas != null) {
                dialogCanvas.alpha = 0f;
                dialogCanvas.blocksRaycasts = false;
            }
        }

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

        public void Hide() {
            _fadeTween?.Kill();
            _autoHideTween?.Kill();
            _audioSource?.Stop();

            if (dialogCanvas != null) {
                _fadeTween = dialogCanvas.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() => {
                    dialogCanvas.blocksRaycasts = false;
                    _currentIndex = -1;
                });
            } else {
                _currentIndex = -1;
            }
        }

        public bool IsShowing => _currentIndex >= 0 && (dialogCanvas == null || dialogCanvas.alpha > 0f);
    }
}