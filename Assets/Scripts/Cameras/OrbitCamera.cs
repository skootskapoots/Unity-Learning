using System;
using Inputs;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using World.Gravity;

namespace Cameras
{
    [RequireComponent(typeof(Camera))]
    public class OrbitCamera : MonoBehaviour, IPlayerActionLook
    {
        [Header("Cursor Settings")]
        [SerializeField] private bool isCursorLocked = true;
        
        [Header("Camera Settings")]
        [Tooltip("The distance the camera should maintain from the target")]
        [SerializeField, Range(1f, 20f)] private float followDistance = 17f;
        [Tooltip("Radius of movement until the camera moves with the target")]
        [SerializeField, Min(0f)] private float followRadius = 1f;
        [Tooltip("Factoring for centering the camera on the target")]
        [SerializeField, Range(0f, 1f)] private float targetCentering = 0.5f;
        
        [Header("Orbit Settings")]
        [Tooltip("Invert the vertical pitch controls")]
        [SerializeField] private bool isPitchInverted = true;
        [Tooltip("Set the rotational speed of the orbit in degrees per second")]
        [SerializeField, Range(1f, 360f)] private float rotationSpeed = 90f;
        [Tooltip("Constrain the minimum vertical angle of the camera")]
        [SerializeField, Range(-89f, 89f)] private float minVerticalAngle = -30f;
        [Tooltip("Constrain the maximum vertical angle of the camera")]
        [SerializeField, Range(-89f, 89f)] private float maxVerticalAngle = 60f;
        [Tooltip("Alignment delay in seconds before centering behind the player")]
        [SerializeField, Min(0f)] private float alignDelay = 5f;
        [Tooltip("Set the alignment smoothing in degrees per second")]
        [SerializeField, Range(0f, 90f)] private float alignSmoothing = 45f;
        [Tooltip("Alignment speed of the up direction based on gravity")]
        [SerializeField, Min(0f)] private float upAlignmentSpeed = 360f;
        
        [Header("Component Registry")]
        [SerializeField] private Transform target;
        [SerializeField] private LayerMask obstructionMask;

        private PlayerControls _playerControls;
        private Camera _regularCamera;
        private Quaternion _gravityAlignment = Quaternion.identity;
        private Quaternion _orbitRotation;
        private Vector3 _focusPoint;
        private Vector3 _previousFocusPoint;
        private Vector2 _mouseInput;
        private Vector2 _orbitAngles = new Vector2(25f, 0f);
        private float _lastManualRotationTime;

        private Vector3 CameraHalfExtends
        {
            get
            {
                Vector3 halfExtends;
                halfExtends.y = _regularCamera.nearClipPlane *
                                Mathf.Tan(0.5f * Mathf.Deg2Rad * _regularCamera.fieldOfView);
                halfExtends.x = halfExtends.y * _regularCamera.aspect;
                halfExtends.z = 0f;
                return halfExtends;
            }
        }

        private void Awake()
        {
            _playerControls = new PlayerControls();
            _playerControls.Player.SetLookCallbacks(this);

            _regularCamera = GetComponent<Camera>();
            
            _focusPoint = target.position;
            transform.localRotation = _orbitRotation * Quaternion.Euler(_orbitAngles);
        }

        private void Start()
        {
            if (!isCursorLocked) return;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            UpdateGravityAlignment();
            UpdateFocusPoint();

            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                _orbitRotation = Quaternion.Euler(_orbitAngles);
            }

            var lookRotation = _gravityAlignment * _orbitRotation;
            var lookDirection = lookRotation * Vector3.forward;
            var lookPosition = _focusPoint - lookDirection * followDistance;

            var rectOffset = lookDirection * _regularCamera.nearClipPlane;
            var rectPosition = lookPosition + rectOffset;
            var castFrom = target.position;
            var castLine = rectPosition - castFrom;
            var castDistance = castLine.magnitude;
            var castDirection = castLine / castDistance;

            if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out var hit, lookRotation, castDistance, obstructionMask))
            {
                rectPosition = castFrom + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }

            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        private void OnValidate()
        {
            if (maxVerticalAngle < minVerticalAngle)
                maxVerticalAngle = minVerticalAngle;
        }

        private void UpdateFocusPoint()
        {
            _previousFocusPoint = _focusPoint;
            var targetPoint = target.position;

            if (followDistance > 0f)
            {
                var distance = Vector3.Distance(targetPoint, _focusPoint);
                var t = 1f;
                if (distance > 0.01f && targetCentering > 0f)
                    t = Mathf.Pow(1f - targetCentering, Time.unscaledDeltaTime);

                if (distance > followRadius)
                    t = Mathf.Min(t, followRadius / distance);

                _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
            }
            else
            {
                _focusPoint = targetPoint;
            }
        }

        private bool ManualRotation()
        {
            const float e = 0.001f;
            if (_mouseInput.x < -e || _mouseInput.x > e || _mouseInput.y < -e || _mouseInput.y > e)
            {
                var inputValues = new Vector2(isPitchInverted ? -_mouseInput.y : _mouseInput.y, _mouseInput.x);
                _orbitAngles += rotationSpeed * Time.unscaledDeltaTime * inputValues;
                _lastManualRotationTime = Time.unscaledTime;
                return true;
            }

            return false;
        }

        private bool AutomaticRotation()
        {
            if (Time.unscaledTime - _lastManualRotationTime < alignDelay)
            {
                return false;
            }
            
            var alignedDelta = Quaternion.Inverse(_gravityAlignment) * (_focusPoint - _previousFocusPoint);
            var movement = new Vector2(alignedDelta.x, alignedDelta.z);
            var movementDeltaSquare = movement.sqrMagnitude;
            
            if (movementDeltaSquare < 0.000001f)
                return false;

            var headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSquare));
            var deltaAbs = Mathf.Abs(Mathf.DeltaAngle(_orbitAngles.y, headingAngle));
            var rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSquare);

            if (deltaAbs < alignSmoothing)
                rotationChange *= deltaAbs / alignSmoothing;
            else if (180f - deltaAbs < alignSmoothing)
                rotationChange *= (180f - deltaAbs) / alignSmoothing;
            
            _orbitAngles.y = Mathf.MoveTowardsAngle(_orbitAngles.y, headingAngle, rotationChange);

            return true;
        }

        private void ConstrainAngles()
        {
            _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, minVerticalAngle, maxVerticalAngle);

            if (_orbitAngles.y < 0)
                _orbitAngles.y += 360f;
            else if (_orbitAngles.y >= 360f)
                _orbitAngles.y -= 360f;
        }
        
        private void UpdateGravityAlignment()
        {
            var fromUp = _gravityAlignment * Vector3.up;
            var toUp = DefaultGravity.GetUpAxis(_focusPoint);

            var dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);
            var angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            var maxAngle = upAlignmentSpeed * Time.deltaTime;

            var newAlignment = Quaternion.FromToRotation(fromUp, toUp) * _gravityAlignment;

            _gravityAlignment = angle <= maxAngle ? newAlignment : Quaternion.SlerpUnclamped(_gravityAlignment, newAlignment, maxAngle / angle);
        }

        private static float GetAngle(Vector2 direction)
        {
            var angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            return direction.x < 0f ? 360f - angle : angle;
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