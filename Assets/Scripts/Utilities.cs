using UnityEngine;
using System.Collections;

public static class Utilities 
{
    public static Vector2 ToV2(this Transform t)
    {
        return ToV2(t.position);
    }

    public static Vector2 ToV2(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 ToV3(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
    {
        return new Vector3(Mathf.Lerp(from.x, to.x, t), Mathf.Lerp(from.y, to.y, t), Mathf.Lerp(from.z, to.z, t));
    }
}
