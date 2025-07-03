using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class MapBase : MonoBehaviour
{
    /// <summary>
    /// 맵 스크립트가 공통적으로 처리해야 하는 것들
    /// 1. 현재 맵 정보 로드
    /// 2. 현재 맵의 워프 콜라이더 찾기
    /// 3. 콜라이더의 이벤트 처리
    /// </summary>

    // 맵 관련
    protected MapData mapData;
    protected Dictionary<string, Vector3> spawnPoint;
    private string _currentMapID;

    protected virtual void Awake()
    {
        _currentMapID = MapManager.Instance.GetMapName();
        mapData = Database.Instance.Map.GetMapData(_currentMapID);

        SpawnPointSet();

        Debug.Log($"현재 맵은 {_currentMapID} 입니다.");
    }

    protected abstract void OnLoadMap();  // 맵을 호출할 때 작동하는 로직

    public virtual void OnReleaseMap()  // 맵이 나갈 때 작동하는 로직
    {
        // 기존에 있던 맵을 이전 맵으로 만들기
        MapManager.Instance.prevMapID = MapManager.Instance.currentMapID;
        MapManager.Instance.lastPlayerScale = PlayerController.Instance.playerTransform.localScale;


        // 맵을 나갈 때 트리거에 닿은 직후의 스케일 값을 저장해두기

    }

    protected void SpawnPointSet()
    {
        spawnPoint = new Dictionary<string, Vector3>();
        if (!string.IsNullOrEmpty(mapData.left_map))
            spawnPoint.Add(mapData.left_map, mapData.player_position_left);
        if (!string.IsNullOrEmpty(mapData.right_map))
            spawnPoint.Add(mapData.right_map, mapData.player_position_right);
        if (!string.IsNullOrEmpty(mapData.up_map))
            spawnPoint.Add(mapData.up_map, mapData.player_position_up);
        if (!string.IsNullOrEmpty(mapData.down_map))
            spawnPoint.Add(mapData.down_map, mapData.player_position_down);
    }

    /// <summary>
    /// 1. 딕셔너리 prevMapID, player_position 묶어두기
    /// 2. 맵을 넘어갈 때 기존에 있던 맵을 prevMapID로 넘기기
    /// 3. 맵을 넘어가면 OnLoadMap 함수에서 prevMapID 기준 플레이어가 넘어간 방향에 있는 맵의 스폰 포인트로 이동하기
    /// </summary>>

}