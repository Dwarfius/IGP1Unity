using UnityEngine;
using System.Collections;

public class Rotation : MonoBehaviour 
{
    public float rorationSpeed  = 20;

	void Update () 
    {
        Vector3 rot = transform.localEulerAngles;
        rot.y += rorationSpeed * Time.deltaTime;
        transform.localEulerAngles = rot;
	}
}
