using UnityEngine;

public class Database : MonoBehaviour
{
    private static Database _instance;
    public static Database Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<Database>();

            if (_instance == null)
                _instance = new GameObject() { name = "DataBase" }.AddComponent<Database>();

            return _instance;
        }
    }

    private MapDB _mapDB;
    public MapDB Map => _mapDB ??= new MapDB();
}
