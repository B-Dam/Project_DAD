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
        if (prevMapID == null)  // ���� ����ż� ���� �������� �ٲ���ٸ� map.csv�� �����ϸ� ��
        {
            _playerPosition = mapData.player_position_left;
        }
        else
        {
           _playerPosition = mapData.player_position_right;
        }
    }

    protected override void OnReleaseMap()
    {
        // ���� ���� �� �۵��ϴ� ����
        
        prevMapID = MapManager.Instance.currentMapName;
        Debug.Log("Map001�ʿ��� �������ϴ�");
    }
}
