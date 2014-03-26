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
        public float distance;
        public Cars carName;
        public Car carScript;
    }
    #endregion

    public static float minimapX1 = 190, minimapX2 = 1796, minimapY1 = 449, minimapY2 = 1580;
    public static int lapsToFinish = 1;

    public CarStorage[] cars = null;
    public int carIndex = -1;
    public bool canUpdate;
    public GUISkin skin;
    public bool ticketFound;
    public int ticketAmount;

    float circleDist;
    Texture2D displayTexture, red, yellow, green;

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

                GameObject car = (GameObject)Instantiate(((Cars)type).GetPrefab(), positions[i].position, Quaternion.identity);

                Car carScript = car.GetComponent<Car>();
                cars[i].carScript = car.AddComponent<SteeringAI>();
                (cars[i].carScript as SteeringAI).InitWithCarScript(carScript);
                Destroy(carScript);

                cars[i].carName = (Cars)type;
                cars[i].carScript.enabled = true;
                car.name = cars[i].carName.ToString();
            }

            //spawning player on last position
            GameObject playerCar = (GameObject)Instantiate(((Cars)carIndex).GetPrefab(), positions[5].position, Quaternion.identity);

            cars[5].carScript = playerCar.GetComponent<Car>();
            cars[5].carName = (Cars)carIndex;

            playerCar.GetComponentInChildren<Camera>().enabled = true;
            playerCar.name = cars[5].carName.ToString() + " - Player";

            StartCoroutine(StartCounter());

            //marking to follow stats
            canUpdate = true;
        }
        else
            canUpdate = false;
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
                for (int j = i + 1; j < cars.Length; j++)
                {
                    if (cars[i].distance < cars[j].distance)
                        Swap(i, j);
                }
            }
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
            if (cars[i].carName == carType && ++cars[i].lap >= lapsToFinish)
                cars[i].carScript.finished = true;
        }
    }

    public bool IsFirst(Cars carType)
    {
        return cars[0].carName == carType;
    }

    public void FinishGame(bool ticketFound, bool retry)
    {
        
    }

    IEnumerator StartCounter()
    {
        foreach (CarStorage car in cars)
            car.carScript.rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        float time = 4;
        while ((time -= 1) > 0)
        {
            if (time == 3)
                Debug.Log("Ready");
            else if (time == 2)
                Debug.Log("Steady");
            else
            {
                Debug.Log("Go");
                foreach (CarStorage car in cars)
                    car.carScript.rigidbody.constraints = RigidbodyConstraints.None;
            }
            yield return new WaitForSeconds(1);
        }
    }
}
