using UnityEngine;
using System.Collections;

public class PopcornBall : MonoBehaviour 
{
    void Start()
    {
        Destroy(gameObject, 10);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * 200 * Time.deltaTime);
    }

    void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);    
    }
}
