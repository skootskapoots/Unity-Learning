using System;
using UnityEngine;

public class DrawRay : MonoBehaviour
{
    public int range = 50;
    public int accuracy = 23;

    private void OnDrawGizmos()
    {
        var playerPosition = transform.position;
        Debug.DrawRay(playerPosition, transform.TransformDirection(Vector3.forward) * (100 - range), Color.green);
    }
}
