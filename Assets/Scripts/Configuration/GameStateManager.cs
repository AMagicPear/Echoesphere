using System;
using UnityEngine;

namespace Echoesphere.Runtime.Configuration {
    public enum EmotionState {
        Lost,       // 迷茫
        Exploring,  // 探索
        Resonating, // 共鸣
        Freedom     // 自由
    }

    public enum ChapterState {
        Origin,    // 初域·未定之原
        Colony,    // 聚落群·彼方的光影之城
        Deepflow   // 深流之径·未被触及的知识海
    }

    public class GameStateManager : MonoBehaviour {
        public static GameStateManager Instance { get; private set; }

        [Header("Current State")]
        [SerializeField] private ChapterState _currentChapter = ChapterState.Origin;
        [SerializeField] private EmotionState _currentEmotion = EmotionState.Lost;

        public ChapterState CurrentChapter {
            get => _currentChapter;
            private set {
                if (_currentChapter == value) return;
                _currentChapter = value;
                OnChapterChanged?.Invoke(_currentChapter);
            }
        }

        public EmotionState CurrentEmotion {
            get => _currentEmotion;
            private set {
                if (_currentEmotion == value) return;
                _currentEmotion = value;
                OnEmotionChanged?.Invoke(_currentEmotion);
            }
        }

        public Action<ChapterState> OnChapterChanged;
        public Action<EmotionState> OnEmotionChanged;

        private void Awake() {
            if (Instance) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetChapter(ChapterState chapter) => CurrentChapter = chapter;
        public void SetEmotion(EmotionState emotion) => CurrentEmotion = emotion;
    }
}