using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ColliderBase : MonoBehaviour
{
    protected MapData mapData;
    private string _currentMapID;

    // 맵 워프 관련
    protected string PlayerTag = "Player";

    protected virtual void Awake()
    {
        _currentMapID = MapManager.Instance.GetMapName();
        mapData = Database.Instance.Map.GetMapData(_currentMapID);
    }

    protected abstract void OnTriggerEnter2D(Collider2D collision);

    /// <summary>
    /// 씬 로드 함수들어갈 함수들 (Action MoveTo...Map에 등록됨)
    /// </summary>

    protected void MapMove(string warpDir)
    {
        switch (warpDir)
        {
            case "Right":
                OnRightMap();
                break;
            case "Left":
                OnLeftMap();
                break;
            case "Up":
                OnUpMap();
                break;
            case "Down":
                OnDownMap();
                break;
            default:
                break;
        }
    }

    public void OnRightMap()
    {
        MapManager.Instance.LoadMap(mapData.right_map);
    }

    public void OnLeftMap()
    {
        MapManager.Instance.LoadMap(mapData.left_map);
    }

    public void OnUpMap()
    {
        MapManager.Instance.LoadMap(mapData.up_map);
    }

    public void OnDownMap()
    {
        MapManager.Instance.LoadMap(mapData.down_map);
    }

}