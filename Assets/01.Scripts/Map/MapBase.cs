using UnityEngine;

public abstract class MapBase : MonoBehaviour
{
	public MapData MapData;

	private void Awake()
	{
		
    }

    protected abstract void OnLoadMap();  // 맵을 호출할 때 작동하는 로직

    protected abstract void OnReleaseMap();  // 맵이 나갈 때 작동하는 로직


}
