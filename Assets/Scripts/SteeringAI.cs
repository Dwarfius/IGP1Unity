using UnityEngine;
using System.Collections;

public class SteeringAI : MonoBehaviour 
{
    int currentWaypoint = 0;
    Line currentSegm;

    void Start()
    {
        currentSegm = WaypointManager.Instance.GetSegment(currentWaypoint);
    }

    void Update()
    {
        Vector3 projectedPos = transform.position + rigidbody.velocity.magnitude * transform.forward;
        bool inSegm;
        Vector2 newPoint = currentSegm.MapPointOnLine(projectedPos.ToV2(), out inSegm);
        if (!inSegm)
        {
            Debug.Log(Time.time + ": Changing segm to " + (currentWaypoint + 1));
            if (++currentWaypoint == WaypointManager.Instance.waypoints.Length)
            {
                Debug.Log("Loop Done");
                currentWaypoint = 0;
            }
            currentSegm = WaypointManager.Instance.GetSegment(currentWaypoint);
            newPoint = currentSegm.MapPointOnLine(projectedPos.ToV2(), out inSegm);
        }
        float rad = currentSegm.GetRadiusForMappedPoint(newPoint);
        if((projectedPos.ToV2()-newPoint).sqrMagnitude > rad*rad)
            Debug.DrawLine(new Vector3(projectedPos.x, transform.position.y, projectedPos.z), new Vector3(newPoint.x, transform.position.y, newPoint.y), Color.red);
        else
            Debug.DrawLine(new Vector3(projectedPos.x, transform.position.y, projectedPos.z), new Vector3(newPoint.x, transform.position.y, newPoint.y), Color.green);
    }
}
