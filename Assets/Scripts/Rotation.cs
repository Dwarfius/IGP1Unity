using UnityEngine;
using System.Collections;

public class Rotation : MonoBehaviour 
{
    public float rorationSpeed;

	void Update () 
    {
        Vector3 rot = transform.eulerAngles;
        rot.y += rotationSpeed * Time.deltaTime;
        carStorage.carTransform.eulerAngles = rot;
	}
}
