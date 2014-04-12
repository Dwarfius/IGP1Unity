using UnityEngine;
using System.Collections;

public enum Cars
{
    Serpent, Janitor, Cola, Popcorn, French, Gorilla
}

public class GameStorage : MonoBehaviour 
{
    #region Singleton implementation
    static GameStorage instance;
    public static GameStorage Instance
    {
        get
        {
            if (!instance)
            {
                GameObject go = new GameObject("GameStorage");
                instance = go.AddComponent<GameStorage>();
                instance.Init();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    #endregion

    #region CarStorage
    public class CarStorage
    {
        public int lap;
        public float time;
        public float distance;
        public Cars carName;
        public Car carScript;
        public bool passedJump;
    }
    #endregion

    public static float minimapX1 = 623, minimapX2 = 1772, minimapY1 = 107, minimapY2 = 1107;
    public static int lapsToFinish = 3;
    public static int ticketsMax = 20;

    public CarStorage[] cars = null;
    public int carIndex = -1;
    public bool canUpdate;
    public GUISkin skin;
    public bool ticketFound;
    public int ticketAmount;
    public Texture2D ticket;

    float circleDist;
    Texture2D red, yellow, green, background;
    GameObject cameraPrefab;

    public void Init()
    {
        red = (Texture2D)Resources.Load("Textures/redlight");
        yellow = (Texture2D)Resources.Load("Textures/yellowlight");
        green = (Texture2D)Resources.Load("Textures/greenlight");
        background = (Texture2D)Resources.Load("Textures/light background");
        ticket = (Texture2D)Resources.Load("Textures/ticketcollected");
        ticketAmount = PlayerPrefs.GetInt("Tickets");
        cameraPrefab = (GameObject)Resources.Load("Prefabs/Camera");

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = (AudioClip)Resources.Load("Music/Dandy");
        audioSource.loop = true;
        audioSource.Play();
    }

    void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        {
            for (int i = 0; i < WaypointManager.Instance.waypoints.Length; i++) //caching total lap distance
                circleDist += WaypointManager.Instance.GetSegment(i).Distance;

            Transform[] positions = new Transform[6];
            cars = new CarStorage[6];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = GameObject.Find("Pos" + (i + 1)).transform; //gathering all starting positions
                cars[i] = new CarStorage(); //also initing the array for future use
            }

            int[] usedIndices = new int[5]; //the last position is given to player
            int[] potentialTypes = new int[5];
            int j = 0;
            for (int i = 0; i < usedIndices.Length; i++) //mixing AI positions
            {
                usedIndices[i] = -1;
                if (i == carIndex)
                    j++;
                potentialTypes[i] = i + j;
            }

            for (int i = 0; i < potentialTypes.Length; i++) //spawning AIs
            {
                int type = -1;
                while (WasUsed(type, usedIndices))
                    type = potentialTypes[Random.Range(0, 5)];
                usedIndices[i] = type;

                GameObject car = (GameObject)Instantiate(((Cars)type).GetPrefab(), positions[i].position, positions[i].rotation);

                Car carScript = car.GetComponent<Car>();
                cars[i].carScript = car.AddComponent<SteeringAI>();
                (cars[i].carScript as SteeringAI).InitWithCarScript(carScript);
                Destroy(carScript);

                cars[i].carName = (Cars)type;
                cars[i].carScript.enabled = true;
                car.name = cars[i].carName.ToString();
            }

            //spawning player on last position
            GameObject playerCar = (GameObject)Instantiate(((Cars)carIndex).GetPrefab(), positions[5].position, positions[5].rotation);

            cars[5].carScript = playerCar.GetComponent<Car>();
            cars[5].carName = (Cars)carIndex;

            playerCar.name = cars[5].carName.ToString() + " - Player";

            GameObject cam = (GameObject)Instantiate(cameraPrefab, playerCar.transform.position - playerCar.transform.forward, Quaternion.identity);
            cam.GetComponent<CarFollowCamera>().target = playerCar.transform;
            cam.transform.LookAt(playerCar.transform);

            StartCoroutine(StartCounter());

            //music
            audio.Stop();
            audio.clip = (AudioClip)Resources.Load("Music/Dandy");
            audio.Play();

            //marking to follow stats
            canUpdate = true;
        }
        else
        {
            //music
            audio.Stop();
            audio.clip = (AudioClip)Resources.Load("Music/Dandy");
            audio.Play();

            canUpdate = false;
            if (level == 0)
                Camera.main.gameObject.GetComponent<MainMenu>().state = MainMenu.State.Ticket;
        }
    }

    void Update()
    {
        if (canUpdate)
        {
            //gathering info
            for (int i = 0; i < cars.Length; i++ )
            {
                CarStorage car = cars[i];
                Line l = WaypointManager.Instance.GetSegment(car.carScript.currentWaypoint);
                bool test = false;
                Vector2 pos = l.MapPointOnLine(car.carScript.transform.ToV2(), out test);
                car.distance = car.lap * circleDist;
                for (int j = 0; j < car.carScript.currentWaypoint; j++)
                    car.distance += WaypointManager.Instance.GetSegment(j).Distance;
                car.distance += (pos - l.A).magnitude;
            }

            for (int i = 0; i < cars.Length; i++) //derp sort
            {
                if (!cars[i].carScript.finished)
                {
                    for (int j = i + 1; j < cars.Length; j++)
                    {
                        if (cars[i].distance < cars[j].distance)
                            Swap(i, j);
                    }
                }
            }
        }
    }

    void OnGUI()
    {
        if (time > 0)
        {
            float xOffsetCoeff = 0.35f, yOffsetCoeff = 0.15f;
            float xCoeff = 0.30f, yCoeff = 0.1f;
            Rect rect = new Rect(Screen.width * xOffsetCoeff, Screen.height * yOffsetCoeff, Screen.width * xCoeff, Screen.height * yCoeff);
            GUI.DrawTexture(rect, background);
            float emptySpace = rect.width * 0.05f;
            float y = rect.yMin + rect.height * 0.05f;
            float height = rect.height - 2 * rect.height * 0.05f;
            float width = (rect.width - emptySpace) / 3 - emptySpace;
            if(time <= 3)
                GUI.DrawTexture(new Rect(rect.xMin + emptySpace, y, width, height), red);
            if(time <= 2)
                GUI.DrawTexture(new Rect(rect.xMin + width + 2 * emptySpace, y, width, height), yellow);
            if(time <= 1)
                GUI.DrawTexture(new Rect(rect.xMin + 2 * width + 3 * emptySpace, y, width, height), green);
        }
    }

    void Swap(int i, int j)
    {
        CarStorage tmp = cars[i];
        cars[i] = cars[j];
        cars[j] = tmp;
    }

    bool WasUsed(int t, int[] num)
    {
        for (int i = 0; i < num.Length; i++)
            if (t == num[i])
                return true;
        return false;
    }

    public void AddLap(Cars carType)
    {
        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i].carName == carType && cars[i].passedJump && ++cars[i].lap >= lapsToFinish)
            {
                cars[i].passedJump = false;
                cars[i].carScript.finished = true;
                cars[i].time = Time.timeSinceLevelLoad - cars[i].time;
            }
        }
    }

    public void MarkJumpPassed(Cars carType)
    {
        for (int i = 0; i < cars.Length; i++)
            if (cars[i].carName == carType)
                cars[i].passedJump = true;
    }

    public bool IsFirst(Cars carType)
    {
        return cars[0].carName == carType;
    }

    public void FinishGame(bool first)
    {
        if (first && ticketFound && ticketAmount < 20)
        {
            PlayerPrefs.SetInt("Tickets", ++ticketAmount);
            PlayerPrefs.Save();
            ticketFound = false;
        }
        Application.LoadLevel(0);
    }

    public void Retry()
    {
        Application.LoadLevel(1);
    }

    float time;
    IEnumerator StartCounter()
    {
        foreach (CarStorage car in cars)
            car.carScript.rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        time = 5;
        while ((time -= 1) > 0)
        {
            if(time == 1)
            {
                foreach (CarStorage car in cars)
                {
                    car.carScript.rigidbody.constraints = RigidbodyConstraints.None;
                    car.time = Time.timeSinceLevelLoad;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
}
