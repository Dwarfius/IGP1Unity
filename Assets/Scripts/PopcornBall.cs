using UnityEngine;
using System.Collections;

public class PopcornBall : MonoBehaviour 
{
    void Update()
    {
        transform.Translate(Vector3.forward * 100 * Time.deltaTime);
    }

    void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);    
    }
}
