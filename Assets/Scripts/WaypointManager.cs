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
            Gizmos.color = (i == 0 ? Color.magenta : Color.red);
            Gizmos.DrawCube(waypoints[i].position, Vector3.one);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(waypoints[i].position, (i == waypoints.Length - 1 ? waypoints[0].position : waypoints[i+1].position));
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