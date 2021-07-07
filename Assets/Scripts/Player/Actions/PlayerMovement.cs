using System;
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
        [SerializeField] private float movementSpeedFactor = 6f;
        [SerializeField] private float movementSpeedSmoothing = 0.02f;
        [SerializeField] private float sprintSpeedMultiplier = 1.5f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 100f;
        [Header("Crouch Settings")]
        [SerializeField] private Vector3 crouchPosition = new Vector3(0f, -0.25f, 0f);
        [SerializeField] private float crouchSpeedMultiplier = 0.4f;

        [Header("Component Registry")]
        [SerializeField] private Rigidbody playerRigid;
        [SerializeField] private GameObject playerBody;
        [SerializeField] private LayerMask environmentLayer;

        private PlayerControls _playerControls;
        private Vector2 _targetMove;
        private bool _isSprinting;
        private bool _isCrouching;
        private bool _isGrounded;

        private void Awake()
        {
            _playerControls = new PlayerControls();
            _playerControls.Player.SetMovementCallbacks(this);
            _playerControls.Player.SetSprintCallbacks(this);
            _playerControls.Player.SetJumpCallbacks(this);
            _playerControls.Player.SetCrouchCallbacks(this);
        }

        private void Update()
        {
            // GetStance();
        }

        private void FixedUpdate()
        {
            GetMove();
        }

        private void GetMove()
        {
            var currentTransform = transform;
            var move = currentTransform.right * _targetMove.x + currentTransform.forward * _targetMove.y;
            
            playerRigid.MovePosition(currentTransform.position + move * (Time.deltaTime * GetSpeed()));
        }

        private void GetStance()
        {
            playerBody.transform.localPosition = _isCrouching ? crouchPosition : Vector3.zero;
        }

        private float GetSpeed()
        {
            var speed = movementSpeedFactor;

            if (_isSprinting) speed *= sprintSpeedMultiplier;
            if (_isCrouching) speed *= crouchSpeedMultiplier;

            return speed;
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == environmentLayer) _isGrounded = true;
        }
        
        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.layer == environmentLayer) _isGrounded = false;
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            _targetMove = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.ReadValue<float>() > 0 && _isGrounded;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            _isCrouching = context.ReadValue<float>() > 0 && _isGrounded;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() > 0)
            {
                playerRigid.AddForce(Vector3.up * jumpForce);
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