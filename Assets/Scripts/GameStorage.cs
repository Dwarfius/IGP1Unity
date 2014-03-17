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
    public struct CarStorage
    {
        public int lap;
        public float distance;
        public Cars carName;
        public Car carScript;
    }
    #endregion

    public CarStorage[] cars = new CarStorage[6];
    public int carIndex;
    public bool canUpdate;

    float circleDist;

    void Start()
    {
        for (int i = 0; i < WaypointManager.Instance.waypoints.Length; i++)
            circleDist += WaypointManager.Instance.GetSegment(i).Distance;
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

            for (int i = 0; i < cars.Length; i++)
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
}
