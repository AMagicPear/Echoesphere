using Echoesphere.Runtime.Configuration;
using Echoesphere.Runtime.Agent;
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
        private static AgentCommunicator Communicator => GameRoot.Instance.rasPiCommunicator;

        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Player")) return;
            _ = Communicator.SendText($"HitBlockColor: {hitNote}");
            // StartCoroutine(Communicator.SendScreenshot());
        }
    }
    
    
}