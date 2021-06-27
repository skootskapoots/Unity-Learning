using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IPlayerActionMovement
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float movementSpeedFactor = 6f;
    [SerializeField] private float movementSpeedSmoothing = 0.02f;
    
    private PlayerControls _playerControls;
    private Vector2 _currentMove;
    private Vector2 _targetMove;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.Player.SetMovementCallbacks(this);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        characterController.Move(move * (movementSpeedFactor * Time.deltaTime));
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _targetMove = context.ReadValue<Vector2>();
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