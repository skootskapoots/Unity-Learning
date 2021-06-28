using UnityEngine.InputSystem;

namespace Interfaces
{
    public interface IPlayerActionJump
    {
        void OnJump(InputAction.CallbackContext context);
    }
}