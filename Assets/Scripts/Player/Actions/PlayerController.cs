using Inputs;
using Interfaces;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;
using World.Gravity;

namespace Player.Actions
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour, IPlayerActionMovement, IPlayerActionSprint, IPlayerActionJump,
        IPlayerActionCrouch, IPlayerActionSwim
    {
        [Header("Walk Settings")]
        [Tooltip("Toggle player walking")]
        [SerializeField] private BoolVariable enableWalk;
        [Tooltip("Walking speed (m/s)")]
        [SerializeField] private FloatConstant walkSpeed;
        [Tooltip("Walk acceleration")]
        [SerializeField] private FloatConstant walkAcceleration;
        [Tooltip("Max angle in which the player can climb naturally")]
        [SerializeField] private FloatConstant maxGroundAngle;
        [Tooltip("Max angle in which the player can climb stairs")]
        [SerializeField] private FloatConstant maxStairsAngle;
        
        [Header("Sprint Settings")]
        [Tooltip("Toggle player sprinting")]
        [SerializeField] private BoolVariable enableSprint;
        [Tooltip("Sprinting speed (m/s)")]
        [SerializeField] private FloatConstant sprintSpeed;
        [Tooltip("Sprint acceleration")]
        [SerializeField] private FloatConstant sprintAcceleration;

        [Header("Crouch Settings")]
        [Tooltip("Toggle player crouching")]
        [SerializeField] private BoolVariable enableCrouch;
        [Tooltip("Crouch height in meters")]
        [SerializeField] private FloatConstant crouchHeight;
        [Tooltip("Crouching speed (m/s)")]
        [SerializeField] private FloatConstant crouchSpeed;
        
        [Header("Jump Settings")]
        [Tooltip("Toggle player jumping")]
        [SerializeField] private BoolVariable enableJump;
        [Tooltip("Player jump acceleration")]
        [SerializeField] private FloatConstant jumpAcceleration;
        [Tooltip("Jump height in meters")]
        [SerializeField] private FloatConstant jumpHeight;
        [Tooltip("Maximum amount of air jumps")]
        [SerializeField] private IntConstant maxAirJumps;

        [Header("Climb Settings")]
        [Tooltip("Toggle player climbing")]
        [SerializeField] private BoolVariable enableClimb;
        [Tooltip("Climbing speed (m/s)")]
        [SerializeField] private FloatConstant climbSpeed;
        [Tooltip("Player climb acceleration")]
        [SerializeField] private FloatConstant climbAcceleration;
        [Tooltip("Maximum angle in which the player can climb")]
        [SerializeField] private FloatConstant maxClimbAngle;
        
        [Header("Swim Settings")]
        [Tooltip("Toggle player swimming")]
        [SerializeField] private BoolVariable enableSwim;
        [Tooltip("Threshold in which the player is considered swimming")]
        [SerializeField] private FloatConstant swimThreshold;
        [Tooltip("When the player is considered submerged")]
        [SerializeField] private FloatConstant submergenceOffset;
        [Tooltip("The maximum range of submergence probe")]
        [SerializeField] private FloatConstant submergenceProbe;
        [Tooltip("Maximum swim speed (m/s)")]
        [SerializeField] private FloatConstant swimSpeed;
        [Tooltip("Player swim acceleration")]
        [SerializeField] private FloatConstant swimAcceleration;
        [Tooltip("Drag applied when in water")]
        [SerializeField] private FloatConstant waterDrag;
        [Tooltip("The buoyancy of the player (Zero value sinks)")]
        [SerializeField] private FloatConstant buoyancy;

        [Header("Ground Settings")]
        [Tooltip("Alignment speed of the up direction based on gravity")]
        [SerializeField] private FloatConstant upAlignmentSpeed;
        [Tooltip("Max speed for ground snapping")]
        [SerializeField] private FloatConstant maxSnapSpeed;
        [Tooltip("Distance in which to check for ground in order to snap to")]
        [SerializeField] private FloatConstant groundCheckDistance;

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
        private bool IsSwimming => _submergence >= swimThreshold.Value;

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
            _shouldClimb = !IsSwimming && enableClimb.Value;
            var gravity = DefaultGravity.GetGravity(_playerRigidbody.position, out _upAxis);

            UpdateState();
            
            if (IsInWater)
                _velocity *= 1f - waterDrag.Value * _submergence * Time.deltaTime;

            UpdateGravityAlignment();
            AdjustVelocity();
            
            Crouch();
            if (_isJumping && !_isCrouching && !IsSwimming)
            {
                _isJumping = false;
                Jump(gravity);
            }

            if (IsClimbing)
                _velocity -= _contactNormal * (climbAcceleration.Value * 0.9f * Time.deltaTime);
            else if (IsInWater)
                _velocity += gravity * ((1f - buoyancy.Value * _submergence) * Time.deltaTime);
            else if (IsGrounded && _velocity.sqrMagnitude < 0.05f)
                _velocity += _contactNormal * (Vector3.Dot(gravity, _contactNormal) * Time.deltaTime);
            else if (_shouldClimb && IsGrounded)
                _velocity += (gravity - _contactNormal * (climbAcceleration.Value * 0.9f)) * Time.deltaTime;
            else
                _velocity += gravity * Time.deltaTime;
            
            _playerRigidbody.velocity = _velocity;
            transform.SetPositionAndRotation(transform.position, _gravityAlignment);

            ResetState();
        }

        private void OnValidate()
        {
            _minGroundDotProduct = Mathf.Cos(maxGroundAngle.Value * Mathf.Deg2Rad);
            _minStairsDotProduct = Mathf.Cos(maxStairsAngle.Value * Mathf.Deg2Rad);
            _minClimbDotProduct = Mathf.Cos(maxClimbAngle.Value * Mathf.Deg2Rad);
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
                case true when !Mathf.Approximately(_playerCollider.height, crouchHeight.Value):
                {
                    _playerCollider.height = crouchHeight.Value;
                
                    var center = _playerCollider.center;
                    _playerCollider.center = new Vector3(center.x, 0.25f, center.z);
                    break;
                }
                case false when Mathf.Approximately(_playerCollider.height, crouchHeight.Value):
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
            else if (maxAirJumps.Value > 0 && _jumpPhase <= maxAirJumps.Value)
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
            
            var jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight.Value);

            if (IsInWater)
                jumpSpeed *= Mathf.Max(0f, 1f - _submergence / swimThreshold.Value);
            
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

            if (IsClimbing)
            {
                acceleration = climbAcceleration.Value;
                speed = climbSpeed.Value;
                xAxis = Vector3.Cross(_contactNormal, _upAxis);
                zAxis = _upAxis;
            }
            else if (IsInWater)
            {
                var swimFactor = Mathf.Min(1f, _submergence / swimThreshold.Value);
                acceleration = Mathf.LerpUnclamped(
                    IsGrounded ? walkAcceleration.Value : jumpAcceleration.Value,
                    swimAcceleration.Value, swimFactor);

                speed = Mathf.LerpUnclamped(walkSpeed.Value, swimSpeed.Value, swimFactor);
                xAxis = _rightAxis;
                zAxis = _forwardAxis;
            }
            else
            {
                acceleration = IsGrounded ? walkAcceleration.Value : jumpAcceleration.Value;
                speed = _isSprinting ? sprintSpeed.Value : walkSpeed.Value;
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
        }
        
        private void UpdateGravityAlignment()
        {
            var fromUp = _gravityAlignment * Vector3.up;
            var toUp = DefaultGravity.GetUpAxis(transform.position);

            var dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);
            var angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            var maxAngle = upAlignmentSpeed.Value * Time.deltaTime;

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
            if (speed > maxSnapSpeed.Value) return false;

            if (!Physics.Raycast(_playerRigidbody.position, -_upAxis, out var hit, groundCheckDistance.Value, groundLayer, QueryTriggerInteraction.Ignore)) return false;

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
                _playerRigidbody.position + _upAxis * submergenceOffset.Value,
                -_upAxis, out var hit, submergenceProbe.Value + 1f,
                waterLayer, QueryTriggerInteraction.Collide
            ))
            {
                _submergence = 1f - hit.distance / submergenceProbe.Value;
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