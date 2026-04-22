using Echoesphere.Runtime.Stuff;
using UnityEngine;

namespace Echoesphere.Runtime.Traveler {
    public class InteractionDetector : MonoBehaviour {
        private IInteractable currentInteractable;

        void OnTriggerEnter(Collider other) {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable != null) {
                currentInteractable = interactable;
                // 通知 UI 显示提示，例如：UIManager.Instance.ShowHint(interactable.GetInteractText());
            }
        }

        void OnTriggerExit(Collider other) {
            if (other.GetComponent<IInteractable>() == currentInteractable) {
                currentInteractable = null;
                // UIManager.Instance.HideHint();
            }
        }
    }
}