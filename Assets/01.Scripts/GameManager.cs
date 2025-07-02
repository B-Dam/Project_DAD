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

	public DialogTriggerList Trigger;

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
    }

}

