using UnityEngine;
using System.Collections;

public class Baguette : MonoBehaviour 
{
    void Start()
    {
        transform.RotateAround(transform.up, 90);
        Destroy(gameObject, 15);
    }
}
