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
    private string currentMapName;


    protected virtual void Awake()
	{
		currentMapName = gameObject.name;
        mapData = Database.Instance.Map.GetMapData(currentMapName);
    }

    protected abstract void OnLoadMap();  // ���� ȣ���� �� �۵��ϴ� ����

    protected abstract void OnReleaseMap();  // ���� ���� �� �۵��ϴ� ����


}
