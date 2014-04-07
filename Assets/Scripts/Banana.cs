using UnityEngine;
using System.Collections;

public class Banana : MonoBehaviour 
{
    [HideInInspector] public Vector3 heading;

    void Start()
    {
        Destroy(gameObject, 10);
    }

	void Update () 
    {
        transform.position += heading * 100 * Time.deltaTime;
        transform.RotateAround(transform.right, 30 * Time.deltaTime);
	}

    void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
    }
}
