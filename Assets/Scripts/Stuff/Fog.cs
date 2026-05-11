using UnityEngine;

namespace Echoesphere.Runtime.Stuff {
    public class Fog : MonoBehaviour {
        private static readonly int Offset = Shader.PropertyToID("_Offset");
        [Header("Fog Offset")]
        public Vector3 offsetSpeed = new(0.1f, 0f, 0.1f);
        public Vector3 offsetWobble = new(0.02f, 0f, 0.02f);
        public Material fogMaterial;

        private Vector3 _currentOffset;

        private void Start() {
            _currentOffset = Vector3.zero;
        }

        private void Update() {
            float wobbleX = Mathf.Sin(Time.time * 2f) * offsetWobble.x;
            float wobbleZ = Mathf.Cos(Time.time * 1.7f) * offsetWobble.z;

            _currentOffset.x += (offsetSpeed.x + wobbleX) * Time.deltaTime;
            _currentOffset.z += (offsetSpeed.z + wobbleZ) * Time.deltaTime;

            fogMaterial.SetVector(Offset, _currentOffset);
        }

        private void OnDestroy() {
            fogMaterial.SetVector(Offset, Vector3.zero);
        }
    }
}
