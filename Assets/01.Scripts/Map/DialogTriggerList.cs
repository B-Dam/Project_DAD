using UnityEngine;

public class DialogTriggerList : MonoBehaviour
{
    public GameObject triggerObj;
    public string mapTriggerPrefabPath;

    private void Awake()
    {
        GameManager.Instance.Trigger = this;

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
                Debug.Log($"triggerObj에 {triggerObj.name}를 할당했습니다.\nMapTrigger 찾기를 시도합니다");
                triggerObj = GameObject.Find("Colliders").transform.Find("MapTrigger").gameObject;
                if (triggerObj.name == "MapTrigger")
                {
                    Debug.Log($"triggerObj에 MapTrigger를 할당했습니다. {triggerObj.name}");
                }
                else
                {
                    Debug.Log($"MapTrigger를 찾지 못했습니다. 현재 triggerObj 상태 : {triggerObj.name}");
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

    /// <summary>
    /// 1. 기본적으로 모든 대사 트리거 Off 해두기
    /// 2. 필요한 트리거를 On 하는 함수 만들기
    /// </summary>
	
	public void TriggerAllOff()
    {
        foreach (Transform child in triggerObj.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void TriggerOn(string triggerID)
    {
        GameObject targetTrigger = triggerObj.transform.Find(triggerID)?.gameObject;
        targetTrigger?.SetActive(true);
    }

    public void TriggerOff(string triggerID)
    {
        GameObject targetTrigger = triggerObj.transform.Find(triggerID)?.gameObject;
        targetTrigger?.SetActive(false);
    }
        /// <summary>
        /// 1. triggerObj의 자식 오브젝트를 모두 Off로 설정
        /// 2. triggerID와 일치하는 오브젝트를 찾아 On으로 설정
        /// </summary>
}
