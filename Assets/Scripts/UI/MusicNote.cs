using DG.Tweening;
using Echoesphere.Runtime.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace Echoesphere.Runtime.UI {
    public enum NoteType {
        WaterDrop,
        Crossing,
        Tide,
        Breeze
    }

    public static class NoteTypeExtensions {
        public static string ToCommandName(this NoteType noteType) => noteType switch {
            NoteType.WaterDrop => "waterdrop",
            NoteType.Crossing => "crossing",
            NoteType.Tide => "tide",
            NoteType.Breeze => "breeze",
            _ => throw new System.ArgumentOutOfRangeException(nameof(noteType))
        };
    }

    [RequireComponent(typeof(Image))]
    public class MusicNote : MonoBehaviour {
        [Header("Settings")]
        public NoteType noteType;

        [Header("Connection Line")]
        [Tooltip("音符上方对应的连接线（道路交错、海浪、风需要）")]
        [SerializeField] private Image connectionLine;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float connectionLineFadeDuration = 0.6f;

        [Header("Play Animation")]
        [Tooltip("演奏时颜色过渡时长")]
        [SerializeField] private float playColorDuration = 0.5f;
        [Tooltip("发光脉冲次数")]
        [SerializeField] private int glowPulseCount = 2;
        [Tooltip("发光脉冲时长")]
        [SerializeField] private float glowPulseDuration = 0.2f;
        [Tooltip("发光强度")]
        [SerializeField] private float glowIntensity = 0.6f;

        [Header("Acquire Animation")]
        [Tooltip("获取时缩放持续时间")]
        [SerializeField] private float acquireScaleDuration = 0.2f;
        [Tooltip("获取时缩放大小")]
        [SerializeField] private float acquireScale = 1.3f;

        [Header("Audio")]
        [Tooltip("获取提示音")]
        [SerializeField] private AudioSource acquireSound;

        [Header("References (Auto-assigned)")]
        [SerializeField] private Image noteImage;
        [Tooltip("演奏时的背景发光图像")]
        [SerializeField] private Image glowImage;

        private Color _originalColor;
        private Tween _fadeTween;
        private Tween _connectionLineTween;
        private Tween _playTween;
        private Tween _flashTween;

        private bool _isAcquired;

        private static readonly Color[] NoteColors = {
            new(0.4f, 0.8f, 1f, 1f),      // WaterDrop - 淡蓝水色
            new(1f, 0.85f, 0.6f, 1f),    // Crossing - 温暖金色
            new(0.3f, 0.6f, 0.9f, 1f),    // Tide - 深海蓝
            new(0.7f, 1f, 0.8f, 1f)       // Breeze - 清新绿风
        };

        public bool IsAcquired => _isAcquired;

        private void Awake() {
            if (noteImage == null) noteImage = GetComponent<Image>();

            _originalColor = noteImage != null ? noteImage.color : Color.white;

            // 初始状态：隐藏
            if (noteImage != null) noteImage.DOFade(0f, 0f);
            if (connectionLine != null) connectionLine.DOFade(0f, 0f);
            if (glowImage != null) glowImage.DOFade(0f, 0f);
        }

        private void OnDestroy() {
            _fadeTween.Kill();
            _connectionLineTween.Kill();
            _playTween.Kill();
            _flashTween.Kill();
        }

        /// <summary>
        /// 获得音符，播放渐现动画和缩放提示
        /// </summary>
        public void Acquire() {
            if (_isAcquired) return;
            _isAcquired = true;

            // 向 raspberry_pi 发送 gain_note 命令
            if (GameRoot.Instance != null && GameRoot.Instance.agentCommunicator != null) {
                _ = GameRoot.Instance.agentCommunicator.SendCommand(
                    $"gain_note:{noteType.ToCommandName()}", relayTo: "raspberry_pi");
            }

            _fadeTween.Kill();
            _connectionLineTween.Kill();

            // 播放获取音效
            if (acquireSound != null) acquireSound.Play();

            switch (noteType) {
                case NoteType.WaterDrop:
                    // 水滴音符：缩放和渐显同时进行
                    _flashTween = AcquireAnimation();
                    _fadeTween = FadeIn();
                    break;

                case NoteType.Crossing:
                case NoteType.Tide:
                case NoteType.Breeze:
                    // 其他音符：连接线先渐显，等音符可见时再缩放
                    _connectionLineTween = FadeInConnectionLine();
                    // 延迟缩放动画，与音符渐显同步
                    _flashTween = DOTween.Sequence()
                        .AppendInterval(connectionLineFadeDuration)
                        .AppendCallback(() => _flashTween = AcquireAnimation());
                    break;
            }
        }

        /// <summary>
        /// 弹奏音符，播放变色+发光动画
        /// </summary>
        public void Play() {
            if (!_isAcquired || _playTween != null) return;

            if (GameRoot.Instance != null && GameRoot.Instance.agentCommunicator != null) {
                _ = GameRoot.Instance.agentCommunicator.SendCommand(
                    $"play_note:{noteType.ToCommandName()}", relayTo: "raspberry_pi");
            }

            _playTween = PlayAnimation();
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset() {
            _isAcquired = false;

            _fadeTween.Kill();
            _connectionLineTween.Kill();
            _playTween.Kill();
            _flashTween.Kill();

            if (noteImage != null) {
                noteImage.DOFade(0f, 0f);
                noteImage.color = _originalColor;
            }
            if (connectionLine != null) connectionLine.DOFade(0f, 0f);
            if (glowImage != null) glowImage.DOFade(0f, 0f);
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

        private Tween AcquireAnimation() {
            return DOTween.Sequence()
                .Append(transform.DOScale(acquireScale, acquireScaleDuration))
                .Append(transform.DOScale(1f, acquireScaleDuration))
                .Append(transform.DOScale(acquireScale, acquireScaleDuration))
                .Append(transform.DOScale(1f, acquireScaleDuration));
        }

        private Tween PlayAnimation() {
            var color = NoteColors[(int)noteType];

            var sequence = DOTween.Sequence();

            // 变色
            sequence.Append(noteImage.DOColor(color, playColorDuration));

            // 发光脉冲
            if (glowImage != null) {
                for (int i = 0; i < glowPulseCount; i++) {
                    sequence.Append(glowImage.DOFade(glowIntensity, glowPulseDuration))
                        .Append(glowImage.DOFade(0f, glowPulseDuration));
                }
            }

            // 颜色恢复
            sequence.Append(noteImage.DOColor(_originalColor, playColorDuration * 0.5f));

            return sequence.OnComplete(() => _playTween = null);
        }
    }
}