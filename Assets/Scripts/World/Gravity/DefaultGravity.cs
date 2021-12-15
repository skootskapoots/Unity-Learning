using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Utilities;

namespace World.Gravity
{
    public class DefaultGravity : MonoBehaviour
    {
        private static readonly List<GravitySource> GravitySources = new List<GravitySource>();
        
        public static Vector3 GetGravity(Vector3 position)
        {
            return AggregateSources(position);
        }

        public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
        {
            var g = AggregateSources(position);
            upAxis = -g.normalized;
            
            return g;
        }

        public static Vector3 GetUpAxis(Vector3 position)
        {
            var g = AggregateSources(position);
            return -g.normalized;
        }

        public static void Register(GravitySource source)
        {
            Debug.Assert(
                !GravitySources.Contains(source),
                "Duplicate registration of gravity source!", source);
            GravitySources.Add(source);
        }

        public static void Unregister(GravitySource source)
        {
            Debug.Assert(
                GravitySources.Contains(source),
                "Unregistration of unknown gravity source!", source);
            GravitySources.Remove(source);
        }

        private static Vector3 AggregateSources(Vector3 position)
        {
            return GravitySources.Aggregate(Vector3.zero, (current, src) => current + src.GetGravity(position));
        }
    }
}
