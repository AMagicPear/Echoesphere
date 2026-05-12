using Echoesphere.Runtime.Configuration;
using UnityEngine;

namespace Echoesphere.Runtime.ProgressTriggers {
    public class TriggerDialogOnly : MonoBehaviour {
        [SerializeField] private int dialogId;
        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Player")) return;
            GameRoot.Instance.dialogController.Show(dialogId);
        }
    }
}