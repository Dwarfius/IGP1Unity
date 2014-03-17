using UnityEngine;
using System.Collections;

public class SteeringAI : Car 
{
    const float timerConst = 1.25f;
    public bool userOverride;
    float timer;

    public override void Start()
    {
        base.Start();
        StartCoroutine(WrongWayCoroutine());
    }

    public override void Update()
    {
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

    public override void OnGUI() { }

    //===========================================================================
    void MakeDecision()
    {
        Vector3 projectedPos = transform.position + rigidbody.velocity.magnitude * transform.forward;
        bool inSegm;
        Vector2 newPoint = currentSegm.MapPointOnLine(projectedPos.ToV2(), out inSegm);
        if (!inSegm)
        {
            if (++currentWaypoint == WaypointManager.Instance.waypoints.Length)
                currentWaypoint = 0;
            currentSegm = WaypointManager.Instance.GetSegment(currentWaypoint);
            newPoint = currentSegm.MapPointOnLine(projectedPos.ToV2(), out inSegm);
        }

        float rad = currentSegm.GetRadiusForMappedPoint(newPoint);
        Color color;
        if (timer <= 0)
        {
            if ((projectedPos.ToV2() - newPoint).sqrMagnitude > rad * rad)
            {
                throttle = (rigidbody.velocity.sqrMagnitude < 100 ? 1 : 0);
                steer = currentSegm.IsLeftOfLine(projectedPos.ToV2());
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
                steer = -currentSegm.IsLeftOfLine(projectedPos.ToV2());
            }
            else //remaining %25 on repositioning
            {
                if ((projectedPos.ToV2() - newPoint).sqrMagnitude > rad * rad)
                {
                    throttle = (rigidbody.velocity.sqrMagnitude < 100 ? 1 : 0);
                    steer = currentSegm.IsLeftOfLine(projectedPos.ToV2());
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
            if (timer <= 0 && Mathf.Abs(transform.forward.z - currentSegm.ForwardNormal.z) > 1) //if going the other direction
            {
                t += Time.fixedDeltaTime;
                if (t >= timerConst)
                {
                    t = 0;
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                    transform.position = currentSegm.aTrans.position;
                    transform.LookAt(currentSegm.bTrans);
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
