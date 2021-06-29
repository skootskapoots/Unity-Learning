using Inputs;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Actions
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

        [Header("Component Registry")]
        [SerializeField] private CharacterController characterController;

        private PlayerControls _playerControls;
        private Vector2 _currentMove;
        private Vector2 _targetMove;
        private Vector3 _velocity;
        private bool _isSprinting;

        private void Awake()
        {
            _playerControls = new PlayerControls();
            _playerControls.Player.SetMovementCallbacks(this);
            _playerControls.Player.SetSprintCallbacks(this);
            _playerControls.Player.SetJumpCallbacks(this);
        }

        private void Update()
        {
            _velocity.y += Physics.gravity.y * gravityFactor * Time.deltaTime;

            var position = transform;
            var zeroVelocity = Vector2.zero;

            _currentMove = Vector2.SmoothDamp(_currentMove, _targetMove, ref zeroVelocity, movementSpeedSmoothing);

            var move = position.right * _currentMove.x + position.forward * _currentMove.y;
            var speed = _isSprinting ? movementSpeedFactor * sprintSpeedMultiplier : movementSpeedFactor;

            characterController.Move(move * (speed * Time.deltaTime) + _velocity * Time.deltaTime);
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            _targetMove = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (characterController.isGrounded)
            {
                _isSprinting = context.ReadValue<float>() > 0;
            }
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() > 0 && characterController.isGrounded)
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