 using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CInput : MonoBehaviour 
{
    public static Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();

    static bool initialised;

    static void Init()
    {
        //use player prefs
        initialised = true;
    }

    public static bool GetKey(string name)
    {
        if (!initialised)
            Init();
        KeyCode keyCode;
        if(keyBindings.TryGetValue(name, out keyCode))
            return Input.GetKey(keyCode); 
        return false;
    }

    public static bool GetKeyDown(string name)
    {
        if (!initialised)
            Init();
        KeyCode keyCode;
        if (keyBindings.TryGetValue(name, out keyCode))
            return Input.GetKeyDown(keyCode);
        return false;
    }

    public static bool GetKeyUp(string name)
    {
        if (!initialised)
            Init();
        KeyCode keyCode;
        if (keyBindings.TryGetValue(name, out keyCode))
            return Input.GetKeyUp(keyCode);
        return false;
    }
}
