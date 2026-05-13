using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Echoesphere.Runtime.Stuff {
    public class CrumblingPathway : MonoBehaviour {
        [Header("Fall")]
        [SerializeField] private float fallHeight = 5f;
        [SerializeField] private float fallDuration = 0.5f;
        [SerializeField] private Ease fallEase = Ease.OutQuad;

        [Header("Rise")]
        [SerializeField] private Ease riseEase = Ease.InQuad;

        [Header("Activation")]
        [SerializeField] private Vector3 startPoint;
        [SerializeField] private float forwardExtension = 5f;

        private enum BlockState { Hidden, Falling, Fallen, Rising }

        private Transform[] _blockTransforms;
        private MeshRenderer[] _blockRenderers;
        private Vector3[] _targetWorldPos;
        private Vector3[] _elevatedWorldPos;
        private float[] _projectedPositions;
        private Tween[] _activeTweens;
        private BlockState[] _blockStates;
        private Transform _traveler;

        private void Awake() {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            var blocks = new List<Transform>();
            foreach (var r in renderers) {
                if (r.transform != transform)
                    blocks.Add(r.transform);
            }

            blocks.Sort((a, b) => ProjectedDistance(a.position).CompareTo(ProjectedDistance(b.position)));
            _blockTransforms = blocks.ToArray();

            int count = _blockTransforms.Length;
            _blockRenderers = new MeshRenderer[count];
            _targetWorldPos = new Vector3[count];
            _elevatedWorldPos = new Vector3[count];
            _projectedPositions = new float[count];
            _activeTweens = new Tween[count];
            _blockStates = new BlockState[count];

            for (int i = 0; i < count; i++) {
                _blockRenderers[i] = _blockTransforms[i].GetComponent<MeshRenderer>();
                _targetWorldPos[i] = _blockTransforms[i].position;
                _elevatedWorldPos[i] = _targetWorldPos[i] + Vector3.up * fallHeight;
                _projectedPositions[i] = ProjectedDistance(_blockRenderers[i].bounds.center);
            }
        }

        private void Start() {
            _traveler = GameObject.FindGameObjectWithTag("Player")?.transform;

            float startProj = ProjectedDistance(startPoint);
            float travelerProj = _traveler != null ? ProjectedDistance(_traveler.position) : startProj;
            float activeEnd = Mathf.Max(startProj, travelerProj) + forwardExtension;

            for (int i = 0; i < _blockTransforms.Length; i++) {
                bool shouldBeFallen = _projectedPositions[i] >= startProj && _projectedPositions[i] <= activeEnd;

                if (shouldBeFallen) {
                    _blockTransforms[i].position = _targetWorldPos[i];
                    if (_blockRenderers[i] != null) _blockRenderers[i].enabled = true;
                    _blockStates[i] = BlockState.Fallen;
                } else {
                    _blockTransforms[i].position = _elevatedWorldPos[i];
                    if (_blockRenderers[i] != null) _blockRenderers[i].enabled = false;
                    _blockStates[i] = BlockState.Hidden;
                }
            }
        }

        private void Update() {
            if (_traveler == null || _blockTransforms.Length == 0) return;

            float startProj = ProjectedDistance(startPoint);
            float travelerProj = ProjectedDistance(_traveler.position);
            float activeEnd = Mathf.Max(startProj, travelerProj) + forwardExtension;

            for (int i = 0; i < _blockTransforms.Length; i++) {
                bool shouldBeFallen = _projectedPositions[i] >= startProj && _projectedPositions[i] <= activeEnd;
                BlockState state = _blockStates[i];

                if (shouldBeFallen && (state == BlockState.Hidden || state == BlockState.Rising))
                    ActivateBlock(i);
                else if (!shouldBeFallen && (state == BlockState.Fallen || state == BlockState.Falling))
                    DeactivateBlock(i);
            }
        }

        private float ProjectedDistance(Vector3 worldPos) {
            return Vector3.Dot(worldPos - transform.position, transform.forward);
        }

        private void ActivateBlock(int index) {
            if (_activeTweens[index] != null)
                _activeTweens[index].Kill();

            _blockStates[index] = BlockState.Falling;
            if (_blockRenderers[index] != null) _blockRenderers[index].enabled = true;

            int ci = index;
            _activeTweens[index] = _blockTransforms[index].DOMove(_targetWorldPos[ci], fallDuration)
                .SetEase(fallEase)
                .OnComplete(() => {
                    _blockStates[ci] = BlockState.Fallen;
                    _activeTweens[ci] = null;
                });
        }

        private void DeactivateBlock(int index) {
            if (_activeTweens[index] != null)
                _activeTweens[index].Kill();

            _blockStates[index] = BlockState.Rising;

            int ci = index;
            _activeTweens[index] = _blockTransforms[index].DOMove(_elevatedWorldPos[ci], fallDuration)
                .SetEase(riseEase)
                .OnComplete(() => {
                    _blockStates[ci] = BlockState.Hidden;
                    _activeTweens[ci] = null;
                    if (_blockRenderers[ci] != null) _blockRenderers[ci].enabled = false;
                });
        }

        private void OnDestroy() {
            if (_activeTweens != null) {
                foreach (var t in _activeTweens) {
                    if (t != null) t.Kill();
                }
            }
        }
    }
}
