using UnityEngine.InputSystem;

namespace Interfaces
{
    public interface IPlayerActionCrouch
    {
        void OnCrouch(InputAction.CallbackContext context);
    }
}