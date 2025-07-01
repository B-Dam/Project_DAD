using UnityEngine;

public class DialogTriggerList : MonoBehaviour
{
    public GameObject triggerObj;
    public string mapTriggerPrefabPath;

    private void Awake()
    {
        TriggerObjCheck();

        mapTriggerPrefabPath = $"MapTrigger/{MapManager.Instance.GetMapName()}";
        TriggerReset();
        TriggerLoad();
    }

    private void TriggerObjCheck()
    {
        if (triggerObj == null)
        {
            Debug.Log("triggerObj가 비어있습니다. Colliders 찾기를 시도합니다");
            triggerObj = GameObject.Find("Colliders");
            if (triggerObj != null)
            {
                Debug.Log($"triggerObj가 존재합니다. {triggerObj.name}\nMapTrigger 찾기를 시도합니다");
                triggerObj = GameObject.Find("Colliders").transform.Find("MapTrigger").gameObject;
                if (triggerObj.name == "MapTrigger")
                {
                    Debug.Log($"triggerObj에 MapTrigger를 할당했습니다. {triggerObj.name}");
                }
                else
                {
                    Debug.Log($"MapTrigger를 찾지 못했습니다. {triggerObj.name}");
                }

            }
            else
            {
                Debug.Log("Colliders를 찾지 못했습니다.");
                return;
            }
            
        }
    }

    private void TriggerReset()
    {
        int TriggerCount = triggerObj.transform.childCount;
        for (int i = TriggerCount -1; i >= 0; i--)
        {
            Transform child = triggerObj.transform.GetChild(i);
            
            Debug.Log($"맵 트리거 제거됨 : {child.name}");
            DestroyImmediate(child.gameObject);
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
            obj.transform.SetParent(triggerObj.transform);
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
