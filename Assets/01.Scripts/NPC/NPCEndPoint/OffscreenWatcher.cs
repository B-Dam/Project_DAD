using System;
using System.Collections.Generic;
using UnityEngine;

public class OffscreenWatcher : MonoBehaviour
{
    private List<Transform> targets = new List<Transform>();
    private Action onAllRemovedCallback;

    void Update()
    {
        if (Camera.main == null) return;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            Transform obj = targets[i];
            if (obj == null)
            {
                targets.RemoveAt(i);
                continue;
            }

            Vector3 vp = Camera.main.WorldToViewportPoint(obj.position);

            bool isOutOfView =
         vp.z < 0 || // 뒤쪽에 있음
         vp.x < 0 || vp.x > 1 ||
         vp.y < 0 || vp.y > 1;

            if (isOutOfView)
            {
                Debug.Log($"❌ {obj.name} 카메라 밖 → 제거");
                //Destroy(obj.gameObject);
                obj.gameObject.SetActive(false); // 오브젝트 비활성화
                targets.RemoveAt(i);
            }
        }
        // 조건 만족 시 콜백 실행
        if (onAllRemovedCallback != null && targets.Count == 0)
        {
            Debug.Log("🎯 모든 오브젝트 제거됨 → 콜백 실행");
            onAllRemovedCallback.Invoke();
            onAllRemovedCallback = null; // 한 번만 실행
        }
    }

    // 외부에서 오브젝트 추가해주는 함수
    public void Register(Transform target)
    {
        if (!targets.Contains(target) && target != null)
            targets.Add(target);
    }
    // 외부에서 콜백 등록
    public void WatchUntilAllRemoved(Action callback)
    {
        if (targets.Count == 0)
        {
            callback?.Invoke();//  감시할 대상이 이미 없으면 바로 실행
            return;
        }

        onAllRemovedCallback = callback;//  아직 감시할 대상이 남아있으면 저장해둠
    }
}
