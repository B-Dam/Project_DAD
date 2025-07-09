using System.Collections;
using System.Collections.Generic;
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
    private FadeManager fadeManager;

    private Dictionary<string, bool> puzzleClearStatus = new Dictionary<string, bool>
    {
        { "105", false },
        { "108", false }
    };

    public string prevMapID;
    public string currentMapID;  // 첫 시작 시 001로 설정

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (string.IsNullOrEmpty(currentMapID))
            {
                currentMapID = "001";
            }
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
        fadeManager = FindAnyObjectByType<FadeManager>();
        AudioManager.Instance.PlayBGM("LostSouls"); // 기본 BGM 설정
    }

    // prevMapID를 기존 맵 ID로 바꾸고 currentMapID를 새 맵 ID로 바꾸고
    // mapData를 currentMapID로 업데이트 하고 현재 맵의 플레이어 좌표를 지정한 좌표로 이동하기
    public void UpdateMapData(string newMapID)
    {
        prevMapID = currentMapID;
        currentMapID = newMapID;
        mapData = Database.Instance.Map.GetMapData(currentMapID);

        PuzzleCheck();

        string targetBGM;

        if (currentMapID == "105" || currentMapID == "108")
        {
            targetBGM = "Puzzle_Sound";
        }
        else
        {
            targetBGM = "LostSouls";
        }

        if (AudioManager.Instance != null && AudioManager.Instance.currentBGMName != targetBGM)
        {
            AudioManager.Instance.PlayBGM(targetBGM);
        }

        Debug.Log($"맵 데이터가 업데이트되었습니다: {currentMapID}");
    }

    public void PuzzleCheck()
    {
        // 만약 현재 맵 아이디의 타입이 puzzle이라면
        if (mapData.type == MapType.puzzle)
        {
            // bool isClear를 검수하고 false라면 퍼즐 오브젝트를 SetActive(false)로 설정
            puzzleClearStatus.TryGetValue(currentMapID, out bool isClear);
            if (isClear)
            {

            }
        }

        // Bridge 프리펩을 MapObjects에도 넣어 놓고 해당 맵으로 돌아올 때 isClear가 true라면 Bridge 오브젝트를 활성화
    }

    public IEnumerator OnLeftMap()
    {
        yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeOut(1f));
        UpdateMapData(mapData.left_map);
        PlayerController.Instance.transform.position = mapData.player_position_right;
        yield return new WaitForSeconds(0.5f);
        yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeIn(1f));
    }
    public IEnumerator OnRightMap()
    {
        yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeOut(1f));
        UpdateMapData(mapData.right_map);
        PlayerController.Instance.transform.position = mapData.player_position_left;
        yield return new WaitForSeconds(0.5f);
        yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeIn(1f));
    }
    public IEnumerator OnUpMap()
    {
        yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeOut(1f));
        UpdateMapData(mapData.up_map);
        PlayerController.Instance.transform.position = mapData.player_position_down;
        yield return new WaitForSeconds(0.5f);
        yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeIn(1f));
    }
    public IEnumerator OnDownMap()
    {
        yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeOut(1f));
        UpdateMapData(mapData.down_map);
        PlayerController.Instance.transform.position = mapData.player_position_up;
        yield return new WaitForSeconds(0.5f);
        yield return fadeManager.fadeCoroutine = StartCoroutine(fadeManager.FadeIn(1f));
    }
}
