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

		Debug.Log("Map001�� �ε�Ǿ����ϴ�");
    }

    protected override void OnReleaseMap()
    {
        // ���� ���� �� �۵��ϴ� ����

        Debug.Log("Map001�ʿ��� �������ϴ�");
    }
}
