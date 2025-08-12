using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraEventPlayer : MonoBehaviour
{
    public static CameraEventPlayer Instance;

    [Header("시네머신 가상 카메라")]
    public CinemachineCamera virtualCam;

    [Header("기본 따라갈 대상 (보통 플레이어)")]
    public Transform defaultFollowTarget;

    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 카메라를 목표 위치로 부드럽게 이동한 후 작업을 수행하고, 원래대로 돌아옴
    /// </summary>
    public void PlayCameraSequence(Transform target, float duration, float holdTime, Action onReachedTarget = null)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(CameraSequence(target, duration, holdTime, onReachedTarget));
    }

    private IEnumerator CameraSequence(Transform target, float duration, float holdTime, Action onReachedTarget)
    {
        if (virtualCam == null || defaultFollowTarget == null)
        {
            //Debug.LogError("❌ virtualCam 또는 defaultFollowTarget이 할당되지 않았습니다!");
            yield break;
        }

        // 입력 잠금
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetVelocity(Vector2.zero);
            PlayerController.Instance.enabled = false;
        }

        // 1. Follow 타겟을 일시적으로 이벤트 위치로 전환
        GameObject tempTarget = new GameObject("TempCameraTarget");
        tempTarget.transform.position = virtualCam.transform.position; // 현재 위치에서 시작
        virtualCam.Follow = tempTarget.transform;

        // 2. 부드럽게 이동
        yield return StartCoroutine(MoveSmooth(tempTarget.transform, tempTarget.transform.position, target.position, duration));

        // 3. 도착 후 콜백
        onReachedTarget?.Invoke();

        yield return new WaitForSeconds(holdTime);

        // 4. 원래 위치로 되돌아오기
        yield return StartCoroutine(MoveSmooth(tempTarget.transform, tempTarget.transform.position, defaultFollowTarget.position, duration));

        // 5. Follow 다시 플레이어로
        virtualCam.Follow = defaultFollowTarget;
        Destroy(tempTarget);

        // 입력 해제
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetVelocity(Vector2.zero);
            PlayerController.Instance.enabled = true;
        }
    }

    private IEnumerator MoveSmooth(Transform cam, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            cam.position = Vector3.Lerp(from, to, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        cam.position = to;
    }
}
