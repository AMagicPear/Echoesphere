using UnityEngine;

namespace Echoesphere.Runtime.Stuff {
    public class CrumblingPathway : MonoBehaviour {
        private MeshRenderer[] _pathBlocks;

        private void Awake() {
            _pathBlocks = GetComponentsInChildren<MeshRenderer>();
        }
    }
}