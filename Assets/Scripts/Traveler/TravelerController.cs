using UnityEngine;
using UnityEngine.InputSystem;

namespace Traveler {
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(CharacterController))]
    public class TravelerController : MonoBehaviour {
        public float moveSpeed = 2.0f;
        public Vector2 moveInput;
        [Range(0.0f, 0.3f)] public float rotationSmoothTime = 0.12f;

        private CharacterController _controller;
        private GameObject _mainCamera;
        private float _speed;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;

        /// 接收PlayerInput的输入
        public void OnMove(InputValue value) {
            moveInput = value.Get<Vector2>();
        }

        private void Awake() {
            if (_mainCamera == null) {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start() {
            _controller = GetComponent<CharacterController>();
        }

        private void FixedUpdate() {
            ApplyGravity();
            Move();
        }

        private void Move() {
            Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;
            if (moveInput != Vector2.zero) {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
                Vector3 horizontalMove =
                    targetDirection.normalized * (moveSpeed * Time.deltaTime * moveInput.magnitude);
                _controller.Move(horizontalMove + new Vector3(0, _verticalVelocity * Time.deltaTime, 0));
            } else {
                _controller.Move(new Vector3(0, _verticalVelocity * Time.deltaTime, 0));
            }
        }

        private void ApplyGravity() {
            if (!_controller.isGrounded) {
                _verticalVelocity += Physics.gravity.y * Time.deltaTime;
            } else {
                _verticalVelocity = -.5f;
            }
        }
    }
}