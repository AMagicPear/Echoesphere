using DG.Tweening;
using Echoesphere.Runtime.Agent;
using Echoesphere.Runtime.Stuff;
using Echoesphere.Runtime.UI;
using Unity.Cinemachine;
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
        private struct GameState {
            public ChapterState chapter;
            public bool echoTitleActive;
            public bool atBornPoint;
        }

        [SerializeField] private string uniqueId = "game_state_manager";
        public string UniqueId => uniqueId;

        [Header("Current State")] [SerializeField]
        private GameState gameState = new()
            { chapter = ChapterState.Origin, echoTitleActive = true, atBornPoint = true };

        [SerializeField] private float cameraMoveDuration = 3f;

        [Header("References")] [SerializeField]
        private InputActionAsset playerInputActions;

        [SerializeField] private InputActionReference submitActionReference;

        [SerializeField] private EchoTitle echoTitle;

        [SerializeField] private CinemachineSplineDolly splineDolly;

        private static DialogController DialogController => GameRoot.Instance.dialogController;

        public ChapterState CurrentChapter {
            get => gameState.chapter;
            set => gameState.chapter = value;
        }

        public bool AtBornPoint {
            get => gameState.atBornPoint;
            set {
                if (value) {
                    if (!gameState.echoTitleActive) {
                        DialogController.Hide();
                        MoveCameraTo(1);
                    }
                } else {
                    DialogController.Show(2);
                    MoveCameraTo(0);
                }

                gameState.atBornPoint = value;
            }
        }

        public object CaptureState() => gameState;

        public void RestoreState(object data) {
            if (data is GameState s) gameState = s;
            if (!gameState.echoTitleActive) {
                DialogController.Hide();
                echoTitle.gameObject.SetActive(false);
                MoveCameraTo(2);
            }

            if (!gameState.atBornPoint) {
                MoveCameraTo(0);
            }
        }

        public bool FacePresent { get; private set; }

        private float _lastTarget;

        private static AgentCommunicator Agent => GameRoot.Instance.agentCommunicator;

        private void OnEnable() {
            echoTitle.OnDismissComplete += OnTitleDismissed;
            Agent.OnCommandReceived += OnCommandReceived;
            submitActionReference.action.performed += OnSubmitPerformed;
        }

        private void Start() {
            if (!gameState.echoTitleActive) return;
            playerInputActions?.FindActionMap("Player")?.Disable();
            FacePresent = false;
        }

        private void OnDisable() {
            echoTitle.OnDismissComplete -= OnTitleDismissed;
            Agent.OnCommandReceived -= OnCommandReceived;
            submitActionReference.action.performed -= OnSubmitPerformed;
        }

        private void OnSubmitPerformed(InputAction.CallbackContext context) {
            if (!FacePresent) return;
            echoTitle.Dismiss();
            gameState.echoTitleActive = false;
            DialogController.Hide();
            Agent.SendCommand("chase:lightgreen", null, "raspberry_pi");
        }

        private void OnCommandReceived(JsonMessage msg) {
            switch (msg.data) {
                case "face:in":
                    FacePresent = true;
                    if (gameState.echoTitleActive) {
                        MoveCameraTo(2);
                        DialogController.Show(0);
                    }

                    break;
                case "face:out":
                    FacePresent = false;
                    if (gameState.echoTitleActive) {
                        MoveCameraTo(3);
                        DialogController.Hide();
                    }

                    break;
            }
        }

        private void MoveCameraTo(float target) {
            if (Mathf.Approximately(_lastTarget, target)) return;
            _lastTarget = target;

            DOTween.To(
                () => splineDolly.CameraPosition,
                x => splineDolly.CameraPosition = x,
                target,
                cameraMoveDuration
            ).SetEase(Ease.OutQuad);
        }

        private void OnTitleDismissed() {
            playerInputActions?.FindActionMap("Player")?.Enable();
            DialogController.Show(1);
            Agent.SendCommand("hand_direction:on", null, "mediapipe");
        }
    }
}