using Inputs;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Actions
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour, IPlayerActionMovement, IPlayerActionSprint, IPlayerActionJump,
        IPlayerActionCrouch
    {
        [Header("Movement Settings")]
        [Tooltip("Walking speed (m/s)")]
        [SerializeField] private float walkingSpeed = 4f;
        [Tooltip("Sprinting speed (m/s)")]
        [SerializeField] private float sprintSpeed = 6f;
        [Tooltip("Maximum player acceleration and deceleration")]
        [SerializeField] private float maxAcceleration = 10f;

        [Header("Jump Settings")]
        [Tooltip("Jump height in meters")]
        [SerializeField] private float jumpHeight = 1.2f;
        [Tooltip("Maximum player jump acceleration")]
        [SerializeField] private float maxJumpAcceleration = 1f;
        
        [Header("Ground Settings")]
        [Tooltip("Max angle in which the player can climb naturally")]
        [SerializeField] private float maxGroundAngle = 25f;
        
        [Header("Camera Settings")]
        [Tooltip("The target that the virtual camera will follow")]
        [SerializeField] private GameObject followTarget;
        [Tooltip("Look up clamp")]
        [SerializeField] float upClamp = 90f;
        [Tooltip("Look down clamp")]
        [SerializeField] float downClamp = -90f;

        [Header("Component Registry")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask environmentLayer;
        
        private PlayerControls _playerControls;
        private Rigidbody _playerRigidbody;
        private Vector3 _velocity;
        private Vector3 _contactNormal;
        private Vector2 _targetMove;
        private float _minGroundDotProduct;
        private bool _isSprinting;
        private bool _isJumping;
        private bool _isCrouching;
        private bool _isGrounded;

        private void Awake()
        {
            OnValidate();
            
            _playerControls = new PlayerControls();
            _playerRigidbody = GetComponent<Rigidbody>();
            _playerControls.Player.SetMovementCallbacks(this);
            _playerControls.Player.SetSprintCallbacks(this);
            _playerControls.Player.SetJumpCallbacks(this);
            _playerControls.Player.SetCrouchCallbacks(this);
        }

        private void FixedUpdate()
        {
            InitializeSimulations();
            Jump();
            AdjustVelocity();
            ResetSimulations();
        }

        private void OnValidate()
        {
            _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        }

        private void InitializeSimulations()
        {
            _velocity = _playerRigidbody.velocity;

            if (_isGrounded)
            {
                _contactNormal.Normalize();
            }
            else
            {
                _contactNormal = Vector3.up;
            }
        }

        private void ResetSimulations()
        {
            _playerRigidbody.velocity = _velocity;
            _isGrounded = false;
            _contactNormal = Vector3.zero;
        }

        private void Jump()
        {
            if (!_isJumping || !_isGrounded) return;
            
            _isJumping = false;
            var jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            var alignedSpeed = Vector3.Dot(_velocity, _contactNormal);
            if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);

            _velocity += _contactNormal * jumpSpeed;
        }

        private void EvaluateCollision(Collision collision)
        {
            for (var i = 0; i < collision.contactCount; i++)
            {
                var normal = collision.GetContact(i).normal;
                if (normal.y >= _minGroundDotProduct) {
                    _isGrounded = true;
                    _contactNormal += normal;
                }
            }
        }

        private void AdjustVelocity()
        {
            var speed = _isSprinting ? sprintSpeed : walkingSpeed;
            
            var xAxis = ProjectOnContactPlane(Vector3.right).normalized;
            var zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

            var currentX = Vector3.Dot(_velocity, xAxis);
            var currentZ = Vector3.Dot(_velocity, zAxis);
            
            var desiredVelocity = new Vector3(_targetMove.x, 0f, _targetMove.y) * speed;
            var acceleration = _isGrounded ? maxAcceleration : maxJumpAcceleration;
            var maxSpeedChange = acceleration * Time.deltaTime;
            
            var newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
            var newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);
            
            _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }
        
        private Vector3 ProjectOnContactPlane(Vector3 vector)
        {
            return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
        }

        public void OnCollisionEnter(Collision collision)
        {
            EvaluateCollision(collision);
        }
        
        public void OnCollisionStay(Collision collision)
        {
            EvaluateCollision(collision);
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            var rawInputValue = context.ReadValue<Vector2>();
            _targetMove = Vector2.ClampMagnitude(rawInputValue, 1f);
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.ReadValueAsButton();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            _isCrouching = context.ReadValueAsButton() && _isGrounded;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            _isJumping = context.ReadValueAsButton();
        }
    
        private void OnEnable()
        {
            _playerControls.Enable();
        }

        private void OnDisable()
        {
            _playerControls.Disable();
        }
    }
}