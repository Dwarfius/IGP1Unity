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

    public int carIndex;
}
