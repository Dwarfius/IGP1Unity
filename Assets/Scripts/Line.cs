using UnityEngine;
using System.Collections;

public class Line
{
    public Vector2 A, B;
    public Transform aTrans, bTrans;
    float r1, r2;
    float k, c;

    public Vector3 ForwardNormal { get { return (bTrans.position - aTrans.position).normalized; } }

    public Line(Transform a, Transform b)
    {
        aTrans = a;
        bTrans = b;
        A = aTrans.ToV2();
        B = bTrans.ToV2();
        r1 = a.GetComponent<Waypoint>().radius;
        r2 = b.GetComponent<Waypoint>().radius;
        k = (B.y - A.y) / (B.x - A.x);
        c = A.y - k * A.x;
    }

    public Vector2 MapPointOnLine(Vector2 point, out bool inSegment)
    {
        float k2 = -1 / k;
        float c2 = point.y - k2 * point.x;
        float x2 = (c2 - c) / (k - k2);
        float y2 = k2 * x2 + c2;
        Vector2 mappedPoint = new Vector2(x2, y2);
        inSegment = (B - A).magnitude >= (mappedPoint - A).magnitude;
        return mappedPoint;
    }

    public float GetRadiusForMappedPoint(Vector2 point)
    {
        float coeff = (point.x - A.x) / (B.x - A.x); //can be negative (due to point can be before A), but then it'll be clamped to 0
        return Mathf.Lerp(r1, r2, coeff);
    }

    public float IsLeftOfLine(Vector2 point)
    {
        return Mathf.Sign((B.x - A.x) * (point.y - A.y) - (B.y - A.y) * (point.x - A.x));
    }
}
