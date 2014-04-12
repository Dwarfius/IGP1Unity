using UnityEngine;
using System.Collections;

public class WaypointManager : MonoBehaviour 
{
    static WaypointManager instance;

    public Transform[] waypoints;
    public Line[] segments;

    public static WaypointManager Instance 
    { 
        get 
        {
            if (!instance)
            {
                instance = GameObject.FindGameObjectWithTag("WaypointRoot").GetComponent<WaypointManager>();
                instance.Init();
            }
            return instance; 
        }
    }

    public void Init()
    {
        segments = new Line[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++ )
            segments[i] = new Line(waypoints[i], (i + 1 == waypoints.Length ? waypoints[0] : waypoints[i + 1]));
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform a = waypoints[i];
            Transform b = (i == waypoints.Length - 1) ? waypoints[0] : waypoints[i + 1];
            Gizmos.color = (i == 0 ? Color.magenta : Color.red);
            Gizmos.DrawCube(a.position, Vector3.one);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(a.position, b.position);
            Gizmos.color = Color.green;
            Vector3 offsetA = a.right * a.GetComponent<Waypoint>().radius, offsetB = b.right * b.GetComponent<Waypoint>().radius; //slightly inefective, but oh well
            Gizmos.DrawLine(a.position + offsetA, b.position + offsetB);
            Gizmos.DrawLine(a.position - offsetA, b.position - offsetB);
        }
    }

    public void UpdateWaypoints()
    {
        int count = transform.GetChildCount();
        waypoints = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            waypoints[i] = transform.GetChild(i);
            if (!waypoints[i].GetComponent<Waypoint>())
                waypoints[i].gameObject.AddComponent<Waypoint>();
        }
    }

    public Line GetSegment(int segmNum)
    {
        return segments[segmNum];
    }

    public void CastToGround()
    {
        RaycastHit hit;
        foreach (Transform waypoint in waypoints)
        {
            if (Physics.Raycast(waypoint.position, Vector3.down, out hit))
                waypoint.position = hit.point + Vector3.up;
        }
    }
}