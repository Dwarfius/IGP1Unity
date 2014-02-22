using UnityEngine;
using System.Collections;

public class Line
{
    public Vector2 A, B;
    float k, c;
    public Line(Vector2 a, Vector2 b)
    {
        A = a;
        B = b;
        k = (B.y - A.y) / (B.x - A.x);
        c = A.y - k * A.x;
    }

    public Vector2 MapPointOnLine(Vector2 point, out bool inSegment)
    {
        float k2 = -k; //done by making a perpendicullar through the current line and the given point
        float c2 = point.y - k2 * point.x;
        float x2 = (c2 - c) / (2 * k);
        float y2 = k2 * x2 + c2;
        Vector2 mappedPoint = new Vector2(x2, y2);
        inSegment = (B - A).magnitude >= (mappedPoint - A).magnitude; 
        return mappedPoint;
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
            Gizmos.color = Color.red;
            Gizmos.DrawCube(waypoints[i].position, 2 * Vector3.one);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(waypoints[i].position, (i == waypoints.Length - 1 ? waypoints[0].position : waypoints[i+1].position));
        }
    }

    public void UpdateWaypoints(Transform root)
    {
        int count = root.GetChildCount();
        waypoints = new Transform[count];
        for(int i=0; i<count; i++)
            waypoints[i] = root.GetChild(i);
    }

    public Line GetSegment(int segmNum)
    {
        return new Line(waypoints[segmNum].ToV2(), waypoints[segmNum + 1].ToV2());
    }
}
