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

    public MapBase currentActiveMapScript { get; private set; }

    public string currentMapID {  get; private set; }  // MapBase ��ũ��Ʈ�� Ȱ��ȭ �� �� ���� GetMapName()�� ȣ���Ͽ� ��������

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentActiveMapScript = FindAnyObjectByType<MapBase>();
    }

    public string GetMapName()
    {
        currentMapID = SceneManager.GetActiveScene().name;
        return currentMapID;
    }

    public void LoadMap(string mapID)
    {
        //currentActiveMapScript.OnReleaseMap();

        SceneManager.LoadScene(mapID);
    }
}
