using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Echoesphere.Runtime.Stuff {
    public class EchoTitle : MonoBehaviour {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int Surface = Shader.PropertyToID("_Surface");
        private static readonly int Blend = Shader.PropertyToID("_Blend");

        private enum State {
            Floating,
            Dismissing,
            Dismissed
        }

        [Header("Input Settings")] [SerializeField]
        private InputActionReference projectWideInput;

        [Header("Floating")] [Range(0f, 0.1f)] public float positionStrength = 0.005f;

        [Range(0f, 10f)] public float positionSpeed = 1f;
        [Range(0f, 5f)] public float rotationStrength = 0.5f;
        [Range(0f, 10f)] public float rotationSpeed = 1f;

        [Header("Dismiss")] [Range(0.5f, 5f)] public float dismissDuration = 3f;

        public float dismissSpreadRadius = 2f;
        public float dismissRandomRotation = 180f;
        public float dismissXJitter = 0.3f;

        public event Action OnDismissComplete;

        private Transform[] _children;
        private Vector3[] _basePositions;
        private Quaternion[] _baseRotations;
        private Vector3[] _noiseOffsets;
        private Vector3[] _dismissTargetPositions;
        private Quaternion[] _dismissTargetRotations;
        private MeshRenderer[] _renderers;
        private MaterialPropertyBlock[] _propertyBlocks;
        private Color[] _baseColors;
        private State _state = State.Floating;

        private void OnEnable() {
            if (projectWideInput != null) {
                projectWideInput.action.performed += OnSubmitPerformed;
            }
        }

        private void OnDisable() {
            if (projectWideInput != null) {
                projectWideInput.action.performed -= OnSubmitPerformed;
            }
        }

        private void OnSubmitPerformed(InputAction.CallbackContext context) {
            Dismiss();
        }

        private void Start() {
            int childCount = transform.childCount;
            _children = new Transform[childCount];
            _basePositions = new Vector3[childCount];
            _baseRotations = new Quaternion[childCount];
            _noiseOffsets = new Vector3[childCount];
            _dismissTargetPositions = new Vector3[childCount];
            _dismissTargetRotations = new Quaternion[childCount];
            _renderers = new MeshRenderer[childCount];
            _propertyBlocks = new MaterialPropertyBlock[childCount];
            _baseColors = new Color[childCount];

            for (int i = 0; i < childCount; i++) {
                _children[i] = transform.GetChild(i);
                _basePositions[i] = _children[i].localPosition;
                _baseRotations[i] = _children[i].localRotation;
                _noiseOffsets[i] = new Vector3(
                    Random.Range(0f, 100f),
                    Random.Range(0f, 100f),
                    Random.Range(0f, 100f)
                );

                _renderers[i] = _children[i].GetComponent<MeshRenderer>();
                if (_renderers[i] != null) {
                    var mat = _renderers[i].material;
                    mat.SetFloat(Surface, 1);
                    mat.SetFloat(Blend, 0);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = 3000;
                    _baseColors[i] = mat.GetColor(BaseColor);

                    _propertyBlocks[i] = new MaterialPropertyBlock();
                    _renderers[i].GetPropertyBlock(_propertyBlocks[i]);
                }
            }
        }

        public void Dismiss() {
            if (_state != State.Floating) return;
            _state = State.Dismissing;
            PrepareDismissTargets();
            StartCoroutine(DismissRoutine());
        }

        private void PrepareDismissTargets() {
            for (int i = 0; i < _children.Length; i++) {
                Vector3 basePos = _basePositions[i];
                Vector3 yzDir = new Vector3(0, basePos.y, basePos.z);
                if (yzDir.sqrMagnitude < 0.001f) {
                    yzDir = new Vector3(0, Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                } else {
                    yzDir.Normalize();
                }

                float xJitter = Random.Range(-dismissXJitter, dismissXJitter);
                _dismissTargetPositions[i] = basePos + yzDir * dismissSpreadRadius + new Vector3(xJitter, 0, 0);

                _dismissTargetRotations[i] = _baseRotations[i] * Quaternion.Euler(
                    Random.Range(-dismissRandomRotation, dismissRandomRotation),
                    Random.Range(-dismissRandomRotation, dismissRandomRotation),
                    Random.Range(-dismissRandomRotation, dismissRandomRotation)
                );
            }
        }

        private IEnumerator DismissRoutine() {
            float elapsed = 0f;
            while (elapsed < dismissDuration) {
                elapsed += Time.deltaTime;
                float t = elapsed / dismissDuration;

                for (int i = 0; i < _children.Length; i++) {
                    _children[i].localPosition = Vector3.Lerp(_basePositions[i], _dismissTargetPositions[i], t);
                    _children[i].localRotation =
                        Quaternion.Slerp(_baseRotations[i], _dismissTargetRotations[i], t);

                    SetChildAlpha(i, 1f - t);
                }

                yield return null;
            }

            _state = State.Dismissed;
            OnDismissComplete?.Invoke();
            gameObject.SetActive(false);
        }

        private void SetChildAlpha(int index, float alpha) {
            if (_renderers[index] == null) return;

            var block = _propertyBlocks[index];
            var color = _baseColors[index];
            color.a = alpha;
            block.SetColor(BaseColor, color);
            _renderers[index].SetPropertyBlock(block);
        }

        private void Update() {
            if (_state != State.Floating) return;

            float t = Time.time;

            for (int i = 0; i < _children.Length; i++) {
                float px = Mathf.PerlinNoise(_noiseOffsets[i].x, t * positionSpeed) * 2f - 1f;
                float py = Mathf.PerlinNoise(_noiseOffsets[i].y, t * positionSpeed + 10f) * 2f - 1f;
                float pz = Mathf.PerlinNoise(_noiseOffsets[i].z, t * positionSpeed + 20f) * 2f - 1f;

                _children[i].localPosition = _basePositions[i] + new Vector3(px, py, pz) * positionStrength;

                float rx = Mathf.PerlinNoise(_noiseOffsets[i].x + 50f, t * rotationSpeed) * 2f - 1f;
                float ry = Mathf.PerlinNoise(_noiseOffsets[i].y + 50f, t * rotationSpeed + 10f) * 2f - 1f;
                float rz = Mathf.PerlinNoise(_noiseOffsets[i].z + 50f, t * rotationSpeed + 20f) * 2f - 1f;

                _children[i].localRotation = _baseRotations[i] *
                                             Quaternion.Euler(rx * rotationStrength, ry * rotationStrength,
                                                 rz * rotationStrength);
            }
        }
    }
}