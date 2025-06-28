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