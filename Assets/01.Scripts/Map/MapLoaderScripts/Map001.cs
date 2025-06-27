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
        if (prevMapID == null)
        {
            _playerPosition = mapData.player_position_left;
        }
        else
        {
           _playerPosition = mapData.player_position_right;
        }
    }

    public override void OnReleaseMap()
    {
        // ���� ���� �� �۵��ϴ� ����
        
        prevMapID = MapManager.Instance.currentMapID;
        Debug.Log("Map001�ʿ��� �������ϴ�");
    }
}
