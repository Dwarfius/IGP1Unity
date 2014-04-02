using UnityEngine;
using System.Collections;

public class ColaSpill : MonoBehaviour 
{
    void Start()
    {
        Destroy(gameObject, 30);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car")
            other.GetComponent<Car>().BuffTopSpeed(0.5f, 10);
    }
}
