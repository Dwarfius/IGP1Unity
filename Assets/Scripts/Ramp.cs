using UnityEngine;
using System.Collections;

public class Ramp : MonoBehaviour 
{
    public float targetVel;
    public float radius;
    public GameObject followCamera;
    public float timeToFollow;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car")
        {
            other.rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
            float vel = other.rigidbody.velocity.magnitude;
            if (vel < targetVel)
                other.rigidbody.AddForce(other.transform.forward * (targetVel - vel), ForceMode.VelocityChange);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Car")
        {
            other.rigidbody.constraints = RigidbodyConstraints.None;
            Car car = other.GetComponent<Car>();
            GameStorage.Instance.MarkJumpPassed(car.car);
            if (car.car == (Cars)GameStorage.Instance.carIndex && followCamera) //if it's the player car
            {
                FollowCamera script = followCamera.GetComponent<FollowCamera>();
                script.target = other.transform;
                script.timeToFollow = timeToFollow;
                followCamera.camera.enabled = true;
                CarFollowCamera.instance.camera.enabled = false;
            }
        }
    }
}
