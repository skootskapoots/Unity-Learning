using Inputs;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerActions
{
    public class PlayerMovement : MonoBehaviour, IPlayerActionMovement, IPlayerActionSprint
    {
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeedFactor = 6f;
        [SerializeField] private float movementSpeedSmoothing = 0.02f;
        [SerializeField] private float sprintSpeedMultiplier = 1.5f;

        [Header("Component Registry")]
        [SerializeField] private CharacterController characterController;

        private PlayerControls _playerControls;
        private Vector2 _currentMove;
        private Vector2 _targetMove;
        private bool _isSprinting;

        private void Awake()
        {
            _playerControls = new PlayerControls();
            _playerControls.Player.SetMovementCallbacks(this);
            _playerControls.Player.SetSprintCallbacks(this);
        }

        private void Update()
        {
            MovementUpdate();
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

        public void OnMovement(InputAction.CallbackContext context)
        {
            _targetMove = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.ReadValue<float>() > 0;
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