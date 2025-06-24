using System;
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
                _instance = new GameObject() { name = "MapManager" }.AddComponent<MapManager>();

            return _instance;
        }
    }

    // 각 맵에 map_id와 next, prev_map_id를 부여
    // 플레이어가 영역 중앙에 들어오면 맵 이동

    private GameObject[] _mapPrefabs;
    private Dictionary<int, GameObject> _mapID;

    private void Start()
    {
        // MapManager 오브젝트 활성화 됐을 때 맵 오브젝트에 map_id 부여
    }

    public void MapUpdate()
    {
        
    }

}
