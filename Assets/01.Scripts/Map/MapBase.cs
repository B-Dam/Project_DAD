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
	protected string prevMapID;
    private string _currentMapName;
	private GameObject _colliders;



    protected virtual void Awake()
	{
		_colliders = GameObject.Find("Colliders");
        _currentMapName = MapManager.Instance.GetMapName();

        mapData = Database.Instance.Map.GetMapData(_currentMapName);
		Debug.Log($"현재 맵은 {_currentMapName} 입니다.");

        // 맵 이동 콜라이더 찾기

    }

    protected abstract void OnLoadMap();  // 맵을 호출할 때 작동하는 로직

    protected abstract void OnReleaseMap();  // 맵이 나갈 때 작동하는 로직


}
