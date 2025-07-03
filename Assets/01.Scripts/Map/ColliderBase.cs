using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ColliderBase : MonoBehaviour
{
    protected MapData mapData;
    private string _currentMapID;

    protected string PlayerTag = "Player";

    protected virtual void Awake()
    {
        _currentMapID = MapManager.Instance.GetMapName();
        mapData = Database.Instance.Map.GetMapData(_currentMapID);
    }

    protected abstract void OnTriggerEnter2D(Collider2D collision);

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

    // SceneLoad가 아니라 좌표 이동으로 변경될 예정
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