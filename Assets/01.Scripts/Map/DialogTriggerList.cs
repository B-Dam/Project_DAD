using UnityEngine;

public class DialogTriggerList : MonoBehaviour
{
    public GameObject triggerListPrefab;

    private void Awake()
    {
        triggerListPrefab = GameObject.Find("Colliders").transform.Find("MapTrigger").gameObject;
    }

	
	public void TriggerSetting()
	{
        /// <summary>
        /// 1. 특정 상황에서 임의의 대사 트리거(콜라이더)를 온오프 해야함
        /// 2. 트리거에 닿았을 때 대사 띄우기
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
