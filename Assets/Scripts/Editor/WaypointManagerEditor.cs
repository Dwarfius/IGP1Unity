using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(WaypointManager))]
public class WaypointManagerEditor : Editor 
{
    Transform root;

    public override void OnInspectorGUI()
    {
        if (!root)
            root = GameObject.FindGameObjectWithTag("WaypointRoot").transform;
        if (GUILayout.Button("Add Waypoint"))
        {
            GameObject obj = new GameObject("Waypoint"+root.GetChildCount());
            obj.transform.parent = root;
        }
        if (GUILayout.Button("Update Waypoints"))
            (target as WaypointManager).UpdateWaypoints(root);
    }
}
