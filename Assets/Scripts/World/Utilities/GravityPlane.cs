using UnityEngine;

namespace World.Utilities
{
    public class GravityPlane : GravitySource
    {
        [Header("Gravity Settings")]
        [Tooltip("Set the gravity of the component")]
        [SerializeField] private float gravity = 9.81f;
        [Tooltip("Set the range of gravity on the plane")]
        [SerializeField, Min(0)] private float gravityRange = 1f;

        public override Vector3 GetGravity(Vector3 position)
        {
            var cachedTransform = transform;
            var up = cachedTransform.up;
            var distance = Vector3.Dot(up, position - cachedTransform.position);
            
            if (distance > gravityRange) return Vector3.zero;

            var g = -gravity;
            if (distance > 0f) g *= 1f - distance / gravityRange;
            
            return g * up;
        }

        public void OnDrawGizmos()
        {
            var cachedTransform = transform;
            var scale = cachedTransform.localScale;
            scale.y = gravityRange;
            
            Gizmos.matrix = Matrix4x4.TRS(cachedTransform.position, cachedTransform.rotation, scale);
            var size = new Vector3(1f, 0f, 1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, size);

            if (!(gravityRange > 0f)) return;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.up, size);
        }
    }
}