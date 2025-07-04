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
                _instance = new GameObject("MapManager").AddComponent<MapManager>();

            return _instance;
        }
    }

    public V2MapBase MapBase;
    public MapData mapData;

    public string prevMapID;
    public string currentMapID;  // 첫 시작 시 001로 설정
    public Vector3 lastPlayerScale;

    // 퍼즐 버전 맵 클리어 여부
    public bool isClear105 = false;
    public bool isClear108 = false;

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

    private void Start()
    {
        if (currentMapID == null || currentMapID == "")
        {
            currentMapID = "001";
        }

        mapData = Database.Instance.Map.GetMapData(currentMapID);
    }

    // prevMapID를 기존 맵 ID로 바꾸고 currentMapID를 새 맵 ID로 바꾸고
    // mapData를 currentMapID로 업데이트 하고 현재 맵의 플레이어 좌표를 지정한 좌표로 이동하기
    public void UpdateMapData(string newMapID)
    {
        prevMapID = currentMapID;
        currentMapID = newMapID;

        mapData = Database.Instance.Map.GetMapData(currentMapID);

        Debug.Log($"맵 데이터가 업데이트되었습니다: {currentMapID}");
    }

    public void OnLeftMap()
    {
        UpdateMapData(mapData.left_map);
        PlayerController.Instance.transform.position = mapData.player_position_right;
    }
    public void OnRightMap()
    {
        UpdateMapData(mapData.right_map);
        PlayerController.Instance.transform.position = mapData.player_position_left;
    }
    public void OnUpMap()
    {
        UpdateMapData(mapData.up_map);
        PlayerController.Instance.transform.position = mapData.player_position_down;
    }
    public void OnDownMap()
    {
        UpdateMapData(mapData.down_map);
        PlayerController.Instance.transform.position = mapData.player_position_up;
    }
}
