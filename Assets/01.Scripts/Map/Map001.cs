using UnityEngine;

public class Map001 : MapBase
{
    protected override void Awake()
    {
        base.Awake();
        OnLoadMap();
    }

    protected override void OnLoadMap()
	{
		// ���� �� �� �۵��ϴ� ����


        
    }

    protected override void OnReleaseMap()
    {
        // ���� ���� �� �۵��ϴ� ����
        
        prevMapID = MapManager.Instance.currentMapName;
        Debug.Log("Map001�ʿ��� �������ϴ�");
    }
}
