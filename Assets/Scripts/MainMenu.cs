using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
    enum State { MainMenu, Options, LevelSelect, KartSelect }

    public Vector3 btnSize;

    State state = State.MainMenu;

    void OnGUI()
    {
        if (state == State.MainMenu)
        {
            float x = Screen.width / 2, y = Screen.height / 2;
            if(GUI.Button(new Rect(x - btnSize.x/2, y, btnSize.x, btnSize.y), "Play The Game"))
                state = State.LevelSelect;

            y += btnSize.z;
            if(GUI.Button(new Rect(x - btnSize.x/2, y, btnSize.x, btnSize.y), "Options"))
                state = State.Options;

            y += btnSize.z;
            if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Exit"))
                Application.Quit();
        }
        else if (state == State.Options)
        {
            
        }
        else if (state == State.LevelSelect)
        {

        }
        else if(state == State.KartSelect)
        {

        }
    }
}
