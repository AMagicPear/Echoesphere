using Echoesphere.Runtime.Configuration;
using UnityEngine;

namespace Echoesphere.Runtime.ProgressTriggers {
    public class BornPoint: MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Player")) return;
            GameRoot.Instance.gameStateManager.AtBornPoint = true;
        }

        private void OnTriggerExit(Collider other) {
            if (!other.CompareTag("Player")) return;
            GameRoot.Instance.gameStateManager.AtBornPoint = false;
        }
    }
}