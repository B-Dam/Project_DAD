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
		// 맵을 들어갈 때 작동하는 로직

		Debug.Log("Map001이 로드되었습니다");
    }

    protected override void OnReleaseMap()
    {
        // 맵을 나갈 때 작동하는 로직

        Debug.Log("Map001맵에서 나갔습니다");
    }
}
