using UnityEngine;
using UnityEngine.Rendering;

public class V2ColliderBase : MonoBehaviour
{
    protected string PlayerTag = "Player";

    protected void V2MapMove(string warpDir)
    {
        Debug.Log($"V2MapMove 호출됨: {warpDir}");
        switch (warpDir)
        {
            // currentMapID 바꾸기
            // 좌표값 바꿔주기

            // 참조를 너무 자주 하니까 웬만하면 참조해야할 스크립트에서 함수로 구현 해두고 한번 씩만 참조 하도록
            case "Left":
                StartCoroutine(MapManager.Instance.OnLeftMap());
                break;
            case "Right":
                StartCoroutine(MapManager.Instance.OnRightMap());
                break;
            case "Up":
                StartCoroutine(MapManager.Instance.OnUpMap());
                break;
            case "Down":
                StartCoroutine(MapManager.Instance.OnDownMap());
                break;
            default:
                Debug.LogWarning($"올바르지 않은 콜라이더 방향입니다: {warpDir}");
                break;
        }
    }
}
