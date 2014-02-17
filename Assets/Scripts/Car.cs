using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour 
{
    WheelFrictionCurve curve;

    public class Wheel
    {
        public WheelCollider col;
        public Transform wheelGraphic; 
        public bool driveWheel, steerWheel;
        public Vector3 wheelVel, groundSpeed;
    }

    public Transform[] frontWheels, backWheels;
    public int gears = 5;
    public float topSpeed = 160, suspensionRange = 0.1f, suspensionDamper = 50;
    public float maximumTurn = 15, minimumTurn = 10, resetTime = 3;
    public float suspensionSpringFront = 18500, suspensionSpringRear = 9000;
    public float handbrakeXDragFactor = 0.5f;
    public Transform centerOfMass;
    public Vector3 dragMultiplier = new Vector3(2, 5, 1);

    Wheel[] wheels;
    float[] engineForceValues, gearSpeeds;
    float handbrakeTimer, currentEnginePower, throttle;
    float handbrakeTime, steer, initialDragMultiplierX, resetTimer;
    bool handbrake, canDrive, canSteer;
    int currentGear;

	void Start () 
    {
        wheels = new Wheel[frontWheels.Length + backWheels.Length];

        SetUpWheels();
        SetUpCenterOfMass();
        SetUpGears();

        initialDragMultiplierX = dragMultiplier.x;
	}
	
	void Update () 
    {
        Vector3 relativeVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        GetInput();
        CheckIfFlipped();
        UpdateGear(relativeVelocity);
	}

    void FixedUpdate()
    {
        // The rigidbody velocity is always given in world space, but in order to work in local space of the car model we need to transform it first.
	    Vector3 relativeVel = transform.InverseTransformDirection(rigidbody.velocity);

        CalculateState();
        UpdateFriction(relativeVel);
        UpdateDrag(relativeVel);
        CalculateEnginePower(relativeVel);
        if (canDrive)
            ApplyThrottle(relativeVel);
        if (canSteer)
            ApplySteering(relativeVel);
        RotateWheels(relativeVel);
    }

    //start functions
    void SetUpWheels()
    {
        SetupWheelFrictionCurve();

        int count = 0;
        foreach(Transform wheel in frontWheels)
            wheels[count++] = SetUpWheel(wheel, true);
        foreach(Transform wheel in backWheels)
            wheels[count++] = SetUpWheel(wheel, false);
    }

    void SetupWheelFrictionCurve()
    {
        curve = new WheelFrictionCurve();
        curve.extremumSlip = 1;
        curve.extremumValue = 50;
        curve.asymptoteSlip = 2;
        curve.asymptoteValue = 25;
        curve.stiffness = 1;
    }

    Wheel SetUpWheel(Transform wheelTransform, bool isFront)
    {
        GameObject go = new GameObject(wheelTransform + " Collider");
        go.transform.position = wheelTransform.position;
        go.transform.parent = transform;
        go.transform.rotation = wheelTransform.rotation;

        WheelCollider wc = go.AddComponent<WheelCollider>();
        wc.suspensionDistance = suspensionRange;
        JointSpring js = wc.suspensionSpring;
        js.spring = (isFront ? suspensionSpringFront : suspensionSpringRear);
        js.damper = suspensionDamper;
        wc.suspensionSpring = js;

        Wheel wheel = new Wheel();
        wheel.col = wc;
        wc.sidewaysFriction = curve;
        wheel.wheelGraphic = wheelTransform;
        wheel.col.radius = wheelTransform.renderer.bounds.size.y / 2;

        if (isFront)
        {
            wheel.steerWheel = true;

            go = new GameObject(wheelTransform.name + " Steer Column");
            go.transform.position = wheelTransform.position;
            go.transform.rotation = wheelTransform.rotation;
            go.transform.parent = transform;
            wheelTransform.parent = go.transform;
        }
        else
            wheel.driveWheel = true;

        return wheel;
    }

    void SetUpCenterOfMass()
    {
        if (centerOfMass)
            rigidbody.centerOfMass = centerOfMass.localPosition;
    }

    void SetUpGears()
    {
        engineForceValues = new float[gears];
        gearSpeeds = new float[gears];

        float tempTopSpeed = topSpeed;
        for (int i = 0; i < gears; i++)
        {
            if (i == 0)
                gearSpeeds[i] = tempTopSpeed / 4;
            else
                gearSpeeds[i] = tempTopSpeed / 4 + gearSpeeds[i - 1];
            tempTopSpeed -= tempTopSpeed / 4;
        }

        float engineFactor = topSpeed / gearSpeeds[gears-1];
        for (int i = 0; i < gears; i++)
        {
            float maxLinearDrag = gearSpeeds[i] * gearSpeeds[i];
            engineForceValues[i] = maxLinearDrag * engineFactor;
        }
    }

    //Update functions
    void GetInput()
    {
        throttle = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");

        CheckHandbrake();
    }

    void CheckHandbrake()
    {
        if (Input.GetButton("Handbrake"))
        {
            if (!handbrake)
            {
                handbrake = true;
                handbrakeTime = Time.time;
                dragMultiplier.x = initialDragMultiplierX * handbrakeXDragFactor;
            }
        }
        else if (handbrake)
        {
            handbrake = false;
            StartCoroutine(StopHandbraking(Mathf.Min(5, Time.time - handbrakeTime)));
        }
    }

    IEnumerator StopHandbraking(float seconds)
    {
        float diff = initialDragMultiplierX - dragMultiplier.x;
        handbrakeTimer = 1;

        while (dragMultiplier.x < initialDragMultiplierX && !handbrake)
        {
            dragMultiplier.x += diff * (Time.deltaTime / seconds);
            handbrakeTimer -= Time.fixedDeltaTime / seconds;
            yield return new WaitForFixedUpdate();
        }

        dragMultiplier.x = initialDragMultiplierX;
        handbrakeTimer = 0;
    }

    void CheckIfFlipped()
    {
        if (transform.localEulerAngles.z > 80 && transform.localEulerAngles.z < 280)
            resetTimer += Time.deltaTime;
        else
            resetTimer = 0;

        if (resetTimer > resetTime)
            FlipCar();
    }

    void FlipCar()
    {
	    transform.rotation = Quaternion.LookRotation(transform.forward);
	    transform.position += Vector3.up * 0.5f;
	    rigidbody.velocity = Vector3.zero;
	    rigidbody.angularVelocity = Vector3.zero;
	    resetTimer = 0;
	    currentEnginePower = 0;
    }

    void UpdateGear(Vector3 relativeVel)
    {
        currentGear = 0;
        for (int i = 0; i < gears - 1; i++)
            if (relativeVel.z > gearSpeeds[i]) 
                currentGear = i;
    }

    //Fixed Update functions
    void CalculateState()
    {
        canDrive = canSteer = false;
        foreach (Wheel wheel in wheels)
        {
            if (wheel.col.isGrounded)
            {
                if (wheel.steerWheel)
                    canSteer = true;
                if (wheel.driveWheel)
                    canDrive = true;
            }
        }
    }

    void UpdateDrag(Vector3 relativeVel)
    {
        Vector3 relativeDrag = -relativeVel;
        //Vector3 drag = Vector3.Scale(dragMultiplier, relativeDrag);
        Vector3 drag = new Vector3(relativeDrag.x * relativeDrag.x * Sign(relativeDrag.x), //using the sqr of the relativeDrag, while retaining the sign
                                   relativeDrag.y * relativeDrag.y * Sign(relativeDrag.y),
                                   relativeDrag.z * relativeDrag.z * Sign(relativeDrag.z));
        if (initialDragMultiplierX > dragMultiplier.x) // Handbrake
        {
            drag.x /= (relativeVel.magnitude / (topSpeed / (1 + 2 * handbrakeXDragFactor)));
            drag.z *= (1 + Mathf.Abs(Vector3.Dot(rigidbody.velocity.normalized, transform.forward)));
            drag += rigidbody.velocity * Mathf.Clamp01(rigidbody.velocity.magnitude / topSpeed);
        }
        else // No handbrake
            drag.x *= topSpeed / relativeVel.magnitude;

        if (Mathf.Abs(relativeVel.x) < 5 && !handbrake)
            drag.x = -relativeVel.x * dragMultiplier.x;

        rigidbody.AddForce(transform.TransformDirection(drag) * rigidbody.mass * Time.deltaTime);
    }

    void UpdateFriction(Vector3 relativeVel)
    {
        float sqrVel = relativeVel.x * relativeVel.x;
        curve.extremumValue = Mathf.Clamp(300 - sqrVel, 0, 300);
        curve.asymptoteValue = Mathf.Clamp(150 - (sqrVel / 2), 0, 150);

        foreach (Wheel wheel in wheels)
        {
            wheel.col.sidewaysFriction = curve;
            wheel.col.forwardFriction = curve;
        }
    }

    void CalculateEnginePower(Vector3 relativeVel)
    {
        if (throttle == 0)
            currentEnginePower -= 200 * Time.deltaTime;
        else if (Sign(relativeVel.z) == Sign(throttle))
        {
            float normPower = (currentEnginePower / engineForceValues[gears - 1]) * 2;
            currentEnginePower += 200 * EvaluateNormPower(normPower) * Time.deltaTime;
        }
        else
            currentEnginePower -= 300 * Time.deltaTime;

        if (currentGear == 0)
            currentEnginePower = Mathf.Clamp(currentEnginePower, 0, engineForceValues[0]);
        else
            currentEnginePower = Mathf.Clamp(currentEnginePower, engineForceValues[currentGear - 1], engineForceValues[currentGear]);
    }

    float EvaluateNormPower(float normPower)
    {
        if (normPower < 1)
            return 10 - normPower * 9;
        else
            return 1.9f - normPower * 0.9f;
    }

    void ApplyThrottle(Vector3 relativeVel)
    {
        float throttleForce = 0, brakeForce = 0;
        if (Sign(relativeVel.z) == Sign(throttle))
        {
            if (!handbrake)
                throttleForce = Sign(throttle) * currentEnginePower * rigidbody.mass;
        }
        else
            brakeForce = Sign(throttle) * engineForceValues[0] * rigidbody.mass;

        rigidbody.AddForce((throttleForce + brakeForce) * transform.forward * Time.deltaTime);
    }

    void ApplySteering(Vector3 relativeVel)
    {
        float turnRadius = 3 / Mathf.Sin(((90 - steer * 30) * Mathf.Deg2Rad));
        float minMaxTurn = EvaluateSpeedToTurn(rigidbody.velocity.magnitude);
        float turnSpeed = Mathf.Clamp(relativeVel.z / turnRadius, -minMaxTurn / 10, minMaxTurn / 10);

        transform.RotateAround(transform.position + transform.right * turnRadius * steer, transform.up, turnSpeed * steer * Mathf.Deg2Rad * Time.deltaTime * 1000);
        //depending on the steeering, rotate the tire column
        foreach(Wheel w in wheels)
        {
            if (w.steerWheel)
            {
                Vector3 angle = w.wheelGraphic.localEulerAngles;
                angle.y = steer * 15;
                w.wheelGraphic.localEulerAngles = angle;
            }
        }

        if(initialDragMultiplierX > dragMultiplier.x) //handbrake
        {
            float rotationDir = Sign(steer);
            if (steer == 0)
            {
                if (rigidbody.angularVelocity.y < 1) //if we are handbraking without steering, apply small random rotation
                    rotationDir = Random.Range(-1, 1);
                else
                    rotationDir = rigidbody.angularVelocity.y; //else apply the current rotation
            }
            //rotate car around middle point of front wheels
            transform.RotateAround(transform.TransformPoint((frontWheels[0].localPosition + frontWheels[1].localPosition)/2),
                                   transform.up,
                                   rigidbody.velocity.magnitude * Mathf.Clamp01(1 - rigidbody.velocity.magnitude / topSpeed) * rotationDir * Time.deltaTime);
        }
    }

    float EvaluateSpeedToTurn(float speed)
    {
        if (speed > topSpeed / 2)
            return minimumTurn;

        float speedIndex = 1 - (speed * 2/ topSpeed);
        return minimumTurn + speedIndex * (maximumTurn - minimumTurn);
    }

    void RotateWheels(Vector3 relativeVel)
    {
        float speed = relativeVel.magnitude;
        foreach (Wheel w in wheels)
        {
            float L = w.col.radius * 2 * Mathf.PI;
            float percent = speed / L;
            w.wheelGraphic.Rotate(w.wheelGraphic.right, percent * 360 * Time.deltaTime, Space.World);
        }
    }

    int Sign(float f)
    {
        if (f > 0)
            return 1;
        else if (f < 0)
            return -1;
        else
            return 0;
    }
}
