using UnityEngine;
using UnityEngine.InputSystem;

namespace Echoesphere.Runtime.Stuff {
    public class EchoTitle : MonoBehaviour {
        [Header("Input Settings")]
        [SerializeField] private InputActionReference projectWideInput;
        [Header("Echo Title")]
        [Range(0f, 0.1f)] public float positionStrength = 0.005f;
        [Range(0f, 10f)] public float positionSpeed = 1f;
        [Range(0f, 5f)] public float rotationStrength = 0.5f;
        [Range(0f, 10f)] public float rotationSpeed = 1f;

        private Transform[] _children;
        private Vector3[] _basePositions;
        private Quaternion[] _baseRotations;
        private Vector3[] _noiseOffsets;

        void OnEnable() {
            if (projectWideInput != null) {
                projectWideInput.action.performed += OnSubmitPerformed;
            }
        }

        void OnDisable() {
            if (projectWideInput != null) {
                projectWideInput.action.performed -= OnSubmitPerformed;
            }
        }

        private void OnSubmitPerformed(InputAction.CallbackContext context) {
            Debug.Log($"[Interaction] 检测到提交键按下 (设备: {context.control.device.name})");
        }

        private void Start() {
            int childCount = transform.childCount;
            _children = new Transform[childCount];
            _basePositions = new Vector3[childCount];
            _baseRotations = new Quaternion[childCount];
            _noiseOffsets = new Vector3[childCount];

            for (int i = 0; i < childCount; i++) {
                _children[i] = transform.GetChild(i);
                _basePositions[i] = _children[i].localPosition;
                _baseRotations[i] = _children[i].localRotation;
                _noiseOffsets[i] = new Vector3(
                    Random.Range(0f, 100f),
                    Random.Range(0f, 100f),
                    Random.Range(0f, 100f)
                );
            }
        }

        private void Update() {
            float t = Time.time;

            for (int i = 0; i < _children.Length; i++) {
                float px = Mathf.PerlinNoise(_noiseOffsets[i].x, t * positionSpeed) * 2f - 1f;
                float py = Mathf.PerlinNoise(_noiseOffsets[i].y, t * positionSpeed + 10f) * 2f - 1f;
                float pz = Mathf.PerlinNoise(_noiseOffsets[i].z, t * positionSpeed + 20f) * 2f - 1f;

                _children[i].localPosition = _basePositions[i] + new Vector3(px, py, pz) * positionStrength;

                float rx = Mathf.PerlinNoise(_noiseOffsets[i].x + 50f, t * rotationSpeed) * 2f - 1f;
                float ry = Mathf.PerlinNoise(_noiseOffsets[i].y + 50f, t * rotationSpeed + 10f) * 2f - 1f;
                float rz = Mathf.PerlinNoise(_noiseOffsets[i].z + 50f, t * rotationSpeed + 20f) * 2f - 1f;

                _children[i].localRotation = _baseRotations[i] * Quaternion.Euler(rx * rotationStrength, ry * rotationStrength, rz * rotationStrength);
            }
        }
    }
}
