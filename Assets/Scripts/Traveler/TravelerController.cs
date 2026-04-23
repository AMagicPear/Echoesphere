using UnityEngine;
using UnityEngine.InputSystem;

namespace Echoesphere.Runtime.Traveler {
    // [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(CharacterController))]
    public class TravelerController : MonoBehaviour {
        [SerializeField] private InputActionReference _moveActionRef;
        private static readonly int Speed = Animator.StringToHash("speed");
        public float moveSpeed = 2.0f;
        public Vector2 moveInput;
        [Range(0.0f, 0.3f)] public float rotationSmoothTime = 0.12f;
        public Animator animator;

        private CharacterController _controller;
        private GameObject _mainCamera;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;

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
            ApplyAnimation();
        }

        private void Move() {
            moveInput = _moveActionRef.action.ReadValue<Vector2>();
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

        private void ApplyAnimation() {
            animator.SetFloat(Speed, _controller.velocity.magnitude);
        }

        // private void OnControllerColliderHit(ControllerColliderHit hit) {
        // }
    }
}