using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour 
{
    public float radius;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + transform.right * radius - transform.forward, transform.position + transform.right * radius + transform.forward);
        Gizmos.DrawLine(transform.position - transform.right * radius - transform.forward, transform.position - transform.right * radius + transform.forward);
    }
}
