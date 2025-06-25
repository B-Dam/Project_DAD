using UnityEngine;

public abstract class MapBase : MonoBehaviour
{
	/// <summary>
	/// 맵 스크립트가 공통적으로 처리해야 하는 것들
	/// 
	/// 1. 현재 맵 정보 로드
	/// 2. 
	/// </summary>
	
	protected MapData mapData;
    private string currentMapName;


    protected virtual void Awake()
	{
		currentMapName = gameObject.name;
        mapData = Database.Instance.Map.GetMapData(currentMapName);
    }

    protected abstract void OnLoadMap();  // 맵을 호출할 때 작동하는 로직

    protected abstract void OnReleaseMap();  // 맵이 나갈 때 작동하는 로직


}
