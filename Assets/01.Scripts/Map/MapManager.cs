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

    // �� �ʿ� map_id�� next, prev_map_id�� �ο�
    // �÷��̾ ���� �߾ӿ� ������ �� �̵�

    private GameObject[] _mapPrefabs;
    private Dictionary<int, GameObject> _mapID;

    private void Start()
    {
        // MapManager ������Ʈ Ȱ��ȭ ���� �� �� ������Ʈ�� map_id �ο�
    }

    public void MapUpdate()
    {
        
    }

}
