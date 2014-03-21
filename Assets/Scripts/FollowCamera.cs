using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour 
{
    [HideInInspector] public Transform target;
    [HideInInspector] public float timeToFollow;
	
	void Update () 
    {
        if (timeToFollow > 0)
        {
            transform.LookAt(target);
            timeToFollow -= Time.deltaTime;
            if (timeToFollow <= 0)
            {
                camera.enabled = false;
                target.GetComponentInChildren<Camera>().enabled = true;
                target = null;
            }
        }
	}
}
