using Echoesphere.Runtime.Agent;
using Echoesphere.Runtime.Stuff;
using Echoesphere.Runtime.UI;
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

        [SerializeField] private Animator cameraAnimator;

        private static readonly int AnimFacePresent = Animator.StringToHash("FacePresent");
        private static readonly int AnimIsShowing = Animator.StringToHash("IsShowingWaterdrop");
        private static readonly int AnimEchoTitleActive = Animator.StringToHash("EchoTitleActive");
        private static readonly int AnimExitBornPoint = Animator.StringToHash("ExitBornPoint");

        private static DialogController DialogController => GameRoot.Instance.dialogController;

        private bool _facePresent;
        private bool _isShowingWaterdrop;

        public ChapterState CurrentChapter {
            get => gameState.chapter;
            set => gameState.chapter = value;
        }

        public bool AtBornPoint {
            get => gameState.atBornPoint;
            set {
                if (value) {
                    if (!EchoTitleActive) {
                        DialogController.Hide();
                    }
                } else {
                    DialogController.Show(2);
                    cameraAnimator.SetTrigger(AnimExitBornPoint);
                }

                gameState.atBornPoint = value;
            }
        }

        private bool EchoTitleActive {
            get => gameState.echoTitleActive;
            set {
                gameState.echoTitleActive = value;
                cameraAnimator.SetBool(AnimEchoTitleActive, value);
            }
        }

        public bool FacePresent {
            get => _facePresent;
            set {
                _facePresent = value;
                cameraAnimator.SetBool(AnimFacePresent, value);
            }
        }
        
        public bool IsShowingWaterdrop {
            get => _isShowingWaterdrop;
            set {
                _isShowingWaterdrop = value;
                cameraAnimator.SetBool(AnimIsShowing, value);
                if (value) {
                    playerInputActions?.FindActionMap("Player")?.Disable();
                    DialogController.Show(5);
                } else {
                    playerInputActions?.FindActionMap("Player")?.Enable();
                    DialogController.Show(6);
                }
            }
        }

        public object CaptureState() => gameState;

        public void RestoreState(object data) {
            if (data is GameState s) gameState = s;
            if (EchoTitleActive) return;
            DialogController.Hide();
            echoTitle.gameObject.SetActive(false);
        }
        
        private float _lastTarget;

        private static AgentCommunicator Agent => GameRoot.Instance.agentCommunicator;

        private void OnEnable() {
            echoTitle.OnDismissComplete += OnTitleDismissed;
            Agent.OnCommandReceived += OnCommandReceived;
            submitActionReference.action.performed += OnSubmitPerformed;
        }

        private void Start() {
            if (!EchoTitleActive) return;
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
            EchoTitleActive = false;
            DialogController.Hide();
            Agent.SendCommand("chase:lightgreen", null, "raspberry_pi");
        }

        private void OnCommandReceived(JsonMessage msg) {
            switch (msg.data) {
                case "face:in":
                    FacePresent = true;
                    if (EchoTitleActive) {
                        DialogController.Show(0);
                    }

                    break;
                case "face:out":
                    FacePresent = false;
                    if (EchoTitleActive) {
                        DialogController.Hide();
                    }
                    break;
            }
        }

        private void OnTitleDismissed() {
            playerInputActions?.FindActionMap("Player")?.Enable();
            DialogController.Show(1);
            Agent.SendCommand("hand_direction:on", null, "mediapipe");
        }
    }
}