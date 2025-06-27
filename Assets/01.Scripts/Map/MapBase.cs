using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class MapBase : MonoBehaviour
{
	/// <summary>
	/// �� ��ũ��Ʈ�� ���������� ó���ؾ� �ϴ� �͵�
	/// 
	/// 1. ���� �� ���� �ε�
	/// 2. ���� ���� ���� �ݶ��̴� ã��
    /// 3. �ݶ��̴��� �̺�Ʈ ó��
	/// </summary>
	
    // �� ����
	protected MapData mapData;
	protected string prevMapID;
    private string _currentMapID;
	private GameObject _colliders;

    // �ӽ� �÷��̾� ������ ����
    protected Vector2 _playerPosition;


    protected virtual void Awake()
	{
		_colliders = GameObject.Find("Colliders");
        _currentMapID = MapManager.Instance.GetMapName();

        mapData = Database.Instance.Map.GetMapData(_currentMapID);
		Debug.Log($"���� ���� {_currentMapID} �Դϴ�.");

    }

    protected abstract void OnLoadMap();  // ���� ȣ���� �� �۵��ϴ� ����

    public abstract void OnReleaseMap();  // ���� ���� �� �۵��ϴ� ����
}

public abstract class ColliderBase : MonoBehaviour
{
    private MapData mapData;
    private string _currentMapID;

    // �� ���� ����
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

        // �� �̵� �ݶ��̴� ã��
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
    /// �� �ε� �Լ��� �Լ��� (Action MoveTo...Map�� ��ϵ�)
    /// </summary>

    public void OnRightMap() //4
    {
        Debug.Log("������ ������ �̵��մϴ�.");
        MapManager.Instance.LoadMap(mapData.right_map);
    }

    public void OnLeftMap()
    {
        Debug.Log("���� ������ �̵��մϴ�.");
        MapManager.Instance.LoadMap(mapData.left_map);
    }

    public void OnUpMap()
    {
        Debug.Log("���� ������ �̵��մϴ�.");
        MapManager.Instance.LoadMap(mapData.up_map);
    }

    public void OnDownMap()
    {
        Debug.Log("�Ʒ��� ������ �̵��մϴ�.");
        MapManager.Instance.LoadMap(mapData.down_map);
    }

}