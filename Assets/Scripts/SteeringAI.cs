using UnityEngine;
using System.Collections;

public class SteeringAI : Car 
{
    const float timerConst = 2f;
    float timer;

    public void InitWithCarScript(Car script)
    {
        car = script.car;
        frontWheels = (Transform[])script.frontWheels.Clone();
        backWheels = (Transform[])script.backWheels.Clone();
        slipValue = script.slipValue;
        stiffnesCoeff = script.stiffnesCoeff;
        gears = script.gears;
        topSpeed = script.topSpeed;
        maximumTurn = script.maximumTurn;
        minimumTurn = script.minimumTurn;
        resetTime = script.resetTime;
        centerOfMass = script.centerOfMass;
        charScale = script.charScale;
        powerupPrefab = script.powerupPrefab;
    }

    public override void Start()
    {
        base.Start();
        StartCoroutine(WrongWayCoroutine());
        StartCoroutine(StuckCoroutine());
    }

    public override void Update()
    {
        CheckWaypointSegm();
        MakeDecision();
        CheckIfFlipped();
        Vector3 relativeVel = transform.InverseTransformDirection(rigidbody.velocity);
        UpdateGear(relativeVel);
    }

    public override void OnGUI() 
    { 
        DrawMinimap(); 
    }

    //===========================================================================
    void MakeDecision()
    {
        int projectedWaypoint = currentWaypoint;
        Line projectedSegm = WaypointManager.Instance.GetSegment(projectedWaypoint);
        bool inSegm = false;
        Vector2 newPoint;
        Vector3 projectedPos = transform.position + rigidbody.velocity.magnitude * transform.forward;

        while(true) //searching for the actual segment, starting from the current one
        {
            newPoint = projectedSegm.MapPointOnLine(projectedPos.ToV2(), out inSegm);
            if (!inSegm)
            {
                if (++projectedWaypoint == WaypointManager.Instance.waypoints.Length)
                    projectedWaypoint = 0;
                projectedSegm = WaypointManager.Instance.GetSegment(projectedWaypoint);
            }
            else break;
        }

        float rad = projectedSegm.GetRadiusForMappedPoint(newPoint);
        Color color;
        if (timer <= 0)
        {
            if ((projectedPos.ToV2() - newPoint).sqrMagnitude > rad * rad)
            {
                throttle = (rigidbody.velocity.sqrMagnitude < 100 ? 1 : 0);
                steer = projectedSegm.IsLeftOfLine(projectedPos.ToV2());
                color = Color.red;
            }
            else
            {
                throttle = 1;
                steer = 0;
                color = Color.green;
            }
        }
        else
        {
            if (timer > timerConst / 4) //75% spent on reverse
            {
                throttle = -0.1f;
                steer = -projectedSegm.IsLeftOfLine(projectedPos.ToV2());
            }
            else //remaining %25 on repositioning
            {
                if ((projectedPos.ToV2() - newPoint).sqrMagnitude > rad * rad)
                {
                    throttle = (rigidbody.velocity.sqrMagnitude < 100 ? 1 : 0);
                    steer = projectedSegm.IsLeftOfLine(projectedPos.ToV2());
                    color = Color.red;
                }
                else
                {
                    throttle = 1;
                    steer = 0;
                    color = Color.green;
                }
            }
            color = Color.blue;
            timer -= Time.deltaTime;
        }
        Debug.DrawLine(new Vector3(projectedPos.x, transform.position.y, projectedPos.z), new Vector3(newPoint.x, transform.position.y, newPoint.y), color);

        //ability
        if (hasPowerup)
        {
            if (car == Cars.Serpent || car == Cars.Cola || car == Cars.French || car == Cars.Janitor)
                UsePowerUp();
            else //gorilla or popcorn
            {
                RaycastHit hit;
                Ray ray = new Ray(transform.position, transform.forward);
                if (Physics.SphereCast(ray, 3, out hit, 40, 1 << LayerMask.NameToLayer("Cars")))
                    UsePowerUp();
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall")
            timer = timerConst;
    }

    void DrawMinimap()
    {
        if (minimapChar == null)
            return;

        Vector2 minimapSize = Vector2.Scale(new Vector2(minimap.width, minimap.height), minimapScale);
        Vector2 relativePos = transform.ToV2() - minimapStartOffset;
        relativePos = new Vector2(relativePos.x / trackSize.x, relativePos.y / trackSize.y); //[0..1]
        Vector2 minimapPos = Vector2.Scale(relativePos, minimapSize); //[0..minimapSize]
        Vector2 charSize = Vector2.Scale(new Vector2(minimapChar.width, minimapChar.height), charScale);
        GUI.DrawTexture(new Rect(minimapPos.x - charSize.x / 2, Screen.height - minimapPos.y - charSize.y / 2, charSize.x, charSize.y), minimapChar);
    }

    float t;
    IEnumerator WrongWayCoroutine()
    {
        while (true)
        {
            if (timer <= 0 && Mathf.Abs(transform.forward.z - currentSegm.ForwardNormal.z) > 1) //if going the other direction
            {
                t += Time.fixedDeltaTime;
                if (t >= timerConst)
                {
                    t = 0;
                    ResetCar();
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }

    Vector3 prevPos = Vector3.zero;
    IEnumerator StuckCoroutine()
    {
        yield return new WaitForSeconds(4);
        while (true)
        {
            float distance = (transform.position - prevPos).magnitude;
            if (distance < 1)
                ResetCar();
            prevPos = transform.position;
            yield return new WaitForSeconds(1);
        }
    }
}
