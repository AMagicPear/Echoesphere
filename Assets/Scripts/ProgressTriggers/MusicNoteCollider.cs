using Echoesphere.Runtime.Configuration;
using Echoesphere.Runtime.UI.MusicNote;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Echoesphere.Runtime.ProgressTriggers {
    public class MusicNoteCollider : MonoBehaviour {
        public NoteType noteType;
        private bool _inArea;
        [SerializeField] private InputActionReference submitActionReference;

        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Player")) return;
            _inArea = true;
            GameRoot.Instance.gameStateManager.IsShowingWaterdrop = true;
        }

        private void OnEnable() {
            submitActionReference.action.performed += OnSubmitPerformed;
        }

        private void OnDisable() {
            submitActionReference.action.performed -= OnSubmitPerformed;
        }

        private void OnSubmitPerformed(InputAction.CallbackContext context) {
            if (!_inArea) return;
            GameRoot.Instance.musicNoteController.AcquireByType(noteType);
            _inArea = false;
            GameRoot.Instance.gameStateManager.IsShowingWaterdrop = false;
            gameObject.SetActive(false);
        }
    }
}