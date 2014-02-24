using UnityEngine;
using System.Collections;

public class Line
{
    public Vector2 A, B;
    float r1, r2;
    float k, c;
    public Line(Transform a, Transform b)
    {
        A = a.ToV2();
        B = b.ToV2();
        r1 = a.GetComponent<Waypoint>().radius;
        r2 = b.GetComponent<Waypoint>().radius;
        k = (B.y - A.y) / (B.x - A.x);
        c = A.y - k * A.x;
    }

    public Vector2 MapPointOnLine(Vector2 point, out bool inSegment)
    {
        float k2 = -1/k;
        float c2 = point.y - k2 * point.x;
        float x2 = (c2 - c) / (k - k2);
        float y2 = k2 * x2 + c2;
        Vector2 mappedPoint = new Vector2(x2, y2);
        inSegment = (B - A).magnitude >= (mappedPoint - A).magnitude;
        return mappedPoint;
    }

    public float GetRadiusForMappedPoint(Vector2 point)
    {
        float coeff = (point.x - A.x) / (B.x - A.x); //can be negative (due to point can be before A), but then it'll be clamped to 0
        return Mathf.Lerp(r1, r2, coeff);
    }

    public float IsLeft(Vector2 point)
    {
        return Mathf.Sign((B.x - A.x) * (point.y - A.y) - (B.y - A.y) * (point.x - A.x));
    }
}

public class WaypointManager : MonoBehaviour 
{
    static WaypointManager instance;

    public Transform[] waypoints;

    public static WaypointManager Instance { get { return instance; } }

    void Awake()
    {
        instance = this;
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.color = (i == waypoints.Length - 1 ? Color.magenta : Color.red);
            Gizmos.DrawCube(waypoints[i].position, 2 * Vector3.one);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(waypoints[i].position, (i == waypoints.Length - 1 ? waypoints[0].position : waypoints[i+1].position));
        }
    }

    public void UpdateWaypoints(Transform root)
    {
        int count = root.GetChildCount();
        waypoints = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            waypoints[i] = root.GetChild(i);
            if (!waypoints[i].GetComponent<Waypoint>())
                waypoints[i].gameObject.AddComponent<Waypoint>();
        }
    }

    public Line GetSegment(int segmNum)
    {
        if (segmNum == waypoints.Length - 1)
            return new Line(waypoints[segmNum], waypoints[0]);
        else
            return new Line(waypoints[segmNum], waypoints[segmNum + 1]);
    }
}