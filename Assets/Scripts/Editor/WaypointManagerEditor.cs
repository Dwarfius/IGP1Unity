using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(WaypointManager))]
public class WaypointManagerEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Add Waypoint"))
        {
            Transform root = GameObject.FindGameObjectWithTag("WaypointRoot").transform;
            GameObject obj = new GameObject("Waypoint"+root.GetChildCount());
            obj.AddComponent<Waypoint>();
            obj.transform.parent = root;
            (target as WaypointManager).UpdateWaypoints();
        }
        if (GUILayout.Button("Update Waypoints"))
            (target as WaypointManager).UpdateWaypoints();
    }
}
