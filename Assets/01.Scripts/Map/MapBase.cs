using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapBase : MonoBehaviour
{
	/// <summary>
	/// 맵 스크립트가 공통적으로 처리해야 하는 것들
	/// 
	/// 1. 현재 맵 정보 로드
	/// 2. 
	/// </summary>
	
	protected MapData mapData;
	protected string prevMapID;
    private string _currentMapName;
	private GameObject _colliders;
    

    protected virtual void Awake()
	{
		_colliders = GameObject.Find("Colliders");
        _currentMapName = MapManager.Instance.GetMapName();

        mapData = Database.Instance.Map.GetMapData(_currentMapName);
		Debug.Log($"현재 맵은 {_currentMapName} 입니다.");

    }

    protected abstract void OnLoadMap();  // 맵을 호출할 때 작동하는 로직

    protected abstract void OnReleaseMap();  // 맵이 나갈 때 작동하는 로직


}


public abstract class ColliderBase : MonoBehaviour
{
    protected string PlayerTag = "Player";

    protected Action MoveToRightMap;
    protected Action MoveToLeftMap;
    protected Action MoveToUpMap;
    protected Action MoveToDownMap;
    protected Dictionary<Collider2D, Action> mapMoveColliders;

    private void Awake()
    {

        // 맵 이동 콜라이더 찾기
        GetColliderComponent();

        MoveToRightMap += TempRightMessage;
        MoveToLeftMap += TempLeftMessage;
        MoveToUpMap += TempUpMessage;
        MoveToDownMap += TempDownMessage;
    }
    private void GetColliderComponent()
    {
        mapMoveColliders = new Dictionary<Collider2D, Action>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Collider2D childCollider = child.GetComponent<Collider2D>();

            switch (child.name)
            {
                case "Right":
                    mapMoveColliders.Add(childCollider, MoveToRightMap);
                    Debug.Log("Right collider added.");
                    break;
                case "Left":
                    mapMoveColliders.Add(childCollider, MoveToLeftMap);
                    Debug.Log("Left collider added.");
                    break;
                case "Up":
                    mapMoveColliders.Add(childCollider, MoveToUpMap);
                    Debug.Log("Up collider added.");
                    break;
                case "Down":
                    mapMoveColliders.Add(childCollider, MoveToDownMap);
                    Debug.Log("Down collider added.");
                    break;
            }
        }
    }

    protected abstract void OnTriggerEnter2D(Collider2D collision);

    private void TempRightMessage()
    {
        Debug.Log("오른쪽 맵으로 이동합니다.");
    }

    protected void TempLeftMessage()
    {
        Debug.Log("왼쪽 맵으로 이동합니다.");
    }

    protected void TempUpMessage()
    {
        Debug.Log("위쪽 맵으로 이동합니다.");
    }

    protected void TempDownMessage()
    {
        Debug.Log("아래쪽 맵으로 이동합니다.");
    }

}