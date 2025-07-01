using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class MapBase : MonoBehaviour
{
	/// <summary>
	/// 맵 스크립트가 공통적으로 처리해야 하는 것들
	/// 
	/// 1. 현재 맵 정보 로드
	/// 2. 현재 맵의 워프 콜라이더 찾기
    /// 3. 콜라이더의 이벤트 처리
	/// </summary>
	
    // 맵 관련
	protected MapData mapData;
	protected string prevMapID;
    protected Dictionary<string, Vector2> spawnPoint;
    private string _currentMapID;

    // 임시 플레이어 포지션 변수
    protected Vector2 _playerPosition;


    protected virtual void Awake()
	{
        _currentMapID = MapManager.Instance.GetMapName();
        mapData = Database.Instance.Map.GetMapData(_currentMapID);

        SpawnPointSet();

        Debug.Log($"현재 맵은 {_currentMapID} 입니다.");
    }

    protected abstract void OnLoadMap();  // 맵을 호출할 때 작동하는 로직

    public abstract void OnReleaseMap();  // 맵이 나갈 때 작동하는 로직

    protected void SpawnPointSet()
    {
        spawnPoint = new Dictionary<string, Vector2>();
        if (mapData.left_map != "null")
            spawnPoint.Add(mapData.left_map, mapData.player_position_left);
        if (mapData.right_map != "null")
            spawnPoint.Add(mapData.right_map, mapData.player_position_right);
        if (mapData.up_map != "null")
            spawnPoint.Add(mapData.up_map, mapData.player_position_up);
        if (mapData.down_map != "null")
            spawnPoint.Add(mapData.down_map, mapData.player_position_down);
    }

    /// <summary>
    /// 1. 딕셔너리 prevMapID, player_position 묶어두기
    /// 2. 맵을 넘어갈 때 기존에 있던 맵을 prevMapID로 넘기기
    /// 3. 맵을 넘어가면 OnLoadMap 함수에서 prevMapID 기준 플레이어가 넘어간 방향에 있는 맵의 스폰 포인트로 이동하기
    /// </summary>>

}