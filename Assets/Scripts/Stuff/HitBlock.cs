using Echoesphere.Runtime.Configuration;
using Echoesphere.Runtime.RaspberryPi;
using UnityEngine;

namespace Echoesphere.Runtime.Stuff {
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
            if (!other.CompareTag("Player") || !Communicator) return;
            _ = Communicator.SendToAll($"HitBlockColor: {hitNote}");
            StartCoroutine(Communicator.BroadcastScreenshot());
        }
    }
    
    
}