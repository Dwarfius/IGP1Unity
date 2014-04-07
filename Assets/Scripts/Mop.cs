using UnityEngine;
using System.Collections;

public class Mop : MonoBehaviour 
{
    int sign = 1;
    public float timer;
    public float angle = 65;
    [HideInInspector] public Transform followT;

	void Start () 
    {
        Destroy(gameObject, timer);
	}

    void Update()
    {
        transform.position = followT.position + followT.forward * 2 + followT.right * 3 + followT.up;

        if (transform.eulerAngles.z < 180 - angle || transform.eulerAngles.z > 180 + angle)
            sign *= -1;

        transform.eulerAngles = followT.eulerAngles;
        transform.RotateAround(collider.bounds.center, transform.forward, 180 + Mathf.Sin(Time.time * 4) * angle);
    }
}
