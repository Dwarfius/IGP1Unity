using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour 
{
    enum State { MainMenu, Options, LevelSelect, KartSelect }

    public Vector3 btnSize; //z contains the empty space
    public GameObject[] karts;

    State state = State.MainMenu;
    Dictionary<string, KeyCode> keyBinds;
    string editingKey = null;

    void Start()
    {
        keyBinds = CInput.GetKeyBindings();
    }

    void OnGUI()
    {
        if (state == State.MainMenu)
            DrawMenu();
        else if (state == State.Options)
        {
            DrawOptions();
            if (!string.IsNullOrEmpty(editingKey))
                SetKeyBind();
        }
        else if (state == State.LevelSelect)
            DrawLevelSelect();
        else if (state == State.KartSelect)
            DrawKartSelect();
    }

    void DrawMenu()
    {
        float x = Screen.width / 2, y = Screen.height / 2;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Play The Game"))
            state = State.LevelSelect;

        y += btnSize.y + btnSize.z;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Options"))
            state = State.Options;

        y += btnSize.y + btnSize.z;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Exit"))
            Application.Quit();
    }

    void DrawOptions()
    {
        float x = Screen.width / 5, y = Screen.height / 10;
        GUI.Label(new Rect((Screen.width - btnSize.x) / 2, y, btnSize.x, btnSize.y), "Hotkeys");

        y += btnSize.y + btnSize.z;
        foreach(KeyValuePair<string, KeyCode> pair in keyBinds)
        {
            GUI.Label(new Rect(x, y, btnSize.x, btnSize.y), pair.Key);
            string dispString = (string.IsNullOrEmpty(editingKey) || !editingKey.Equals(pair.Key) ? pair.Value.ToString() : "Press Any Key");
            if (GUI.Button(new Rect(4 * x - btnSize.x, y, btnSize.x, btnSize.y), dispString) && string.IsNullOrEmpty(editingKey))
                editingKey = pair.Key;
            y += btnSize.y + btnSize.z;
        }

        if (GUI.Button(new Rect((Screen.width - btnSize.x) / 2, y, btnSize.x, btnSize.y), "Reset"))
            CInput.Reset();

        y += btnSize.y + btnSize.z;
        if (GUI.Button(new Rect((Screen.width - btnSize.x) / 2, y, btnSize.x, btnSize.y), "Back"))
            state = State.MainMenu;
    }

    void DrawLevelSelect()
    {
        float x = Screen.width / 2, y = Screen.height / 10;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Select Kart"))
            state = State.KartSelect;

        y += btnSize.y + btnSize.z;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Back"))
            state = State.MainMenu;
    }

    void DrawKartSelect()
    {
        float x = Screen.width / 2, y = Screen.height / 10;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Play"))
            Application.LoadLevel(1);

        y += btnSize.y + btnSize.z;
        if (GUI.Button(new Rect(x - btnSize.x/2, y, btnSize.x, btnSize.y), "Back"))
            state = State.LevelSelect;
    }

    void SetKeyBind()
    {
        Event e = Event.current;
        if (e.isKey && e.type == EventType.keyDown)
        {
            CInput.ModifyKey(editingKey, e.keyCode);
            editingKey = null;
        }
    }
}
