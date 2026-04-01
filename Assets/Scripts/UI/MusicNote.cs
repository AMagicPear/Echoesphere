using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Echoesphere.Runtime.UI {
    public enum NoteType {
        WaterDrop,
        Crossing,
        Tide,
        Breeze
    }

    public class MusicNote : MonoBehaviour {
        [Header("Settings")]
        public NoteType noteType;

        [Header("Connection Line")]
        [Tooltip("音符上方对应的连接线（道路交错、海浪、风需要）")]
        [SerializeField] private Image connectionLine;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.4f;
        [SerializeField] private float connectionLineFadeDuration = 0.25f;

        [Header("Play Animation")]
        [SerializeField] private float playScaleDuration = 0.4f;
        [SerializeField] private float playScale = 1.25f;

        [Header("References (Auto-assigned)")]
        [SerializeField] private Image noteImage;

        private Color _originalColor;
        private Tween _fadeTween;
        private Tween _connectionLineTween;
        private Tween _playTween;

        private bool _isAcquired;
        private bool _hasBeenPlayed;

        private static readonly Color[] NoteColors = {
            new Color(0.4f, 0.8f, 1f, 1f),      // WaterDrop - 淡蓝水色
            new Color(1f, 0.85f, 0.6f, 1f),    // Crossing - 温暖金色
            new Color(0.3f, 0.6f, 0.9f, 1f),    // Tide - 深海蓝
            new Color(0.7f, 1f, 0.8f, 1f)       // Breeze - 清新绿风
        };

        public bool IsAcquired => _isAcquired;
        public bool HasBeenPlayed => _hasBeenPlayed;

        private void Awake() {
            if (noteImage == null) noteImage = GetComponent<Image>();

            _originalColor = noteImage != null ? noteImage.color : Color.white;

            // 初始状态：隐藏
            if (noteImage != null) noteImage.DOFade(0f, 0f);
            if (connectionLine != null) connectionLine.DOFade(0f, 0f);
        }

        private void OnDestroy() {
            _fadeTween.Kill();
            _connectionLineTween.Kill();
            _playTween.Kill();
        }

        /// <summary>
        /// 获得音符，播放渐现动画
        /// </summary>
        public void Acquire() {
            if (_isAcquired) return;
            _isAcquired = true;

            _fadeTween.Kill();
            _connectionLineTween.Kill();

            switch (noteType) {
                case NoteType.WaterDrop:
                    _fadeTween = FadeIn();
                    break;

                case NoteType.Crossing:
                case NoteType.Tide:
                case NoteType.Breeze:
                    _connectionLineTween = FadeInConnectionLine();
                    break;
            }
        }

        /// <summary>
        /// 弹奏音符
        /// </summary>
        public void Play() {
            if (!_isAcquired || _playTween != null) return;

            if (!_hasBeenPlayed) {
                _hasBeenPlayed = true;
                _playTween = PlayFirstTimeAnimation();
            }
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset() {
            _isAcquired = false;
            _hasBeenPlayed = false;

            _fadeTween.Kill();
            _connectionLineTween.Kill();
            _playTween.Kill();

            if (noteImage != null) {
                noteImage.DOFade(0f, 0f);
                noteImage.color = _originalColor;
            }
            if (connectionLine != null) connectionLine.DOFade(0f, 0f);
            transform.localScale = Vector3.one;
        }

        private Tween FadeIn() {
            if (noteImage != null) {
                return noteImage.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);
            }
            return null;
        }

        private Tween FadeInConnectionLine() {
            if (connectionLine != null) {
                return connectionLine.DOFade(1f, connectionLineFadeDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => _fadeTween = FadeIn());
            }
            return FadeIn();
        }

        private Tween PlayFirstTimeAnimation() {
            var color = NoteColors[(int)noteType];

            return DOTween.Sequence()
                .Append(transform.DOScale(playScale, playScaleDuration * 0.4f).SetEase(Ease.OutQuad))
                .Join(noteImage.DOColor(color, playScaleDuration * 0.2f))
                .Append(transform.DOScale(1f, playScaleDuration * 0.6f).SetEase(Ease.OutElastic))
                .Join(noteImage.DOColor(_originalColor, playScaleDuration * 0.3f))
                .OnComplete(() => _playTween = null);
        }
    }
}