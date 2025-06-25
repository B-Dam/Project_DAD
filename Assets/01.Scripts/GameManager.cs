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
		// �� �̸��� map_id�� ����ϱ� ���ٴ� MapObject�� ���� ������Ʈ�� �̸��� map_id�� ��� ��������

		currentMapName = GameObject.Find("MapObject")?.transform?.GetChild(0)?.name;
        Database.Instance.Map.GetMapData(currentMapName);
        MapManager.Instance.GetMapID();

		Debug.Log($"Current Map Name: {currentMapName}");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


}

