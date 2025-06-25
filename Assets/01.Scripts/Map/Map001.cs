using UnityEngine;

public class Map001 : MapBase
{
    private void Awake()
    {
        MapData = Database.Instance.Map.GetMapData("001");
    }

    protected override void OnLoadMap()
	{
		// 맵을 호출할 때 작동하는 로직

		



		Debug.Log("Map001이 로드되었습니다");
    }

    protected override void OnReleaseMap()
    {
        Debug.Log("Map001맵에서 나갔습니다");
    }
}
