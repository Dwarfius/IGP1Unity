using UnityEngine;
using System.Collections;

public class PickUp : MonoBehaviour 
{
    public float ticketChance = 0.05f;
    public float resetTime = 5f;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car")
        {
            if (Random.Range(0, 1) < ticketChance)
                GameStorage.Instance.ticketFound = true;
            else
                other.GetComponent<Car>().hasPowerup = true;
            renderer.enabled = false;
            collider.enabled = false;
        }
    }

    IEnumerator Reactivate()
    {
        yield return new WaitForSeconds(resetTime);
        renderer.enabled = true;
        collider.enabled = true;
    }
}
