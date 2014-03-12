using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour 
{
    #region CarStorage
    [System.Serializable] public class CarStorage
    {
        public GameObject carDriverPair;
        public string name;

        [HideInInspector] public Transform carTransform;
        [HideInInspector] public int pos;
    }
    #endregion
    enum State { MainMenu, Options, KartSelect }

    public Vector3 btnSize; //z contains the empty space
    public CarStorage[] cars;
    public Vector3[] carPositions;
    public float rotationTime;
    public float rotationSpeed;

    int selectedCar = 0, switchFlag;
    State state = State.MainMenu;
    Dictionary<string, KeyCode> keyBinds;
    string editingKey = null;
    float counter = 0;

    void Start()
    {
        keyBinds = CInput.GetKeyBindings();
        for (int i = 0; i < carPositions.Length; i++)
        {
            cars[i].carTransform = ((GameObject)Instantiate(cars[i].carDriverPair, carPositions[i], Quaternion.identity)).transform;
            cars[i].carTransform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            Utilities.EnableRenders(cars[i].carTransform.gameObject, false);
            cars[i].pos = i;
        }
    }

    void Update()
    {
        if (CInput.GetKeyDown("Right"))
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
                    cars[index].carTransform.position = Utilities.Lerp(carPositions[cars[index].pos], carPositions[nextIndex], counter);
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

        foreach (CarStorage carStorage in cars)
        {
            Vector3 rot = carStorage.carTransform.eulerAngles;
            rot.y += rotationSpeed * Time.deltaTime;
            carStorage.carTransform.eulerAngles = rot;
        }
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
        else if (state == State.KartSelect)
            DrawKartSelect();
    }

    void DrawMenu()
    {
        float x = Screen.width / 2, y = Screen.height / 2;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Play The Game"))
        {
            state = State.KartSelect;
            foreach (CarStorage carStorage in cars)
                Utilities.EnableRenders(carStorage.carTransform.gameObject, true);
        }

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

    void DrawKartSelect()
    {
        float x = Screen.width / 2, y = Screen.height / 10;

        if (GUI.Button(new Rect(x, y, btnSize.x, btnSize.y), "Next"))
        {
            if (++selectedCar == cars.Length) 
                selectedCar = 0;
            switchFlag = -1;
        }
        else if (GUI.Button(new Rect(x - btnSize.x - btnSize.z, y, btnSize.x, btnSize.y), "Prev"))
        {
            if (--selectedCar == -1)
                selectedCar = cars.Length - 1;
            switchFlag = 1;
        }

        y += btnSize.y + btnSize.z;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Play"))
            Application.LoadLevel(1);

        y += btnSize.y + btnSize.z;
        if (GUI.Button(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Back"))
        {
            state = State.MainMenu;
            foreach (CarStorage carStorage in cars)
                Utilities.EnableRenders(carStorage.carTransform.gameObject, false);
        }

        y += btnSize.y + btnSize.z;
        GUI.Box(new Rect(x - btnSize.x / 2, y, btnSize.x, btnSize.y), "Car: " + cars[selectedCar].name);
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
