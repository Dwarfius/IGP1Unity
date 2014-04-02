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
            if (Random.value < ticketChance)
                GameStorage.Instance.ticketFound = true;
            else
                other.GetComponent<Car>().hasPowerup = true;
            Utilities.EnableRenders(gameObject, false);
            collider.enabled = false;
            StartCoroutine(Reactivate());
        }
    }

    IEnumerator Reactivate()
    {
        yield return new WaitForSeconds(resetTime);
        Utilities.EnableRenders(gameObject, true);
        collider.enabled = true;
    }
}
