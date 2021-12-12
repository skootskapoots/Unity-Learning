using System;
using UnityEngine;
using World.Gravity;

namespace World.Utilities
{
    [RequireComponent(typeof(Rigidbody))]
    public class DefaultRigidBodyGravity : MonoBehaviour
    {
        [Header("Physics Settings")]
        [Tooltip("Determine if a body should be allowed to sleep")]
        [SerializeField] private bool floatToSleep = false;
        
        private Rigidbody body;
        private float floatDelay;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
        }

        private void FixedUpdate()
        {
            if (!floatToSleep) return;
            
            if (body.IsSleeping())
            {
                floatDelay = 0f;
                return;
            }

            if (body.velocity.sqrMagnitude < 0.0001f)
            {
                floatDelay += Time.deltaTime;
                if (floatDelay >= 1f) return;
            }
            else
            {
                floatDelay = 0f;
            }
            
            body.AddForce(DefaultGravity.GetGravity(body.position), ForceMode.Acceleration);
        }
    }
}