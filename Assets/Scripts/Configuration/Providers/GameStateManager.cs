using Echoesphere.Runtime.Agent;
using Echoesphere.Runtime.Stuff;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Echoesphere.Runtime.Configuration.Providers {
    public enum ChapterState {
        Origin, // 初域·未定之原
        Colony, // 聚落群·彼方的光影之城
        Deepflow // 深流之径·未被触及的知识海
    }

    public class GameStateManager : MonoBehaviour, ISaveProvider {
        [System.Serializable]
        private struct GameState {
            public ChapterState chapter;
            public bool echoTitleActive;
        }
        
        [SerializeField] private string uniqueId = "game_state_manager";
        public string UniqueId => uniqueId;

        [FormerlySerializedAs("state")] [Header("Current State")] [SerializeField]
        private GameState gameState = new() { chapter = ChapterState.Origin, echoTitleActive = true };

        [Header("References")] [SerializeField]
        private InputActionAsset playerInputActions;

        [SerializeField] private EchoTitle echoTitle;

        public ChapterState CurrentChapter {
            get => gameState.chapter;
            set => gameState.chapter = value;
        }

        public bool EchoTitleActive {
            get => gameState.echoTitleActive;
            set => gameState.echoTitleActive = value;
        }

        public object CaptureState() => gameState;

        public void RestoreState(object data) {
            if (data is GameState s) gameState = s;
        }

        public bool FacePresent { get; private set; }

        private static AgentCommunicator Agent => GameRoot.Instance.agentCommunicator;

        private void Start() {
            DisableGlobalInput();
            echoTitle.OnDismissComplete += OnTitleDismissed;
            Agent.OnCommandReceived += OnCommandReceived;
        }

        private void OnDestroy() {
            echoTitle.OnDismissComplete -= OnTitleDismissed;
            Agent.OnCommandReceived -= OnCommandReceived;
        }

        private void OnCommandReceived(JsonMessage msg) {
            switch (msg.data) {
                case "face:in":
                    FacePresent = true;
                    break;
                case "face:out":
                    FacePresent = false;
                    break;
                case "hand:sweep":
                    if (FacePresent) echoTitle.Dismiss();
                    break;
            }
        }

        private void OnTitleDismissed() {
            EchoTitleActive = false;
            EnableGlobalInput();
            Agent.SendCommand("hand_direction:on", null, "mediapipe");
        }

        private void DisableGlobalInput() {
            playerInputActions?.FindActionMap("Player")?.Disable();
        }

        private void EnableGlobalInput() {
            playerInputActions?.FindActionMap("Player")?.Enable();
        }
    }
}