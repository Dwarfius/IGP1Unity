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

    public static void EnableRenders(GameObject gO, bool state)
    {
        Renderer[] renderers = gO.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
            renderer.enabled = state;
    }

    public static GameObject GetPrefab(this Cars car)
    {
        if (car == Cars.Serpent)
            return (GameObject)Resources.Load("Prefabs/Cars/Snake Car");
        else if (car == Cars.Popcorn)
            return (GameObject)Resources.Load("Prefabs/Cars/Popcorn Car");
        else if (car == Cars.Janitor)
            return (GameObject)Resources.Load("Prefabs/Cars/Bucket Car");
        else if (car == Cars.Gorilla)
            return (GameObject)Resources.Load("Prefabs/Cars/Banana Car");
        else if (car == Cars.French)
            return (GameObject)Resources.Load("Prefabs/Cars/French Car");
        else
            return (GameObject)Resources.Load("Prefabs/Cars/Bottle Car");
    }

    public static Texture2D GetMinimapTexture(this Cars car)
    {
        if(car == Cars.Serpent)
            return (Texture2D)Resources.Load("Textures/snake bubble");
        else if(car == Cars.Popcorn)
            return (Texture2D)Resources.Load("Textures/popcorn bubble");
        else if(car == Cars.Janitor)
            return (Texture2D)Resources.Load("Textures/janitor bubble");
        else if(car == Cars.Gorilla)
            return (Texture2D)Resources.Load("Textures/gorilla bubble");
        else if(car == Cars.French)
            return (Texture2D)Resources.Load("Textures/french bubble");
        else
            return (Texture2D)Resources.Load("Textures/cola bubble");
    }
}
