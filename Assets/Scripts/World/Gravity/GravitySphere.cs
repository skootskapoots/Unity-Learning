using UnityEngine;
using World.Utilities;

namespace World.Gravity
{
    public class GravitySphere : GravitySource
    {
        [Header("Gravity Settings")]
        [Tooltip("Set the gravity of the component")]
        [SerializeField] private float gravity = 9.81f;
        
        [Header("Outer Sphere Settings")]
        [Tooltip("Set the outer radius gravity sphere")]
        [SerializeField, Min(0)] private float outerRadius = 10f;
        [Tooltip("Set the outer falloff radius gravity sphere")]
        [SerializeField, Min(0)] private float outerFalloffRadius = 15f;
        
        [Header("Inner Sphere Settings")]
        [Tooltip("Set the inner radius gravity sphere")]
        [SerializeField, Min(0)] private float innerRadius = 1f;
        [Tooltip("Set the inner falloff radius gravity sphere")]
        [SerializeField, Min(0)] private float innerFalloffRadius = 5f;

        private float outerFalloffFactor;
        private float innerFalloffFactor;

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            innerFalloffRadius = Mathf.Max(innerFalloffRadius, 0f);
            innerRadius = Mathf.Max(innerRadius, innerFalloffRadius);
            outerRadius = Mathf.Max(outerRadius, innerRadius);
            outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius);

            innerFalloffFactor = 1f / (innerRadius - innerFalloffRadius);
            outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
        }

        public override Vector3 GetGravity(Vector3 position)
        {
            var vector = transform.position - position;
            var distance = vector.magnitude;
            
            if (distance > outerFalloffRadius || distance < innerFalloffRadius)
                return Vector3.zero;

            var g = gravity / distance;
            if (distance > outerRadius)
                g *= 1f - (distance - outerRadius) * outerFalloffFactor;
            else if (distance < innerRadius)
                g *= 1f - (innerRadius - distance) * innerFalloffFactor;
            
            return g * vector;
        }

        private void OnDrawGizmos()
        {
            var p = transform.position;
            if (innerFalloffRadius > 0f && innerFalloffRadius < innerRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(p, innerFalloffRadius);
            }
            
            Gizmos.color = Color.yellow;
            if (innerRadius > 0f && innerRadius < outerRadius)
                Gizmos.DrawWireSphere(p, innerRadius);
            Gizmos.DrawWireSphere(p, outerRadius);
            
            if (!(outerFalloffRadius > outerRadius)) return;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, outerFalloffRadius);
        }
    }
}