using UnityEngine;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<MapManager>();

            if (_instance == null)
                _instance = new GameObject() { name = "MapManager" }.AddComponent<MapManager>();

            return _instance;
        }
    }


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

    // �� �ʿ� map_id�� next, prev_map_id�� �ο�
    // �÷��̾ ���� �߾ӿ� ������ �� �̵�


    private void Start()
    {

    }

    public void MapUpdate()
    {
        // �� �ε�� �� ���� �� ���� ���ΰ�ħ
        Database.Instance.Map.GetMapData(GameManager.Instance.currentMapName);

    }

    public void GetMapID()
    {
        
    }

}
