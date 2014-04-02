using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour 
{
    WheelFrictionCurve forwardCurve, sidewaysCurve;
    public static Vector2 minimapScale = new Vector2(0.4f, 0.4f);

    public class Wheel
    {
        public WheelCollider col;
        public Transform wheelGraphic; 
        public bool driveWheel, steerWheel;
        public Vector3 wheelVel, groundSpeed;
    }

    public Cars car;
    public GameObject powerupPrefab;
    public Texture2D pickup;
    public Transform[] frontWheels, backWheels;
    public float slipValue = 300, stiffnesCoeff = 0.6f;
    public int gears = 5;
    public float topSpeed = 160;
    public float maximumTurn = 10, minimumTurn = 3, resetTime = 3;
    public Transform centerOfMass;
    public Vector2 charScale, gaugeScale, arrowScale;
    public float gaugeAngleOffset;
        
    [HideInInspector] public int currentWaypoint;
    [HideInInspector] public bool finished, hasPowerup;

    float handbrakeXDragFactor = 0.5f;
    float suspensionSpringFront = 18500, suspensionSpringRear = 9000, suspensionRange = 0.1f, suspensionDamper = 50;
    Vector3 dragMultiplier = new Vector3(2, 5, 1);
    float[] engineForceValues, gearSpeeds;
    bool handbrake, canDrive, canSteer;
    bool inMenu;
    int currentGear;
    Texture2D blackText, gauge, arrow;
    Line currentSegm = null;
    
    protected Vector2 minimapStartOffset, trackSize;
    protected float currentEnginePower, throttle;
    protected float handbrakeTime, steer, initialDragMultiplierX, resetTimer;
    protected Wheel[] wheels;
    protected Texture2D minimapChar, minimap;

	public virtual void Start () 
    {
        minimapStartOffset = new Vector2(GameStorage.minimapX1, GameStorage.minimapY1);
        trackSize = new Vector2(GameStorage.minimapX2 - GameStorage.minimapX1, GameStorage.minimapY2 - GameStorage.minimapY1);

        minimap = (Texture2D)Resources.Load("Textures/minimap");
        gauge = (Texture2D)Resources.Load("Textures/speedometer");
        arrow = (Texture2D)Resources.Load("Textures/arrow");

        minimapChar = Utilities.GetMinimapTexture(car);
        blackText = new Texture2D(1, 1);
        blackText.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
        blackText.Apply();

        wheels = new Wheel[frontWheels.Length + backWheels.Length];

        SetUpWheels();
        SetUpCenterOfMass();
        SetUpGears();

        initialDragMultiplierX = dragMultiplier.x;

        //get current waypoint
        if (WaypointManager.Instance != null)
        {
            for (int i = 0; i < WaypointManager.Instance.waypoints.Length - 1; i++)
            {
                bool inSegm = false;
                currentSegm = WaypointManager.Instance.GetSegment(i);
                currentSegm.MapPointOnLine(transform.ToV2(), out inSegm);
                if (inSegm)
                {
                    currentWaypoint = i;
                    return;
                }
            }
        }
	}
	
	public virtual void Update () 
    {
        Vector3 relativeVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        if(currentSegm != null)
            CheckWaypointSegm();
        GetInput();
        CheckIfFlipped();
        UpdateGear(relativeVelocity);
	}

    public virtual void FixedUpdate()
    {
        // The rigidbody velocity is always given in world space, but in order to work in local space of the car model we need to transform it first.
	    Vector3 relativeVel = transform.InverseTransformDirection(rigidbody.velocity);

        CalculateState();
        UpdateFriction(relativeVel);
        if(canDrive)
            UpdateDrag(relativeVel);
        CalculateEnginePower(relativeVel);
        if (canDrive)
            ApplyThrottle(relativeVel);
        if (canSteer)
            ApplySteering(relativeVel);
    }

    public virtual void OnGUI()
    {
        GUI.skin = GameStorage.Instance.skin;
        if (!finished)
        {
            DrawMinimap();
            DrawSpeedometer();
            DrawPickups();
            if (GameStorage.Instance != null && GameStorage.Instance.cars != null)
                DrawLeaderboard();
        }
        else
            DrawEndScreen();

        if (inMenu)
            DrawMenu();
    }

    //===========================================================================
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
        forwardCurve = new WheelFrictionCurve();
        forwardCurve.extremumSlip = 1;
        forwardCurve.asymptoteSlip = 2;
        forwardCurve.stiffness = 1;

        sidewaysCurve = new WheelFrictionCurve();
        sidewaysCurve.extremumSlip = 1;
        sidewaysCurve.asymptoteSlip = 2;
        sidewaysCurve.stiffness = 1;
    }

    Wheel SetUpWheel(Transform wheelTransform, bool isFront)
    {
        GameObject go = new GameObject(wheelTransform + " Collider");
        go.transform.position = wheelTransform.position;
        go.transform.parent = transform;
        go.transform.localEulerAngles = new Vector3(0, 0, 0); 

        WheelCollider wc = go.AddComponent<WheelCollider>();
        wc.suspensionDistance = suspensionRange;
        JointSpring js = wc.suspensionSpring;
        js.spring = (isFront ? suspensionSpringFront : suspensionSpringRear);
        js.damper = suspensionDamper;
        wc.suspensionSpring = js;

        Wheel wheel = new Wheel();
        wheel.col = wc;
        wc.forwardFriction = forwardCurve;
        wc.sidewaysFriction = sidewaysCurve;
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

    public void SetUpGears()
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

    //===========================================================================
    //Update functions
    void GetInput()
    {
        if(CInput.GetKeyDown("Pause"))
        {
            inMenu = !inMenu;
            Time.timeScale = (inMenu ? 0 : 1);
        }

        if (hasPowerup && CInput.GetKeyDown("Use Item"))
            UsePowerUp();

        throttle = CInput.GetAxis("Vertical");
        steer = CInput.GetAxis("Horizontal");

        CheckHandbrake();
    }

    protected void UsePowerUp()
    {
        if (car == Cars.Serpent)
        {
            BuffTopSpeed(1.5f, 10);
        }
        else if (car == Cars.Cola)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position - transform.forward * 10 + transform.up * 2, -transform.up);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
            {
                GameObject spill = (GameObject)Instantiate(powerupPrefab, hit.point + hit.normal/8, Quaternion.identity);
                spill.transform.LookAt(hit.point - hit.normal);
            }
        }
        else if (car == Cars.Janitor)
        {
            ((GameObject)Instantiate(powerupPrefab)).GetComponentInChildren<Mop>().followT = transform;
        }
        else if (car == Cars.Gorilla)
        {
            StartCoroutine(SpawBananaOverTime(0.5f, 6, powerupPrefab));
        }
        else if (car == Cars.French)
        {
            StartCoroutine(SpawBaguetteOverTime(1, 5, powerupPrefab));
        }
        else //car == Cars.Popcorn
        {
            Instantiate(powerupPrefab, transform.position + transform.forward, transform.rotation);
            Instantiate(powerupPrefab, transform.position + transform.forward - transform.right * 4, transform.rotation);
            Instantiate(powerupPrefab, transform.position + transform.forward + transform.right * 4, transform.rotation);
        }
        hasPowerup = false;
    }

    protected void CheckWaypointSegm()
    {
        bool inSegm = false;
        currentSegm.MapPointOnLine(transform.ToV2(), out inSegm);
        if (!inSegm)
        {
            if (++currentWaypoint == WaypointManager.Instance.waypoints.Length)
            {
                currentWaypoint = 0;
                GameStorage.Instance.AddLap(car);
            }
            currentSegm = WaypointManager.Instance.GetSegment(currentWaypoint);
        }
    }

    void CheckHandbrake()
    {
        if (CInput.GetKey("Brake"))
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
        float handbrakeTimer = 1;

        while (dragMultiplier.x < initialDragMultiplierX && !handbrake)
        {
            dragMultiplier.x += diff * handbrakeTimer; //(Time.deltaTime / seconds)
            handbrakeTimer -= Time.fixedDeltaTime / seconds;
            yield return new WaitForFixedUpdate();
        }

        dragMultiplier.x = initialDragMultiplierX;
    }

    protected void CheckIfFlipped()
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

    protected void UpdateGear(Vector3 relativeVel)
    {
        currentGear = 0;
        for (int i = 0; i < gears - 1; i++)
            if (relativeVel.z > gearSpeeds[i])
                currentGear = i+1;
    }

    //===========================================================================
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

    void UpdateFriction(Vector3 relativeVel) //change the sideways friction
    {
        /*float sqrVel = relativeVel.x * relativeVel.x;
        sidewaysCurve.extremumValue = forwardCurve.extremumValue = Mathf.Clamp(slipValue - sqrVel, 0, slipValue);
        sidewaysCurve.asymptoteValue = forwardCurve.asymptoteValue = Mathf.Clamp(slipValue / 2 - (sqrVel / 2), 0, slipValue / 2);*/

        float coeff = Mathf.Abs(relativeVel.normalized.x);
        sidewaysCurve.extremumValue = forwardCurve.extremumValue = slipValue - slipValue * 0.9f * coeff;
        sidewaysCurve.asymptoteValue = forwardCurve.asymptoteValue = slipValue/2 - slipValue * 0.45f * coeff;

        sidewaysCurve.stiffness = 1 - stiffnesCoeff * relativeVel.magnitude / topSpeed; //it's kind of cheating, but f it :D

        foreach (Wheel wheel in wheels)
        {
            wheel.col.sidewaysFriction = sidewaysCurve;
            wheel.col.forwardFriction = forwardCurve;
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
        return (normPower < 1 ? 10 - normPower * 9 : 1.9f - normPower * 0.9f);
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
        //depending on the steeering, rotate the tire column
        float turn = EvaluateSpeedToTurn(rigidbody.velocity.magnitude);
        foreach(Wheel w in wheels)
        {
            if (w.steerWheel)
                w.col.steerAngle = turn * steer;
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
        float speedIndex = 1 - (speed / topSpeed);
        return minimumTurn + speedIndex * (maximumTurn - minimumTurn);
    }

    //===========================================================================
    //OnGUI methods
    void DrawMenu()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackText);
        float x = Screen.width / 2, y = Screen.height / 2;
        float width = 125, height = 30, empty = 15;
        if (GUI.Button(new Rect(x - width / 2, y, width, height), "Main Menu"))
        {
            Time.timeScale = 1;
            Application.LoadLevel(0);
        }

        y += height + empty;
        if (GUI.Button(new Rect(x - width / 2, y, width, height), "Back"))
        {
            Time.timeScale = 1;
            inMenu = false;
        }
    }

    void DrawSpeedometer()
    {
        Matrix4x4 backupMatrix = GUI.matrix;
        Vector2 gaugeSize = Vector2.Scale(new Vector2(gauge.width, gauge.height), gaugeScale);
        GUI.DrawTexture(new Rect(Screen.width - gaugeSize.x, Screen.height - gaugeSize.y, gaugeSize.x, gaugeSize.y), gauge);
        Vector2 arrowSize = Vector2.Scale(new Vector2(arrow.width, arrow.height), arrowScale);
        Vector2 arrowPos = new Vector2(Screen.width - gaugeSize.x / 2, Screen.height - gaugeSize.y / 2);
        GUIUtility.RotateAroundPivot(-gaugeAngleOffset + rigidbody.velocity.magnitude / topSpeed * (180 + gaugeAngleOffset), arrowPos);
        GUI.DrawTexture(new Rect(arrowPos.x - arrowSize.x/2, arrowPos.y - arrowSize.y/2, arrowSize.x, arrowSize.y), arrow);
        GUI.matrix = backupMatrix;
    }

    void DrawPickups()
    {
        float scaleCoeff = 0.05f;
        Vector2 size = new Vector2(Screen.width * scaleCoeff, Screen.width * scaleCoeff);
        if (hasPowerup && pickup)
            GUI.DrawTexture(new Rect(Screen.width / 2 - size.x, 0, size.x, size.y), pickup);
        if (GameStorage.Instance.ticketFound)
            GUI.DrawTexture(new Rect(Screen.width / 2, 0, size.x, size.y), GameStorage.Instance.ticket);
    }

    void DrawLeaderboard()
    {
        string b = "";
        for (int i = 0; i < 6; i++)
        {
            if (GameStorage.Instance.cars[i].carName == car)
                b += (i + 1) + ". " + GameStorage.Instance.cars[i].carName + " - Player\n";
            else
                b += (i + 1) + ". " + GameStorage.Instance.cars[i].carName + "\n";
        }
        Vector2 size = GUI.skin.box.CalcSize(new GUIContent(b));
        GUI.Box(new Rect(Screen.width - size.x, 0, size.x, size.y), b);
    }

    void DrawMinimap()
    {
        Vector2 minimapSize = Vector2.Scale(new Vector2(minimap.width, minimap.height), minimapScale);
        GUI.DrawTexture(new Rect(0, Screen.height - minimapSize.y, minimapSize.x, minimapSize.y), minimap);

        if (minimapChar)
        {
            Vector2 relativePos = transform.ToV2() - minimapStartOffset;
            relativePos = new Vector2(relativePos.x / trackSize.x, relativePos.y / trackSize.y); //[0..1]
            Vector2 minimapPos = Vector2.Scale(relativePos, minimapSize); //[0..minimapSize]
            Vector2 charSize = Vector2.Scale(new Vector2(minimapChar.width, minimapChar.height), charScale);
            GUI.DrawTexture(new Rect(minimapPos.x - charSize.x / 2, Screen.height - minimapPos.y - charSize.y / 2, charSize.x, charSize.y), minimapChar);
        }
    }

    void DrawEndScreen()
    {
        float emptySpaceScale = 0.2f;
        Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 boxSize = new Vector2(Screen.width - Screen.width * 2 * emptySpaceScale, Screen.height - Screen.height * 2 * emptySpaceScale);
        Rect boxRect = new Rect(center.x - boxSize.x / 2, center.y - boxSize.y / 2, boxSize.x, boxSize.y);

        string b = "Result:\n";
        for (int i = 0; i < 6; i++)
        {
            GameStorage.CarStorage carStorage = GameStorage.Instance.cars[i];
            if (!carStorage.carScript.finished) //show only finished players
                continue;

            if (carStorage.carName == car) //if player
                b += (i + 1) + ". " + carStorage.carName + " (Player) - " + carStorage.time + "\n";
            else
                b += (i + 1) + ". " + carStorage.carName + " - " + carStorage.time + "\n";
        }
        GUI.Box(boxRect, b);
        
        float width = 125, height = 30, empty = 15;
        if (GUI.Button(new Rect(boxRect.xMax - (width + empty), boxRect.yMax - height - empty, width, height), "Continue"))
            GameStorage.Instance.FinishGame(GameStorage.Instance.IsFirst(car));

        if (GUI.Button(new Rect(boxRect.xMax - (width + empty) * 2, boxRect.yMax - height - empty, width, height), "Retry"))
            GameStorage.Instance.Retry();
    }

    //===========================================================================
    //Utility methods
    int Sign(float f)
    {
        return (f < 0 ? -1 : (f > 0 ? 1 : 0));
    }

    IEnumerator PerformAction(float t, System.Action act)
    {
        yield return new WaitForSeconds(t);
        act();
    }

    public void BuffTopSpeed(float rate, float time)
    {
        topSpeed *= rate;
        SetUpGears();
        PerformAction(time, delegate { topSpeed /= rate; SetUpGears(); });
    }

    IEnumerator SpawBananaOverTime(float rate, int amount, GameObject item)
    {
        for (int i = 0; i < amount; i++)
        {
            ((GameObject)Instantiate(item, transform.position + transform.forward, transform.rotation)).GetComponent<Banana>().heading = transform.forward;
            yield return new WaitForSeconds(rate);
        }
    }

    IEnumerator SpawBaguetteOverTime(float rate, int amount, GameObject item)
    {
        for (int i = 0; i < amount; i++)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position - transform.forward + transform.up, -transform.up);
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
                Instantiate(item, hit.point + hit.normal / 5, transform.rotation);
            
            yield return new WaitForSeconds(rate);
        }
    }
}
