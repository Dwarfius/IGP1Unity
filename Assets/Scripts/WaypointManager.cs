using UnityEngine;
using System.Collections;

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
        return new Line(waypoints[segmNum], (segmNum + 1 == waypoints.Length ? waypoints[0] : waypoints[segmNum + 1]));
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