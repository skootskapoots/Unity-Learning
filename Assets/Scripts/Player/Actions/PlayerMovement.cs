using Inputs;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Actions
{
    public class PlayerMovement : MonoBehaviour, IPlayerActionMovement, IPlayerActionSprint, IPlayerActionJump
    {
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeedFactor = 6f;
        [SerializeField] private float movementSpeedSmoothing = 0.02f;
        [SerializeField] private float sprintSpeedMultiplier = 1.5f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 3f;
        [SerializeField] private float gravityFactor = 2f;
        [SerializeField] private float groundCheckDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;

        [Header("Component Registry")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform groundCheck;

        private PlayerControls _playerControls;
        private Vector2 _currentMove;
        private Vector2 _targetMove;
        private Vector3 _velocity;
        private bool _isSprinting;
        private bool _isGrounded;

        private void Awake()
        {
            _playerControls = new PlayerControls();
            _playerControls.Player.SetMovementCallbacks(this);
            _playerControls.Player.SetSprintCallbacks(this);
            _playerControls.Player.SetJumpCallbacks(this);
        }

        private void Update()
        {
            MovementUpdate();
            JumpUpdate();
        }

        
        private void MovementUpdate()
        {
            var position = transform;
            var zeroVelocity = Vector2.zero;

            _currentMove = Vector2.SmoothDamp(_currentMove, _targetMove, ref zeroVelocity, movementSpeedSmoothing);
            
            var move = position.right * _currentMove.x + position.forward * _currentMove.y;
            var speed = _isSprinting ? movementSpeedFactor * sprintSpeedMultiplier : movementSpeedFactor;
            characterController.Move(move * (speed * Time.deltaTime));
        }

        private void JumpUpdate()
        {
            _isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);

            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            
            _velocity.y += Physics.gravity.y * gravityFactor * Time.deltaTime;
            characterController.Move(_velocity * Time.deltaTime);
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            _targetMove = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.ReadValue<float>() > 0;
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() > 0 && _velocity.y < 0)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            }
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