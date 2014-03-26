using UnityEngine;
using System.Collections;

public class SteeringAI : Car 
{
    const float timerConst = 1.25f;
    public bool userOverride;
    float timer;
    int projectedWaypoint;
    Line projectedSegm;

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
    }

    public override void Start()
    {
        base.Start();
        projectedSegm = WaypointManager.Instance.GetSegment(currentWaypoint);
        StartCoroutine(WrongWayCoroutine());
    }

    public override void Update()
    {
        CheckWaypointSegm();
        if (userOverride && (CInput.GetAxis("Horizontal") != 0 || CInput.GetAxis("Vertical") != 0))
        {
            throttle = CInput.GetAxis("Vertical");
            steer = CInput.GetAxis("Horizontal");
        }
        else
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
        Vector3 projectedPos = transform.position + rigidbody.velocity.magnitude * transform.forward;
        bool inSegm;
        Vector2 newPoint = projectedSegm.MapPointOnLine(projectedPos.ToV2(), out inSegm);
        if (!inSegm)
        {
            if (++projectedWaypoint == WaypointManager.Instance.waypoints.Length)
                projectedWaypoint = 0;
            projectedSegm = WaypointManager.Instance.GetSegment(projectedWaypoint);
            newPoint = projectedSegm.MapPointOnLine(projectedPos.ToV2(), out inSegm);
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
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall")
        {
            foreach (ContactPoint p in other.contacts)
                Debug.DrawLine(p.point, p.point + p.normal, Color.red, 3);

            timer = timerConst;
        }
    }

    float t;
    IEnumerator WrongWayCoroutine()
    {
        while (true)
        {
            if (timer <= 0 && Mathf.Abs(transform.forward.z - projectedSegm.ForwardNormal.z) > 1) //if going the other direction
            {
                t += Time.fixedDeltaTime;
                if (t >= timerConst)
                {
                    t = 0;
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                    transform.position = projectedSegm.aTrans.position + Vector3.up;
                    transform.LookAt(projectedSegm.bTrans);
                }
            }
            yield return new WaitForFixedUpdate();
        }
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
}
