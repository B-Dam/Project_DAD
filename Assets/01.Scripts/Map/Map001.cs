using UnityEngine;

public class Map001 : MapBase
{
    private void Awake()
    {
        MapData = Database.Instance.Map.GetMapData("001");
    }

    protected override void OnLoadMap()
	{
		// ���� ȣ���� �� �۵��ϴ� ����

		



		Debug.Log("Map001�� �ε�Ǿ����ϴ�");
    }

    protected override void OnReleaseMap()
    {
        Debug.Log("Map001�ʿ��� �������ϴ�");
    }
}
