 using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CInput 
{
    static Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();
    static bool initialised;
    static string[] keys =            { "Up",      "Down",    "Left",    "Right",   "Use Item",    "Reset",   "Pause" };
    static KeyCode[] defaultHotKeys = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Space, KeyCode.R, KeyCode.Escape };

    static void Init()
    {
        if (PlayerPrefs.HasKey("Up")) //if playerprefs exists
        {
            foreach (string key in keys)
                keyBindings.Add(key, (KeyCode)PlayerPrefs.GetInt(key));
        }
        else //creating default keybinds
            Reset();
        initialised = true;
    }

    public static Dictionary<string, KeyCode> GetKeyBindings()
    {
        if (!initialised)
            Init();
        return keyBindings;
    }

    public static void ModifyKey(string name, KeyCode key)
    {
        if (keyBindings.ContainsKey(name))
        {
            keyBindings[name] = key;
            PlayerPrefs.SetInt(name, (int)key);
            PlayerPrefs.Save();
        }
        else
            Debug.LogError("CInput: There is no such key \"" + name + "\"");
    }

    public static void Reset()
    {
        keyBindings.Clear();
        for (int i = 0; i < keys.Length; i++)
        {
            keyBindings.Add(keys[i], defaultHotKeys[i]);
            PlayerPrefs.SetInt(keys[i], (int)defaultHotKeys[i]);
        }
        PlayerPrefs.Save();
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

    public static KeyCode GetKeyRepresentation(string name)
    {
        if (!initialised)
            Init();

        KeyCode keyCode;
        if (keyBindings.TryGetValue(name, out keyCode))
            return keyCode;
        return 0;
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

    public static float GetAxis(string name)
    {
        if (!initialised)
            Init();

        if (name == "Horizontal")
            return (GetKey("Left") ? -1 : 0) + (GetKey("Right") ? 1 : 0);
        else if (name == "Vertical")
            return (GetKey("Down") ? -1 : 0) + (GetKey("Up") ? 1 : 0);
        return 0;
    }
}
