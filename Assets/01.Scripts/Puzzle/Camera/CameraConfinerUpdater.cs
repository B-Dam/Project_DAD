using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraConfinerUpdater : MonoBehaviour
{
    public CinemachineConfiner2D confiner; // 메인 카메라의 Confiner2D
    public void UpdateConfinerFor(Transform mapTransform)
    {
        string mapID = mapTransform.name;
        string path = $"Cameras/MapCollider/{mapID}_Collider";

        var colliderObj = GameObject.Find(path);
        if (colliderObj == null)
        {
            Debug.LogWarning($" Collider 오브젝트를 찾을 수 없습니다: {path}");
            return;
        }

        var poly = colliderObj.GetComponent<PolygonCollider2D>();
        if (poly == null)
        {
            Debug.LogWarning(" PolygonCollider2D를 찾을 수 없습니다. 대상: " + mapTransform.name);
            return;
        }

        confiner.BoundingShape2D = null;
        confiner.InvalidateBoundingShapeCache();

        confiner.BoundingShape2D = poly;
        confiner.InvalidateBoundingShapeCache();

        Debug.Log(" Confiner 경계 업데이트 완료");
    }
    //public Transform player; // 플레이어 (혹은 맵 트리거 기준)

    //// 외부에서 호출 시 StartCoroutine으로 감싸기
    //public void SetConfinerToNewMap(GameObject newMap)
    //{
    //    string mapName = MapManager.Instance.currentMapID;
    //    //GameObject mapObj = GameObject.Find("Cameras/MapCollider/"+mapName + "_Collider");
    //    string path = "Cameras/MapCollider/" + mapName + "_Collider";
    //    GameObject mapObj = GameObject.Find(path);
    //    Debug.Log("찾는 경로: " + path + " / 결과: " + (mapObj == null ? "못 찾음" : mapObj.name));

    //    if (mapObj == null)
    //    {
    //        Debug.LogError($" 해당 이름의 맵 오브젝트를 찾을 수 없습니다: {mapName}_Collider");
    //        return;
    //    }

    //  PolygonCollider2D poly = mapObj.GetComponentInChildren<PolygonCollider2D>();
    //    if (poly == null)
    //    {
    //        Debug.LogError(" PolygonCollider2D를 찾을 수 없습니다.");
    //        return;
    //    }

    //    // Rigidbody2D 없이 등록!
    //    confiner.BoundingShape2D = null;
    //    confiner.InvalidateBoundingShapeCache();

    //    confiner.BoundingShape2D = poly;
    //    confiner.InvalidateBoundingShapeCache();

    //    Debug.Log(" PolygonCollider2D 등록 완료 (Rigidbody 없이 시도함)");
    //}

}

