using System;
using System.Linq;
using Inputs;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerActions
{
    public class MouseLook : MonoBehaviour, IPlayerActionLook
    {
        [Header("Cursor Settings")]
        [SerializeField] private bool isCursorLocked = true;

        [Header("Mouse Settings")]
        [SerializeField] private float mouseSensitivity = 100f;

        [Header("Camera Settings")]
        [SerializeField] private float fieldOfView = 75f;
        [SerializeField] private float minClamp = -90f;
        [SerializeField] private float maxClamp = 90f;

        [Header("Component Registry")]
        [SerializeField] private Transform playerGameObject;
        [SerializeField] private Camera playerCamera;
        
        private PlayerControls _playerControls;
        private Vector2 _mouseInput;
        private float _xRotation = 0f;
        
        private void Awake()
        {
            _playerControls = new PlayerControls();
            _playerControls.Player.SetLookCallbacks(this);
            
        }
        
        private void Start()
        {
            if (isCursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            
            if (playerCamera)
            {
                playerCamera.fieldOfView = fieldOfView;
            }
        }

        private void Update()
        {
            Look();
        }

        private void Look()
        {
            var mouseX = _mouseInput.x * mouseSensitivity * Time.deltaTime;
            var mouseY = _mouseInput.y * mouseSensitivity * Time.deltaTime;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, minClamp, maxClamp);
            
            transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            playerGameObject.Rotate(Vector3.up * mouseX);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            _mouseInput = context.ReadValue<Vector2>();
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