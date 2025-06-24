using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	
	
	private static GameManager _instance;
	public static GameManager Instance
	{
		get
		{
			if (_instance == null)
				_instance = FindAnyObjectByType<GameManager>();

			if (_instance == null)
				_instance = new GameObject("GameManager").AddComponent<GameManager>();
			return _instance;
		}
    }

	public string currentMapName;

    private void Awake()
    {
        if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else if (_instance != this)
		{
			Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// 씬 이름을 map_id로 사용하기 보다는 MapObject의 하위 오브젝트의 이름을 map_id로 지어서 가져오기

		currentMapName = GameObject.Find("MapObject")?.transform?.GetChild(0)?.name;
        MapManager.Instance.MapUpdate();
        MapManager.Instance.GetMapID();

    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


}

