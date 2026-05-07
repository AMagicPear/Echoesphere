using Echoesphere.Runtime.Stuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Echoesphere.Runtime.Configuration.Providers {
    public enum ChapterState {
        Origin, // 初域·未定之原
        Colony, // 聚落群·彼方的光影之城
        Deepflow // 深流之径·未被触及的知识海
    }

    public class GameStateManager : MonoBehaviour, ISaveProvider {
        [System.Serializable]
        private struct State {
            public ChapterState chapter;
            public bool echoTitleActive;
        }

        public void SetChapter(ChapterState chapter) => CurrentChapter = chapter;

        [SerializeField] private string uniqueId = "game_state_manager";
        public string UniqueId => uniqueId;

        [Header("Current State")] [SerializeField]
        private State state = new() { chapter = ChapterState.Origin, echoTitleActive = true };

        [Header("References")] [SerializeField]
        private InputActionAsset playerInputActions;

        [SerializeField] private EchoTitle echoTitle;

        public ChapterState CurrentChapter {
            get => state.chapter;
            private set => state.chapter = value;
        }

        public bool EchoTitleActive {
            get => state.echoTitleActive;
            set => state.echoTitleActive = value;
        }

        public object CaptureState() => state;

        public void RestoreState(object data) {
            if (data is State s) state = s;
        }

        private void Start() {
            DisableGlobalInput();
            echoTitle.OnDismissComplete += OnTitleDismissed;
            GameRoot.Instance.agentCommunicator.OnHandSweepGesture += OnHandSweepGesture;
        }

        private void OnDestroy() {
            echoTitle.OnDismissComplete -= OnTitleDismissed;
            GameRoot.Instance.agentCommunicator.OnHandSweepGesture -= OnHandSweepGesture;
        }

        private void OnHandSweepGesture() {
            if (!GameRoot.Instance.agentCommunicator.FacePresent) return;
            echoTitle.Dismiss();
        }

        private void OnTitleDismissed() {
            EchoTitleActive = false;
            EnableGlobalInput();
        }

        private void DisableGlobalInput() {
            playerInputActions?.FindActionMap("Player")?.Disable();
        }

        private void EnableGlobalInput() {
            playerInputActions?.FindActionMap("Player")?.Enable();
        }
    }
}