using Configuration;
using RaspberryPi;
using UnityEngine;

namespace Stuff {
    internal enum HitNote {
        Red,
        Blue,
        Green,
        Orange,
    }

    public class HitBlock : MonoBehaviour {
        [SerializeField] private HitNote hitNote;
        private static RaspberryPiCommunicator Communicator => GameRoot.Instance.rasPiCommunicator;

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player") && Communicator) {
                Communicator.BroadcastMessage($"HitBlockColor: {hitNote}");
                StartCoroutine(Communicator.BroadcastScreenshot());
            }
        }
    }
    
    
}