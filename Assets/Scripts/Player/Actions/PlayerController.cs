using Inputs;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using World.Gravity;

namespace Player.Actions
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour, IPlayerActionMovement, IPlayerActionSprint, IPlayerActionJump,
        IPlayerActionCrouch, IPlayerActionSwim
    {
        [Header("Movement Settings")]
        [Tooltip("Walking speed (m/s)")]
        [SerializeField] private float walkingSpeed = 4f;
        [Tooltip("Sprinting speed (m/s)")]
        [SerializeField] private float sprintSpeed = 6f;
        [Tooltip("Maximum player acceleration and deceleration")]
        [SerializeField] private float maxAcceleration = 10f;
        [Tooltip("Alignment speed of the up direction based on gravity")]
        [SerializeField, Min(0f)] private float upAlignmentSpeed = 360f;
        
        [Header("Crouch Settings")]
        [Tooltip("Crouch height in meters")]
        [SerializeField] private float crouchHeight = 1.2f;
        [Tooltip("Crouching speed (m/s)")]
        [SerializeField] private float crouchSpeed = 2f;

        [Header("Climb Settings")]
        [Tooltip("Enable player climbing")]
        [SerializeField] private bool enableClimb = true;
        [Tooltip("Maximum angle in which the player can climb")]
        [SerializeField, Range(90f, 180f)] private float maxClimbAngle = 140f;
        [Tooltip("Climbing speed (m/s)")]
        [SerializeField] private float climbSpeed = 2f;
        [Tooltip("Maximum player climb acceleration")]
        [SerializeField] private float maxClimbAcceleration = 20f;

        [Header("Jump Settings")]
        [Tooltip("Jump height in meters")]
        [SerializeField] private float jumpHeight = 1.2f;
        [Tooltip("Maximum player jump acceleration")]
        [SerializeField] private float maxJumpAcceleration = 1f;
        [Tooltip("Maximum amount of air jumps")]
        [SerializeField, Range(0, 5)] private int maxAirJumps = 1;
        
        [Header("Swimming Settings")]
        [Tooltip("Maximum swim speed (m/s)")]
        [SerializeField] private float maxSwimSpeed = 5f;
        [Tooltip("Maximum swim acceleration")]
        [SerializeField] private float maxSwimAcceleration = 5f;
        [Tooltip("When the player is considered submerged")]
        [SerializeField] private float submergenceOffset = 0.5f;
        [Tooltip("The maximum range of submergence")]
        [SerializeField, Min(0.1f)] private float submergenceRange = 1f;
        [Tooltip("Drag applied when in water")]
        [SerializeField, Range(0f, 10f)] private float waterDrag = 1f;
        [Tooltip("The buoyancy of the player (Zero value sinks)")]
        [SerializeField, Min(0f)] private float buoyancy = 1f;
        [Tooltip("Threshold in which the player is considered swimming")]
        [SerializeField, Range(0.01f, 1f)] private float swimThreshold = 0.5f;
        
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
        [SerializeField] private LayerMask climbLayer;
        [SerializeField] private LayerMask waterLayer;
        [SerializeField] private Transform playerInputSpace;

        private bool IsGrounded => _groundContactCount > 0;
        private bool IsSteep => _steepContactCount > 0;
        
        private PlayerControls _playerControls;
        private Rigidbody _playerRigidbody;
        private Rigidbody _connectedBody;
        private Rigidbody _previousConnectedBody;
        private CapsuleCollider _playerCollider;
        private Quaternion _gravityAlignment = Quaternion.identity;
        private Vector3 _velocity;
        private Vector3 _desiredVelocity;
        private Vector3 _gravity;
        private Vector3 _contactNormal;
        private Vector3 _steepNormal;
        private Vector3 _climbNormal;
        private Vector3 _lastClimbNormal;
        private Vector3 _connectedVelocity;
        private Vector3 _connectedWorldPosition;
        private Vector3 _connectedLocalPosition;
        private Vector3 _initialColliderCenter;
        private Vector3 _upAxis;
        private Vector3 _rightAxis;
        private Vector3 _forwardAxis;
        private Vector2 _targetMove;
        private Vector2 _targetSwimMove;
        private float _initialColliderHeight;
        private float _minGroundDotProduct;
        private float _minStairsDotProduct;
        private float _minClimbDotProduct;
        private float _submergence;
        private int _stepsSinceGrounded;
        private int _stepsSinceLastJump;
        private int _groundContactCount;
        private int _steepContactCount;
        private int _climbContactCount;
        private int _jumpPhase;
        private bool _shouldClimb;
        private bool _isSprinting;
        private bool _isJumping;
        private bool _isCrouching;

        private bool IsClimbing => _climbContactCount > 0 && _stepsSinceLastJump > 2;
        private bool IsInWater => _submergence > 0f;
        private bool IsSwimming => _submergence >= swimThreshold;

        private void Awake()
        {
            _playerControls = new PlayerControls();
            _playerRigidbody = GetComponent<Rigidbody>();
            _playerCollider = GetComponent<CapsuleCollider>();
            _playerControls.Player.SetMovementCallbacks(this);
            _playerControls.Player.SetSprintCallbacks(this);
            _playerControls.Player.SetJumpCallbacks(this);
            _playerControls.Player.SetCrouchCallbacks(this);
            _playerControls.Player.SetSwimCallbacks(this);
            
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
        }

        private void FixedUpdate()
        {
            _shouldClimb = !IsSwimming && enableClimb;
            _gravity = DefaultGravity.GetGravity(_playerRigidbody.position, out _upAxis);

            UpdateState();

            UpdateGravityAlignment();
            AdjustVelocity();
            
            Crouch();
            if (_isJumping && !_isCrouching && !IsSwimming)
            {
                _isJumping = false;
                Jump(_gravity);
            }

            _playerRigidbody.velocity = _velocity;
            transform.SetPositionAndRotation(transform.position, _gravityAlignment);

            ResetState();
        }

        private void OnValidate()
        {
            _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            _minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
            _minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
        }

        private void UpdateState()
        {
            _stepsSinceGrounded++;
            _stepsSinceLastJump++;
            _velocity = _playerRigidbody.velocity;

            if (CheckClimbing() || CheckSwimming() || IsGrounded || SnapToGround() || CheckSteepContacts())
            {
                _stepsSinceGrounded = 0;
                if (_stepsSinceLastJump > 1) _jumpPhase = 0;
                if (_groundContactCount > 1) _contactNormal.Normalize();
            }
            else
            {
                _contactNormal = _upAxis;
            }
            
            if (_connectedBody)
                if (_connectedBody.isKinematic || _connectedBody.mass >= _playerRigidbody.mass)
                    UpdateConnectedState();
        }

        private void ResetState()
        {
            _playerRigidbody.velocity = _velocity;
            _groundContactCount = 0;
            _steepContactCount = 0;
            _climbContactCount = 0;
            _submergence = 0f;
            _contactNormal = Vector3.zero;
            _steepNormal = Vector3.zero;
            _climbNormal = Vector3.zero;
            _connectedVelocity = Vector3.zero;
            _previousConnectedBody = _connectedBody;
            _connectedBody = null;
        }
        
        private void UpdateConnectedState()
        {
            if (_connectedBody == _previousConnectedBody)
            {
                var connectionMovement = _connectedBody.transform.TransformPoint(_connectedLocalPosition) - _connectedWorldPosition;
                _connectedVelocity = connectionMovement / Time.deltaTime;
            }
            
            _connectedWorldPosition = _playerRigidbody.position;
            _connectedLocalPosition = _connectedBody.transform.InverseTransformPoint(_connectedWorldPosition);
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

            if (IsInWater)
                jumpSpeed *= Mathf.Max(0f, 1f - _submergence / swimThreshold);
            
            var alignedSpeed = Vector3.Dot(_velocity, jumpDirection);
            if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);

            _velocity += jumpDirection * jumpSpeed;
        }

        private void AdjustVelocity()
        {
            Vector3 xAxis;
            Vector3 zAxis;
            float speed;
            float acceleration;
            
            if (IsInWater)
                _velocity *= 1f - waterDrag * _submergence * Time.deltaTime;

            if (IsClimbing)
            {
                acceleration = maxClimbAcceleration;
                speed = climbSpeed;
                xAxis = Vector3.Cross(_contactNormal, _upAxis);
                zAxis = _upAxis;
            }
            else if (IsInWater)
            {
                var swimFactor = Mathf.Min(1f, _submergence / swimThreshold);
                acceleration = Mathf.LerpUnclamped(
                    IsGrounded ? maxAcceleration : maxJumpAcceleration,
                    maxSwimAcceleration, swimFactor);

                speed = Mathf.LerpUnclamped(walkingSpeed, maxSwimSpeed, swimFactor);
                xAxis = _rightAxis;
                zAxis = _forwardAxis;
            }
            else
            {
                acceleration = IsGrounded ? maxAcceleration : maxJumpAcceleration;
                speed = _isSprinting ? sprintSpeed : walkingSpeed;
                xAxis = _rightAxis;
                zAxis = _forwardAxis;
            }
            
            _desiredVelocity = new Vector3(_targetMove.x, 0f, _targetMove.y) * speed;

            xAxis = ProjectDirectionOnPlane(xAxis, _contactNormal);
            zAxis = ProjectDirectionOnPlane(zAxis, _contactNormal);

            var relativeVelocity = _velocity - _connectedVelocity;
            var currentX = Vector3.Dot(relativeVelocity, xAxis);
            var currentZ = Vector3.Dot(relativeVelocity, zAxis);
            
            var maxSpeedChange = acceleration * Time.deltaTime;
            
            var newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
            var newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);
            
            _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

            if (IsSwimming)
            {
                var currentY = Vector3.Dot(relativeVelocity, _upAxis);
                var newY = Mathf.MoveTowards(
                    currentY, _targetSwimMove.y * speed, maxSpeedChange);

                _velocity += _upAxis * (newY - currentY);
            }
            
            if (IsClimbing)
                _velocity -= _contactNormal * (maxClimbAcceleration * 0.9f * Time.deltaTime);
            else if (IsInWater)
                _velocity += _gravity * ((1f - buoyancy * _submergence) * Time.deltaTime);
            else if (IsGrounded && _velocity.sqrMagnitude < 0.05f)
                _velocity += _contactNormal * (Vector3.Dot(_gravity, _contactNormal) * Time.deltaTime);
            else if (_shouldClimb && IsGrounded)
                _velocity += (_gravity - _contactNormal * (maxClimbAcceleration * 0.9f)) * Time.deltaTime;
            else
                _velocity += _gravity * Time.deltaTime;
        }
        
        private void UpdateGravityAlignment()
        {
            var fromUp = _gravityAlignment * Vector3.up;
            var toUp = DefaultGravity.GetUpAxis(transform.position);

            var dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);
            var angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            var maxAngle = upAlignmentSpeed * Time.deltaTime;

            var newAlignment = Quaternion.FromToRotation(fromUp, toUp) * _gravityAlignment;

            _gravityAlignment = angle <= maxAngle ? newAlignment : Quaternion.SlerpUnclamped(_gravityAlignment, newAlignment, maxAngle / angle);
        }
        
        private static Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
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

            if (!Physics.Raycast(_playerRigidbody.position, -_upAxis, out var hit, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore)) return false;

            var upDot = Vector3.Dot(_upAxis, hit.normal);
            if (upDot < GetMinDot(hit.collider.gameObject.layer)) return false;

            _groundContactCount++;
            _contactNormal = hit.normal;
            
            var dot = Vector3.Dot(_velocity, hit.normal);
            if (dot > 0f) _velocity = (_velocity - hit.normal * dot).normalized * speed;

            _connectedBody = hit.rigidbody;
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

        private bool CheckClimbing()
        {
            if (!IsClimbing) return false;
            
            if (_climbContactCount > 1)
            {
                _climbNormal.Normalize();
                var upDot = Vector3.Dot(_upAxis, _climbNormal);
                if (upDot >= _minGroundDotProduct)
                    _climbNormal = _lastClimbNormal;
            }
            _groundContactCount = 1;
            _contactNormal = _climbNormal;
            return true;

        }

        private bool CheckSwimming()
        {
            if (!IsSwimming) return false;
            
            _groundContactCount = 0;
            _contactNormal = _upAxis;
            return true;
        }

        private void EvaluateCollision(Collision collision)
        {
            if (IsSwimming)
                return;
            
            for (var i = 0; i < collision.contactCount; i++)
            {
                var layer = collision.gameObject.layer;
                var minDot = GetMinDot(layer);
                var normal = collision.GetContact(i).normal;
                var upDot = Vector3.Dot(_upAxis, normal);
                if (upDot >= minDot)
                {
                    _groundContactCount++;
                    _contactNormal += normal;
                    _connectedBody = collision.rigidbody;
                }
                else
                {
                    if (upDot > -0.01f)
                    {
                        _steepContactCount++;
                        _steepNormal += normal;
                        if (_groundContactCount == 0)
                            _connectedBody = collision.rigidbody;
                    }

                    if (_shouldClimb && upDot >= _minClimbDotProduct && (climbLayer & (1 << layer)) != 0)
                    {
                        _climbContactCount++;
                        _climbNormal += normal;
                        _lastClimbNormal = normal;
                        _connectedBody = collision.rigidbody;
                    }
                }
            }
        }

        private void EvaluateSubmergence()
        {
            if (Physics.Raycast(
                _playerRigidbody.position + _upAxis * submergenceOffset,
                -_upAxis, out var hit, submergenceRange + 1f,
                waterLayer, QueryTriggerInteraction.Collide
            ))
            {
                _submergence = 1f - hit.distance / submergenceRange;
            }
            else
            {
                _submergence = 1f;
            }

            if (IsSwimming)
                _connectedBody = _playerCollider.attachedRigidbody;
        }

        public void OnCollisionEnter(Collision collision)
        {
            EvaluateCollision(collision);
        }
        
        public void OnCollisionStay(Collision collision)
        {
            EvaluateCollision(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((waterLayer & (1 << other.gameObject.layer)) != 0)
                EvaluateSubmergence();
        }

        private void OnTriggerStay(Collider other)
        {
            if ((waterLayer & (1 << other.gameObject.layer)) != 0)
                EvaluateSubmergence();
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

        public void OnSwim(InputAction.CallbackContext context)
        {
            var rawInputValue = context.ReadValue<Vector2>();
            _targetSwimMove = Vector2.ClampMagnitude(rawInputValue, 1f);
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