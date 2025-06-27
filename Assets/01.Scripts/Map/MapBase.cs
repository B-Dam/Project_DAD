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
    private string _currentMapID;
	private GameObject _colliders;

    // 임시 플레이어 포지션 변수
    protected Vector2 _playerPosition;


    protected virtual void Awake()
	{
		_colliders = GameObject.Find("Colliders");
        _currentMapID = MapManager.Instance.GetMapName();

        mapData = Database.Instance.Map.GetMapData(_currentMapID);
		Debug.Log($"현재 맵은 {_currentMapID} 입니다.");

    }

    protected abstract void OnLoadMap();  // 맵을 호출할 때 작동하는 로직

    public abstract void OnReleaseMap();  // 맵이 나갈 때 작동하는 로직
}