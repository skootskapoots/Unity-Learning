using UnityEngine;
using World.Gravity;

namespace World.Utilities
{
    public class GravitySource : MonoBehaviour
    {
        private void OnEnable()
        {
            DefaultGravity.Register(this);
        }

        private void OnDisable()
        {
            DefaultGravity.Unregister(this);
        }

        public virtual Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }
    }
}