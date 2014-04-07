using UnityEngine;
using System.Collections;

public class CarFollowCamera : MonoBehaviour
{
    public static CarFollowCamera instance;

    public Transform target = null;
    public float height = 1f;
    public float distance = 4f;
    public float heightDamping = 3f;
    public float rotationDamping = 2f;

    void Start()
    {
        instance = this;
    }

    void LateUpdate()
    {
        if (!target)
            return;

        // Calculate the current rotation angles
        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;

        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // Damp the rotation around the y-axis
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

        // Damp the height
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        // Convert the angle into a rotation
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        transform.position = target.position - currentRotation * Vector3.forward * distance;

        // Set the height of the camera
        Vector3 pos = transform.position;
        pos.y = currentHeight;
        transform.position = pos;

        // Always look at the target
        transform.LookAt(target);
        Vector3 euler = transform.eulerAngles;
        euler.x = 0;
        transform.eulerAngles = euler;
    }
}
