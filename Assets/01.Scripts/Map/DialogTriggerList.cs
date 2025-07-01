using UnityEngine;

public class DialogTriggerList : MonoBehaviour
{
    public GameObject triggerObj;
    public string mapTriggerPrefabPath = $"MapTrigger/{MapManager.Instance.GetMapName()}";

    private void Awake()
    {
        TriggerObjCheck();

        TriggerReset();
        TriggerLoad();
    }

    private void TriggerObjCheck()
    {
        if (triggerObj == null)
        {
            Debug.Log("triggerObj가 비어있습니다");
        }
    }

    private void TriggerReset()
    {
        int TriggerCount = triggerObj.transform.childCount;
        for (int i = TriggerCount -1; i >= 0; i--)
        {
            Transform child = triggerObj.transform.GetChild(i);
            
            DestroyImmediate(child);
            Debug.Log($"맵 트리거 제거됨 : {child.name}");
        }
    }

	private void TriggerLoad()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>(mapTriggerPrefabPath);

        if (loadedPrefabs == null || loadedPrefabs.Length == 0)
        {
            Debug.LogWarning($"{mapTriggerPrefabPath} 경로에 대사 프리펩이 존재하지 않습니다");
            return;
        }

        // 대사 트리거 소환용 foreach
        foreach (GameObject prefab in loadedPrefabs)
        {
            GameObject obj = Instantiate(prefab);
        }
    }









	public void TriggerSetting()
	{
        /// <summary>
        /// 1. MapTrigger 안에 맵아이디_번호 형식으로 콜라이더 넣어 만들어 두기
        /// 2. 특정 상황에서 임의의 대사 트리거(콜라이더)를 온오프 해야함
        /// </summary>



		switch (MapManager.Instance.GetMapName())
		{
			case "001":
                
				break;
			case "002":
				break;
            case "101":
                break;
            case "102":
                break;
            case "103":
                break;
            case "104":
                break;
            case "105":
                break;
            case "106":
                break;
            case "107":
                break;
            case "108":
                break;
            case "109":
                break;
            default:
                Debug.Log($"현재 맵은 {MapManager.Instance.GetMapName()}입니다");
                break;
        }
	}
	
	
}
