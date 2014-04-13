using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour 
{
    #region CarStorage
    [System.Serializable] public class CarStorage
    {
        public Transform carDriverPair;
        public string name;

        [HideInInspector] public int pos;
    }
    #endregion
    public enum State { MainMenu, Options, KartSelect, Ticket, Instructions }

    public Vector3 btnSize; //z contains the empty space
    public Vector2 btnCenterOffset;
    public CarStorage[] cars;
    public Vector3[] carPositions;
    public float rotationTime;
    public float rotationSpeed;
    public GUISkin skin;
    public Texture2D background;
    public Texture2D ticketImg;
    public Texture2D progressBar;
    public Vector2 pos, size;

    [HideInInspector] public State state = State.MainMenu;

    int selectedCar = 1, switchFlag;
    Dictionary<string, KeyCode> keyBinds;
    string editingKey = null;
    float counter = 0;

    void Start()
    {
        GameStorage.Instance.skin = skin;
        keyBinds = CInput.GetKeyBindings();
        for (int i = 0; i < carPositions.Length; i++)
        {
            cars[i].carDriverPair.position = carPositions[i];
            cars[i].carDriverPair.GetChild(0).localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            Utilities.EnableRenders(cars[i].carDriverPair.gameObject, false);
            cars[i].pos = i;
        }
    }

    void Update()
    {
        if (switchFlag != 0)
        {
            if ((counter += Time.deltaTime / rotationTime) < 1)
            {
                for (int i = 0; i < cars.Length; i++)
                {
                    int index = (switchFlag == 1 ? i : cars.Length - 1 - i);
                    int nextIndex = cars[index].pos + switchFlag;
                    if (nextIndex == -1)
                        nextIndex = cars.Length - 1;
                    else if (nextIndex == cars.Length)
                        nextIndex = 0;
                    cars[index].carDriverPair.position = Utilities.Lerp(carPositions[cars[index].pos], carPositions[nextIndex], counter);
                }
            }
            else
            {
                counter = 0;
                foreach (CarStorage carStorage in cars)
                {
                    carStorage.pos += switchFlag;
                    if (carStorage.pos == -1)
                        carStorage.pos = cars.Length - 1;
                    else if (carStorage.pos == cars.Length)
                        carStorage.pos = 0;
                }
                switchFlag = 0;
            }
        }
        else if (CInput.GetKeyDown("Right"))
        {
            switchFlag = -1;
            if (++selectedCar == cars.Length)
                selectedCar = 0;
        }
        else if (CInput.GetKeyDown("Left"))
        {
            switchFlag = 1;
            if (--selectedCar == -1)
                selectedCar = cars.Length - 1;
        }
    }

    void OnGUI()
    {
        GUI.skin = skin;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background);
        if (state == State.MainMenu)
            DrawMenu();
        else if (state == State.Options)
        {
            DrawOptions();
            if (!string.IsNullOrEmpty(editingKey))
                SetKeyBind();
        }
        else if (state == State.KartSelect)
            DrawKartSelect();
        else if (state == State.Ticket)
            DrawTicketScreen();
        else if (state == State.Instructions)
            DrawInstructions();
    }

    void DrawMenu()
    {
        float x = Screen.width / 2, y = Screen.height / 2;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Play The Game"))
        {
            state = State.KartSelect;
            foreach (CarStorage carStorage in cars)
                Utilities.EnableRenders(carStorage.carDriverPair.gameObject, true);
        }

        y += btnSize.y + btnSize.z;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Instructions"))
            state = State.Instructions;

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
        GUI.Box(new Rect((Screen.width - btnSize.x) / 2, y, btnSize.x, btnSize.y), "Options");

        y += btnSize.y + btnSize.z;
        GUI.Box(new Rect(x, y, btnSize.x, btnSize.y), "Audio Volume");
        GameStorage.Instance.audio.volume = GUI.HorizontalSlider(new Rect(4 * x - btnSize.x * 3, y + btnSize.y/3, btnSize.x * 1.5f, btnSize.y), GameStorage.Instance.audio.volume, 0, 1);
        GUI.Box(new Rect(4 * x - btnSize.x /2, y, btnSize.x/2, btnSize.y), (int)(GameStorage.Instance.audio.volume * 100) + "%");

        y += btnSize.y + btnSize.z;
        foreach(KeyValuePair<string, KeyCode> pair in keyBinds)
        {
            GUI.Box(new Rect(x, y, btnSize.x, btnSize.y), pair.Key);
            string dispString = (string.IsNullOrEmpty(editingKey) || !editingKey.Equals(pair.Key) ? pair.Value.ToString() : "Press Any Key");
            if (GUI.Button(new Rect(4 * x - btnSize.x, y, btnSize.x, btnSize.y), dispString) && string.IsNullOrEmpty(editingKey))
                editingKey = pair.Key;
            y += btnSize.y + btnSize.z;
        }

        if (GUI.Button(new Rect((Screen.width - btnSize.x) / 2, y, btnSize.x, btnSize.y), "Reset Hotkeys"))
            CInput.Reset();

        y += btnSize.y + btnSize.z;
        if (GUI.Button(new Rect((Screen.width - btnSize.x) / 2, y, btnSize.x, btnSize.y), "Back"))
        {
            PlayerPrefs.SetFloat("Volume", GameStorage.Instance.audio.volume);
            state = State.MainMenu;
        }
    }

    void DrawKartSelect()
    {
        float xLeft = Screen.width / 2 - btnSize.x / 2 - (Screen.width / 2 * btnCenterOffset.x);
        float xRight = Screen.width / 2 - btnSize.x / 2 + (Screen.width / 2 * btnCenterOffset.x);
        float y = Screen.height * btnCenterOffset.y;

        KeyCode rightKey, leftKey;
        string nextString = "Next", prevString = "Prev";
        if (keyBinds.TryGetValue("Right", out rightKey))
            nextString = "Next (" + rightKey + ")";
        if (keyBinds.TryGetValue("Left", out leftKey))
            prevString = "Prev (" + leftKey + ")";

        if (GUI.Button(new Rect(xLeft, y, btnSize.x, btnSize.y), nextString) && switchFlag == 0)
        {
            if (++selectedCar == cars.Length)
                selectedCar = 0;
            switchFlag = -1;
        }
        else if (GUI.Button(new Rect(xRight, y, btnSize.x, btnSize.y), prevString) && switchFlag == 0)
        {
            if (--selectedCar == -1)
                selectedCar = cars.Length - 1;
            switchFlag = 1;
        }

        float x = Screen.width / 2; 
        y = Screen.height / 10;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Play"))
        {
            GameStorage.Instance.carIndex = selectedCar;
            Application.LoadLevel(1);
        }

        y += btnSize.y + btnSize.z;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Back"))
        {
            state = State.MainMenu;
            foreach (CarStorage carStorage in cars)
                Utilities.EnableRenders(carStorage.carDriverPair.gameObject, false);
        }

        y += btnSize.y + btnSize.z;
        string difficulty = (selectedCar == 1 || selectedCar == 3) ? "Hard" : (selectedCar == 4 || selectedCar == 5) ? "Medium" : "Easy"; 
        GUI.Box(new Rect(x - btnSize.x * 1.5f / 2, y, btnSize.x * 1.5f, btnSize.y * 2), "Car: " + cars[selectedCar].name + "\nDifficulty: " + difficulty);
    }

    void DrawTicketScreen()
    {
        float xOffsetCoeff = 0.2f, yOffsetCoeff = 0.2f;
        Rect imgRect = new Rect(Screen.width * xOffsetCoeff, Screen.height * yOffsetCoeff, Screen.width * (1 - xOffsetCoeff * 2), Screen.height * (1 - yOffsetCoeff * 2));

        GUI.DrawTexture(imgRect, ticketImg);
        float percent = GameStorage.Instance.ticketAmount / (float)GameStorage.ticketsMax;
        Debug.Log(percent);
        GUI.DrawTextureWithTexCoords(new Rect(imgRect.xMin + imgRect.width * pos.x, imgRect.yMin + imgRect.height * pos.y, imgRect.width * size.x * percent, imgRect.height * size.y),
                                     progressBar,
                                     new Rect(0, 0, percent, 1));
        if (GUI.Button(new Rect(Screen.width / 2 - btnSize.x / 2, imgRect.yMax + btnSize.z, btnSize.x, btnSize.y), "Menu"))
            state = State.MainMenu;
    }

    void DrawInstructions()
    {
        string header = "Welcome to Serpent Cinema Racing Game!";
        string text = "This is a simple car racing game, where the goal is to reach finish first, after completing 3 laps. The controls are simple:\n" +
                      "WSAD for driving, Space for using a powerup, R for resetting back to last checkpoint (if you are stuck or something else happens)\n" + 
                      "Escape for pause. All the hotkeys can be changes inside the Options menu. During the race, you can pick up a Power up, which looks\n" +
                      "lika a floating Gift. Those usually contain a character-specific powerup, which can be used, but it also can contain a Golden Ticket.\n" +
                      "Golden Tickets are used to provide a discount in our Serpent Cinema, the more you collect, the bigger the discount (maximum is 20%),\n" +
                      "but there is a catch - the ticket will count if you finish first with it. That's it, this is the entire game, now you're ready to start\n" +
                      "racing!";
        Vector2 headerSize = GUI.skin.label.CalcSize(new GUIContent(header)) * 1.5f;
        Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text)) * 1.2f;
        float boxWidth = textSize.x + btnSize.z * 2;
        float boxHeight = headerSize.y + textSize.y + btnSize.y + btnSize.z * 4;
        Rect boxRect = new Rect(Screen.width / 2 - boxWidth / 2, Screen.height / 2 - boxHeight / 2, boxWidth, boxHeight);
        //GUI.Box(boxRect, "");
        GUI.Label(new Rect(boxRect.center.x - headerSize.x / 2, boxRect.yMin + btnSize.z, headerSize.x, headerSize.y), header);
        GUI.Label(new Rect(boxRect.xMin + btnSize.z, boxRect.yMin + btnSize.z * 2 + headerSize.y, textSize.x, textSize.y), text);
        if (GUI.Button(new Rect(boxRect.xMax - btnSize.x - btnSize.z, boxRect.yMax - btnSize.y - btnSize.z, btnSize.x, btnSize.y), "Back"))
            state = State.MainMenu;
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < carPositions.Length; i++)
            Gizmos.DrawCube(carPositions[i], Vector3.one);
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
