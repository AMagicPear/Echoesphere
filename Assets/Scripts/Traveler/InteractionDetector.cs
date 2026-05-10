using UnityEngine;

namespace Echoesphere.Runtime.Traveler {
    public class InteractionDetector : MonoBehaviour {
        
        private void OnTriggerEnter(Collider other) {
            Debug.Log($"OnTriggerEnter: {other.name}");
        }

        private void OnTriggerExit(Collider other) {
            Debug.Log($"OnTriggerExit: {other.name}");
        }
    }
}