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
    public string currentMapID;  // MapBase에서 첫 시작 시 001로 설정, 이후엔 콜라이더에서 맵 이동 시 Instance의 메서드에 의해 변경
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
        if (prevMapID == null || prevMapID == "")
        {
            prevMapID = "001";
        }
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

    // 맵 세팅 관련
    public void SetCurrentMap(string mapID)  // 현재 맵 변수 바꿔주기, OnDirMap 함수에 공통으로 들어갈 내용
    {
        currentMapID = mapID;  // 맵을 이동 했을 때 현재 맵을 새로 이동한 맵의 ID로 설정
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
        UpdateMapData(mapData.left_map);
        PlayerController.Instance.playerTransform.position = mapData.player_position_right;
    }
    public void OnRightMap()
    {
        UpdateMapData(mapData.right_map);
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
