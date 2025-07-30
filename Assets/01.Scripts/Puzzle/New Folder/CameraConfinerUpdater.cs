using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraConfinerUpdater : MonoBehaviour
{
    public CinemachineConfiner2D confiner; // 메인 카메라의 Confiner2D
    public Transform player; // 플레이어 (혹은 맵 트리거 기준)

    // 외부에서 호출 시 StartCoroutine으로 감싸기
    public void SetConfinerToNewMap(GameObject newMap)
    {
        string mapName = MapManager.Instance.currentMapID;
        //GameObject mapObj = GameObject.Find("Cameras/MapCollider/"+mapName + "_Collider");
        string path = "Cameras/MapCollider/" + mapName + "_Collider";
        GameObject mapObj = GameObject.Find(path);
        Debug.Log("찾는 경로: " + path + " / 결과: " + (mapObj == null ? "못 찾음" : mapObj.name));

        if (mapObj == null)
        {
            Debug.LogError($"❌ 해당 이름의 맵 오브젝트를 찾을 수 없습니다: {mapName}_Collider");
            return;
        }

      PolygonCollider2D poly = mapObj.GetComponentInChildren<PolygonCollider2D>();
        if (poly == null)
        {
            Debug.LogError("❌ PolygonCollider2D를 찾을 수 없습니다.");
            return;
        }

        // Rigidbody2D 없이 등록!
        confiner.BoundingShape2D = null;
        confiner.InvalidateBoundingShapeCache();

        confiner.BoundingShape2D = poly;
        confiner.InvalidateBoundingShapeCache();

        Debug.Log("📷 PolygonCollider2D 등록 완료 (Rigidbody 없이 시도함)");
    }

    //private  DelayedSetConfiner(GameObject newMap)
    //{
    //    // 한 프레임 대기 (맵 프리팹이 완전히 Hierarchy에 추가되도록)
    //    yield return null;

    //    string mapName = $"{MapManager.Instance.currentMapID}";
    //    GameObject mapObj = GameObject.Find(mapName);

    //    if (mapObj == null)
    //    {
    //        Debug.LogError($"❌ 해당 이름의 맵 오브젝트를 찾을 수 없습니다: {mapName}");
    //        yield break;
    //    }

    //    CompositeCollider2D newComposite = mapObj.GetComponentInChildren<CompositeCollider2D>();
    //    if (newComposite == null)
    //    {
    //        Debug.LogWarning("⚠️ 해당 맵에 Composite Collider 2D가 없습니다!");
    //        yield break;
    //    }

    //    confiner.BoundingShape2D = newComposite;
    //    confiner.InvalidateBoundingShapeCache();

    //    Debug.Log("📷 카메라 경계 갱신 완료: " + newComposite.gameObject.name);

        //confiner.BoundingShape2D = newComposite;
        //confiner.InvalidateBoundingShapeCache();
        //Debug.Log("BoundingShape2D 할당됨: " + newComposite.name);
        //Debug.Log("Confiner 현재 참조 중인 Collider: " + confiner.BoundingShape2D?.name);
        //Debug.Log("📷 카메라 경계 갱신 완료: " + newComposite.gameObject.name);
    }

