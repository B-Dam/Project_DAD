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
                _instance = new GameObject("MapManager").AddComponent<MapManager>();

            return _instance;
        }
    }

    public MapBase currentActiveMapScript { get; private set; }
    public MapData mapData;

    public string prevMapID;
    public string currentMapID;  // 첫 시작 시 001로 설정
    public Vector3 lastPlayerScale;

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

        // 삭제할 예정
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (currentMapID == null || currentMapID == "")
        {
            currentMapID = "001";
        }

        mapData = Database.Instance.Map.GetMapData(currentMapID);
    }

    // Scene 관련이지만 삭제 예정
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentActiveMapScript = FindAnyObjectByType<MapBase>();
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // currentMapID를 이동한 맵 ID로 바꾸고 mapData를 currentMapID로 업데이트 하고 현재 맵의 플레이어 좌표를 지정한 좌표로 이동하기
    public void UpdateMapData(string newMapID)
    {
        prevMapID = currentMapID;
        currentMapID = newMapID;
        mapData = Database.Instance.Map.GetMapData(currentMapID);

        Debug.Log($"맵 데이터가 업데이트되었습니다: {currentMapID}");
    }

    public void OnLeftMap()
    {
        Debug.Log($"OnLeftMap 함수 작동\nleftMap: {mapData.left_map}");
        UpdateMapData(mapData.left_map);
        Debug.Log($"playerPosition: {mapData.player_position_right}");
        PlayerController.Instance.playerTransform.position = mapData.player_position_right;
    }
    public void OnRightMap()
    {
        Debug.Log($"OnRightMap 함수 작동\nrightMap: {mapData.right_map}");
        UpdateMapData(mapData.right_map);
        Debug.Log($"playerPosition: {mapData.player_position_left}");
        PlayerController.Instance.playerTransform.position = mapData.player_position_left;
    }
    public void OnUpMap()
    {
        UpdateMapData(mapData.up_map);
        PlayerController.Instance.playerTransform.position = mapData.player_position_down;
    }
    public void OnDownMap()
    {
        UpdateMapData(mapData.down_map);
        PlayerController.Instance.playerTransform.position = mapData.player_position_up;
    }













    // 곧 삭제될 메서드
    public string GetMapName()
    {
        currentMapID = SceneManager.GetActiveScene().name;
        return currentMapID;
    }

    // 곧 삭제될 메서드
    public void LoadMap(string mapID)
    {
        currentActiveMapScript.OnReleaseMap();

        SceneManager.LoadScene(mapID);
    }

}
