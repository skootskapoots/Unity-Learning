using Inputs;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using World.Gravity;

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
        
        [Header("Crouch Settings")]
        [Tooltip("Jump height in meters")]
        [SerializeField] private float crouchHeight = 1.2f;
        [Tooltip("Crouching speed (m/s)")]
        [SerializeField] private float crouchSpeed = 2f;

        [Header("Jump Settings")]
        [Tooltip("Jump height in meters")]
        [SerializeField] private float jumpHeight = 1.2f;
        [Tooltip("Maximum player jump acceleration")]
        [SerializeField] private float maxJumpAcceleration = 1f;
        [Tooltip("Maximum amount of air jumps")]
        [SerializeField, Range(0, 5)] private int maxAirJumps = 1;
        
        [Header("Ground Settings")]
        [Tooltip("Max angle in which the player can climb naturally")]
        [SerializeField] private float maxGroundAngle = 25f;
        [Tooltip("Max angle in which the player can climb stairs")]
        [SerializeField] private float maxStairsAngle = 50f;
        [Tooltip("Max speed for ground snapping")]
        [SerializeField] private float maxSnapSpeed = 100f;
        [Tooltip("Distance in which to check for ground in order to snap to")]
        [SerializeField] private float groundCheckDistance = 0.6f;

        [Header("Component Registry")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask stairsLayer;
        [SerializeField] private Transform playerInputSpace;

        private bool IsGrounded => _groundContactCount > 0;
        private bool IsSteep => _steepContactCount > 0;
        
        private PlayerControls _playerControls;
        private Rigidbody _playerRigidbody;
        private CapsuleCollider _playerCollider;
        private Vector3 _velocity;
        private Vector3 _desiredVelocity;
        private Vector3 _contactNormal;
        private Vector3 _steepNormal;
        private Vector3 _initialColliderCenter;
        private Vector3 _upAxis;
        private Vector3 _rightAxis;
        private Vector3 _forwardAxis;
        private Vector2 _targetMove;
        private float _initialColliderHeight;
        private float _minGroundDotProduct;
        private float _minStairsDotProduct;
        private int _stepsSinceGrounded;
        private int _stepsSinceLastJump;
        private int _groundContactCount;
        private int _steepContactCount;
        private int _jumpPhase;
        private bool _isSprinting;
        private bool _isJumping;
        private bool _isCrouching;

        private void Awake()
        {
            _playerControls = new PlayerControls();
            _playerRigidbody = GetComponent<Rigidbody>();
            _playerCollider = GetComponent<CapsuleCollider>();
            _playerControls.Player.SetMovementCallbacks(this);
            _playerControls.Player.SetSprintCallbacks(this);
            _playerControls.Player.SetJumpCallbacks(this);
            _playerControls.Player.SetCrouchCallbacks(this);
            
            _playerRigidbody.useGravity = false;
            _initialColliderHeight = _playerCollider.height;
            _initialColliderCenter = _playerCollider.center;
            
            OnValidate();
        }

        private void Update()
        {
            if (playerInputSpace)
            {
                _rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, _upAxis);
                _forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, _upAxis);
            }
            else
            {
                _rightAxis = ProjectDirectionOnPlane(transform.right, _upAxis);
                _forwardAxis = ProjectDirectionOnPlane(transform.forward, _upAxis);
            }
            
            float speed;
            if (_isSprinting && !_isCrouching)
                speed = sprintSpeed;
            else if (_isCrouching && !_isSprinting)
                speed = crouchSpeed;
            else
                speed = walkingSpeed;
            
            _desiredVelocity = new Vector3(_targetMove.x, 0f, _targetMove.y) * speed;
        }

        private void FixedUpdate()
        {
            var gravity = DefaultGravity.GetGravity(_playerRigidbody.position, out _upAxis);
            
            InitializeSimulations();
            AdjustVelocity();
            Crouch();
            if (_isJumping && !_isCrouching)
            {
                _isJumping = false;
                Jump(gravity);
            }
            _velocity += gravity * Time.deltaTime;
            _playerRigidbody.velocity = _velocity;

            ResetSimulations();
        }

        private void OnValidate()
        {
            _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            _minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        }

        private void InitializeSimulations()
        {
            _stepsSinceGrounded++;
            _stepsSinceLastJump++;
            _velocity = _playerRigidbody.velocity;

            if (IsGrounded || SnapToGround() || CheckSteepContacts())
            {
                _stepsSinceGrounded = 0;
                if (_stepsSinceLastJump > 1) _jumpPhase = 0;
                if (_groundContactCount > 1) _contactNormal.Normalize();
            }
            else
            {
                _contactNormal = _upAxis;
            }
        }

        private void ResetSimulations()
        {
            _playerRigidbody.velocity = _velocity;
            _groundContactCount = 0;
            _steepContactCount = 0;
            _contactNormal = Vector3.zero;
            _steepNormal = Vector3.zero;
        }

        private void Crouch()
        {
            switch (_isCrouching)
            {
                case true when !Mathf.Approximately(_playerCollider.height, crouchHeight):
                {
                    _playerCollider.height = crouchHeight;
                
                    var center = _playerCollider.center;
                    _playerCollider.center = new Vector3(center.x, 0.25f, center.z);
                    break;
                }
                case false when Mathf.Approximately(_playerCollider.height, crouchHeight):
                {
                    _playerCollider.height = _initialColliderHeight;
                    _playerCollider.center = new Vector3(_initialColliderCenter.x, 0f, _initialColliderCenter.z);
                    break;
                }
            }
        }

        private void Jump(Vector3 gravity)
        {
            Vector3 jumpDirection;

            if (IsGrounded)
            {
                jumpDirection = _contactNormal;
            }
            else if (IsSteep)
            {
                jumpDirection = _steepNormal;
                _jumpPhase = 0;
            }
            else if (maxAirJumps > 0 && _jumpPhase <= maxAirJumps)
            {
                if (_jumpPhase == 0) _jumpPhase = 1;
                jumpDirection = _contactNormal;
            }
            else
            {
                return;
            }

            _stepsSinceLastJump = 0;
            _jumpPhase++;
            jumpDirection = (jumpDirection + _upAxis).normalized;
            
            var jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
            var alignedSpeed = Vector3.Dot(_velocity, jumpDirection);
            if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);

            _velocity += jumpDirection * jumpSpeed;
        }

        private void EvaluateCollision(Collision collision)
        {
            for (var i = 0; i < collision.contactCount; i++)
            {
                var minDot = GetMinDot(collision.gameObject.layer);
                var normal = collision.GetContact(i).normal;
                var upDot = Vector3.Dot(_upAxis, normal);
                if (upDot >= minDot)
                {
                    _groundContactCount++;
                    _contactNormal += normal;
                }
                else if (upDot > -0.01f)
                {
                    _steepContactCount++;
                    _steepNormal += normal;
                }
            }
        }

        private void AdjustVelocity()
        {
            var xAxis = ProjectDirectionOnPlane(_rightAxis, _contactNormal);
            var zAxis = ProjectDirectionOnPlane(_forwardAxis, _contactNormal);

            var currentX = Vector3.Dot(_velocity, xAxis);
            var currentZ = Vector3.Dot(_velocity, zAxis);
            
            var acceleration = IsGrounded ? maxAcceleration : maxJumpAcceleration;
            var maxSpeedChange = acceleration * Time.deltaTime;
            
            var newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
            var newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);
            
            _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }
        
        private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
        {
            return direction - normal * Vector3.Dot(direction, normal);
        }

        private float GetMinDot(LayerMask layer)
        {
            return (stairsLayer & (1 << layer)) == 0 ? _minGroundDotProduct : _minStairsDotProduct;
        }

        private bool SnapToGround()
        {
            if (_stepsSinceGrounded > 1 || _stepsSinceLastJump <= 2) return false;
            
            var speed = _velocity.magnitude;
            if (speed > maxSnapSpeed) return false;

            if (!Physics.Raycast(_playerRigidbody.position, -_upAxis, out var hit, groundCheckDistance, groundLayer)) return false;

            var upDot = Vector3.Dot(_upAxis, hit.normal);
            if (upDot < GetMinDot(hit.collider.gameObject.layer)) return false;

            _groundContactCount++;
            _contactNormal = hit.normal;
            
            var dot = Vector3.Dot(_velocity, hit.normal);
            if (dot > 0f) _velocity = (_velocity - hit.normal * dot).normalized * speed;
            
            return true;
        }

        private bool CheckSteepContacts()
        {
            if (_steepContactCount <= 1) return false;
            
            _steepNormal.Normalize();
            var upDot = Vector3.Dot(_upAxis, _steepNormal);
            if (!(upDot >= _minGroundDotProduct)) return false;
            
            _groundContactCount = 1;
            _contactNormal = _steepNormal;
            return true;

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
            _isCrouching = context.ReadValueAsButton() && IsGrounded;
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