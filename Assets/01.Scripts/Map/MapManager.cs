using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<MapManager>();

            if (_instance == null)
                _instance = new GameObject() { name = "MapManager" }.AddComponent<MapManager>();

            return _instance;
        }
    }

    private MapBase _mapBase;
    public MapBase mapBase => _mapBase ??= FindAnyObjectByType<MapBase>();

    public string currentMapID;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public string GetMapName()
    {
        currentMapID = SceneManager.GetActiveScene().name;
        return currentMapID;
    }

    public void LoadMap(string mapName)
    {
        SceneManager.LoadScene(mapName);
    }
}
