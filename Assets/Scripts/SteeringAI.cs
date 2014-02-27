using UnityEngine;
using System.Collections;

public class SteeringAI : Car 
{
    int currentWaypoint = 0;
    Line currentSegm;

    public override void Start()
    {
        base.Start();
        currentSegm = WaypointManager.Instance.GetSegment(currentWaypoint);
    }

    public override void Update()
    {
        Vector3 relativeVel = transform.InverseTransformDirection(rigidbody.velocity);
        MakeDecision();
        CheckIfFlipped();
        UpdateGear(relativeVel);
    }

    public override void OnGUI() { }

    //===========================================================================
    void MakeDecision()
    {
        Vector3 projectedPos = transform.position + rigidbody.velocity.magnitude * transform.forward;
        bool inSegm;
        Vector2 newPoint = currentSegm.MapPointOnLine(projectedPos.ToV2(), out inSegm);
        if (!inSegm)
        {
            if (++currentWaypoint == WaypointManager.Instance.waypoints.Length)
                currentWaypoint = 0;
            currentSegm = WaypointManager.Instance.GetSegment(currentWaypoint);
            newPoint = currentSegm.MapPointOnLine(projectedPos.ToV2(), out inSegm);
        }

        float rad = currentSegm.GetRadiusForMappedPoint(newPoint);
        Color color;
        if ((projectedPos.ToV2() - newPoint).sqrMagnitude > rad * rad)
        {
            float dist = (projectedPos.ToV2() - newPoint).magnitude;
            throttle = 0;// (1 / (dist - rad)) * 0.05f;
            steer = currentSegm.IsLeftOfLine(projectedPos.ToV2());
            color = Color.red;
        }
        else
        {
            throttle = 1;
            steer = 0;
            color = Color.green;
        }
        Debug.DrawLine(new Vector3(projectedPos.x, transform.position.y, projectedPos.z), new Vector3(newPoint.x, transform.position.y, newPoint.y), color);
    }
}
