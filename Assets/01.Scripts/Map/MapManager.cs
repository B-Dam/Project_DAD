using UnityEngine;

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

    // 각 맵에 map_id와 next, prev_map_id를 부여
    // 플레이어가 영역 중앙에 들어오면 맵 이동


    private void Start()
    {

    }

    public void MapUpdate()
    {
        // 씬 로드될 때 마다 맵 정보 새로고침
        Database.Instance.Map.GetMapData(GameManager.Instance.currentMapName);

    }

    public void GetMapID()
    {
        
    }

}
