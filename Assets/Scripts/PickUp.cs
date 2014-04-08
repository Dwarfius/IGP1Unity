using UnityEngine;
using System.Collections;

public class PickUp : MonoBehaviour 
{
    public float ticketChance = 0.05f;
    public float resetTime = 5f;
    public float heightChange, heightChangeSpeed, rotation;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + Vector3.up * heightChange * Mathf.Sin(Time.time * heightChangeSpeed);
        transform.Rotate(Vector3.up, rotation * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car")
        {
            Car car = other.GetComponent<Car>();
            if (car.car == (Cars)GameStorage.Instance.carIndex && Random.value < ticketChance) //if player car
                GameStorage.Instance.ticketFound = true;
            else
                car.hasPowerup = true;
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
