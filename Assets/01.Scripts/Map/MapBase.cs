using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapBase : MonoBehaviour
{
	/// <summary>
	/// �� ��ũ��Ʈ�� ���������� ó���ؾ� �ϴ� �͵�
	/// 
	/// 1. ���� �� ���� �ε�
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
		Debug.Log($"���� ���� {_currentMapName} �Դϴ�.");

    }

    protected abstract void OnLoadMap();  // ���� ȣ���� �� �۵��ϴ� ����

    protected abstract void OnReleaseMap();  // ���� ���� �� �۵��ϴ� ����


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

        // �� �̵� �ݶ��̴� ã��
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
        Debug.Log("������ ������ �̵��մϴ�.");
    }

    protected void TempLeftMessage()
    {
        Debug.Log("���� ������ �̵��մϴ�.");
    }

    protected void TempUpMessage()
    {
        Debug.Log("���� ������ �̵��մϴ�.");
    }

    protected void TempDownMessage()
    {
        Debug.Log("�Ʒ��� ������ �̵��մϴ�.");
    }

}