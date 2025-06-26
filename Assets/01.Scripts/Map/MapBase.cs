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

        // �� �̵� �ݶ��̴� ã��

    }

    protected abstract void OnLoadMap();  // ���� ȣ���� �� �۵��ϴ� ����

    protected abstract void OnReleaseMap();  // ���� ���� �� �۵��ϴ� ����


}
