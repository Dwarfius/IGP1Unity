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

    public CarStorage[] cars = null;
    public int carIndex;
    public bool canUpdate;

    float circleDist;

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
                cars[i].carScript = car.GetComponent<SteeringAI>();
                cars[i].carName = (Cars)type;
                cars[i].carScript.car = cars[i].carName; //just a precaution
                cars[i].carScript.enabled = true;
                car.name = cars[i].carName.ToString();
            }

            //spawning player on last position
            GameObject playerCar = (GameObject)Instantiate(((Cars)carIndex).GetPrefab(), positions[5].position, Quaternion.identity);
            Destroy(playerCar.GetComponent<SteeringAI>());
            cars[5].carScript = playerCar.GetComponent<Car>();
            cars[5].carName = (Cars)carIndex;
            cars[5].carScript.car = cars[5].carName; //same, just a precaution
            cars[5].carScript.enabled = true;
            playerCar.GetComponentInChildren<Camera>().enabled = true;
            playerCar.name = cars[5].carName.ToString() + " - Player";

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
            if (cars[i].carName == carType)
                cars[i].lap++;
        }
    }
}
