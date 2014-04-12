using UnityEngine;
using System.Collections;

public class ShortcutTrigger : MonoBehaviour 
{
    public int nextWaypoint;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car")
            other.GetComponent<Car>().SetWaypoint(nextWaypoint);
    }
}
