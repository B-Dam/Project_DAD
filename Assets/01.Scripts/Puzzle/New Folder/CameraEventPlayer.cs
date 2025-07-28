using System;
using System.Collections;
using UnityEngine;

public class CameraEventPlayer : MonoBehaviour
{
    public static CameraEventPlayer Instance;

    private Transform cam;
    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;
        cam = Camera.main.transform;
    }

    /// <summary>
    /// 카메라를 목표 위치로 부드럽게 이동한 후 작업을 수행하고, 원래대로 돌아옴
    /// </summary>
    /// <param name="target">카메라가 이동할 대상 위치</param>
    /// <param name="duration">이동 소요 시간</param>
    /// <param name="holdTime">머무는 시간</param>
    /// <param name="onReachedTarget">카메라 도착 후 실행할 작업 (예: 문 열기)</param>
    public void PlayCameraSequence(Transform target, float duration, float holdTime, Action onReachedTarget = null)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(CameraSequence(target, duration, holdTime, onReachedTarget));
    }

    private IEnumerator CameraSequence(Transform target, float duration, float holdTime, Action onReachedTarget)
    {
        Vector3 originalPos = cam.position;
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, originalPos.z);

        // 입력 잠금
        if (TestPlayerController.Instance != null)
        {
            TestPlayerController.Instance.SetVelocity(Vector2.zero); //  이동 정지
            TestPlayerController.Instance.enabled = false;            //  입력 차단
        }
        // 카메라 이동
        yield return StartCoroutine(MoveSmooth(cam, originalPos, targetPos, duration));

        // 도착 후 콜백 실행
        onReachedTarget?.Invoke();

        yield return new WaitForSeconds(holdTime);

        // 카메라 원래 위치로 복귀
        yield return StartCoroutine(MoveSmooth(cam, targetPos, originalPos, duration));

        // 입력 해제
        if (TestPlayerController.Instance != null)
        {
            TestPlayerController.Instance.SetVelocity(Vector2.zero); //  멈춰있는 상태 유지
            TestPlayerController.Instance.enabled = true;
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
