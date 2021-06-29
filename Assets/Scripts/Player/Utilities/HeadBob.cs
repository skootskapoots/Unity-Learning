using UnityEngine;

namespace Player.Utilities
{
    public class HeadBob : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private float focusDistanceInUnits = 15f;

        [Header("Wave Settings ")]
        [SerializeField] [Range(0, 0.1f)] private float amplitude = 0.015f;

        [SerializeField] [Range(0, 30)] private float frequency = 10f;

        [Header("Component Registry")]
        [SerializeField] private Transform playerCamera;
        
        private Vector3 _startPosition;
        private float _toggleSpeed = 3f;

        private void Awake()
        {
            _startPosition = playerCamera.localPosition;
        }

        private void Update()
        {
            if (!isEnabled) return;

            CheckMotion();
            ResetPosition();
            playerCamera.LookAt(FocusTarget());
        }

        private void PlayMotion(Vector3 motion)
        {
            playerCamera.localPosition += motion;
        }

        private void CheckMotion()
        {
            var velocity = Vector3.zero;
            var speed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            
            if (speed < _toggleSpeed) return;

            PlayMotion(CalculateMotion());
        }

        private Vector3 CalculateMotion()
        {
            var position = Vector3.zero;

            position.x += Mathf.Cos(Time.deltaTime * frequency / 2) * amplitude * 2;
            position.y += Mathf.Sin(Time.deltaTime * frequency) * amplitude;

            return position;
        }

        private void ResetPosition()
        {
            if (playerCamera.localPosition == _startPosition) return;

            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, _startPosition,
                1 * Time.deltaTime);
        }

        private Vector3 FocusTarget()
        {
            var currentPosition = transform.position;
            var targetPosition = new Vector3(currentPosition.x, transform.position.y + playerCamera.localPosition.y,
                currentPosition.z);

            targetPosition += playerCamera.forward * focusDistanceInUnits;

            return targetPosition;
        }
    }
}