using DG.Tweening;
using Echoesphere.Runtime.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace Echoesphere.Runtime.UI.MusicNote {
    [RequireComponent(typeof(Image))]
    public class MusicNote : MonoBehaviour {
        [Header("Settings")] public NoteType noteType;
        [ColorUsage(false)] public Color offsetColor = Color.white;

        [Header("Connection Line")] [SerializeField]
        private Image connectionLine;

        [Header("Audio")] [SerializeField] private AudioClip playSound;

        private Image _noteImage;
        private Sequence _acquireSequence;
        private bool _isAcquired;
        public MusicNoteController controller;

        public bool IsAcquired => _isAcquired;

        private void Awake() {
            _noteImage = GetComponent<Image>();
            _noteImage.DOFade(0f, 0f);
            if (connectionLine != null) connectionLine.DOFade(0f, 0f);
        }

        private void OnDestroy() {
            _acquireSequence?.Kill();
        }

        public void Acquire() {
            if (_isAcquired) return;
            _isAcquired = true;

            SendCommand("gain_note");
            controller.PlayRandomAcquireSound();

            _acquireSequence?.Kill();
            _acquireSequence = DOTween.Sequence();

            if (noteType == NoteType.WaterDrop) {
                _acquireSequence
                    .Append(_noteImage.DOFade(1f, controller.fadeDuration).SetEase(Ease.OutQuad))
                    .Join(BounceScale())
                    .Append(_noteImage.DOFade(controller.acquiredAlpha, controller.fadeDuration).SetEase(Ease.OutQuad));
            } else {
                _acquireSequence
                    .Append(connectionLine.DOFade(1f, controller.connectionLineFadeDuration).SetEase(Ease.OutQuad))
                    .Append(_noteImage.DOFade(1f, controller.fadeDuration).SetEase(Ease.OutQuad))
                    .Join(BounceScale())
                    .Append(_noteImage.DOFade(controller.acquiredAlpha, controller.fadeDuration).SetEase(Ease.OutQuad))
                    .Join(connectionLine.DOFade(controller.acquiredAlpha, controller.fadeDuration).SetEase(Ease.OutQuad));
            }
        }

        public void Play() {
            if (!_isAcquired) return;
            SendCommand("play_note");

            var originalColor = _noteImage.color;
            DOTween.Sequence()
                .Append(_noteImage.DOColor(offsetColor, controller.playPulseDuration))
                .Append(_noteImage.DOColor(originalColor, controller.playPulseDuration));

            controller.PlaySound(playSound);
        }

        public void Reset() {
            _isAcquired = false;

            _acquireSequence?.Kill();

            _noteImage.DOFade(0f, 0f);
            if (connectionLine != null) connectionLine.DOFade(0f, 0f);
            transform.localScale = Vector3.one;
        }

        private void SendCommand(string command) {
            Debug.Log($"[音符] 发送命令: {command}");
            GameRoot.Instance?.agentCommunicator.SendCommand(
                    $"{command}:{noteType.ToCommandName()}", relayTo: "raspberry_pi")
                .ContinueWith(t => {
                    if (t.IsFaulted) Debug.LogError($"[音符] {command} 发送失败: {t.Exception}");
                });
        }

        private Tween BounceScale() {
            return DOTween.Sequence()
                .Append(transform.DOScale(controller.acquireScale, controller.acquireScaleDuration))
                .Append(transform.DOScale(1f, controller.acquireScaleDuration));
        }
    }
}