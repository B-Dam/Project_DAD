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

public abstract class ColliderBase : MonoBehaviour
{
    private MapData mapData;
    private string _currentMapID;

    // 맵 워프 관련
    protected string PlayerTag = "Player";
    protected Action MoveToRightMap;  //2
    protected Action MoveToLeftMap;
    protected Action MoveToUpMap;
    protected Action MoveToDownMap;
    protected Dictionary<string, Action> warpColliders;

    protected virtual void Awake()
    {
        _currentMapID = MapManager.Instance.GetMapName();
        mapData = Database.Instance.Map.GetMapData(_currentMapID);

        // 맵 이동 콜라이더 찾기
        GetColliderComponent();

        MoveToRightMap += OnRightMap;  //3
        MoveToLeftMap += OnLeftMap;
        MoveToUpMap += OnUpMap;
        MoveToDownMap += OnDownMap;
    }

    protected abstract void OnTriggerEnter2D(Collider2D collision);

    private void GetColliderComponent()
    {
        warpColliders = new Dictionary<string, Action>();

        GameObject obj = GameObject.Find("Colliders");

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            Transform child = obj.transform.GetChild(i);
            Collider2D childCollider = child.GetComponent<Collider2D>();

            switch (child.name)
            {
                case "Right":
                    warpColliders.Add(childCollider.name, MoveToRightMap);
                    Debug.Log("Right collider added.");
                    break;
                case "Left":
                    warpColliders.Add(childCollider.name, MoveToLeftMap);
                    Debug.Log("Left collider added.");
                    break;
                case "Up":
                    warpColliders.Add(childCollider.name, MoveToUpMap);
                    Debug.Log("Up collider added.");
                    break;
                case "Down":
                    warpColliders.Add(childCollider.name, MoveToDownMap);
                    Debug.Log("Down collider added.");
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 씬 로드 함수들어갈 함수들 (Action MoveTo...Map에 등록됨)
    /// </summary>

    public void OnRightMap() //4
    {
        Debug.Log("오른쪽 맵으로 이동합니다.");
        MapManager.Instance.LoadMap(mapData.right_map);
    }

    public void OnLeftMap()
    {
        Debug.Log("왼쪽 맵으로 이동합니다.");
        MapManager.Instance.LoadMap(mapData.left_map);
    }

    public void OnUpMap()
    {
        Debug.Log("위쪽 맵으로 이동합니다.");
        MapManager.Instance.LoadMap(mapData.up_map);
    }

    public void OnDownMap()
    {
        Debug.Log("아래쪽 맵으로 이동합니다.");
        MapManager.Instance.LoadMap(mapData.down_map);
    }

}