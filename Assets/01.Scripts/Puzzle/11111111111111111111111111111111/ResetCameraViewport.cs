using UnityEngine;

public class ResetCameraViewport : MonoBehaviour
{
    public Camera target;

    void Awake()
    {
        if (!target) target = Camera.main;
        if (!target) return;

        // 기본 뷰포트로 복구
        target.rect = new Rect(0f, 0f, 1f, 1f);

        // URP 추가 카메라라면 Base 카메라에서 실행하세요.
        // (필요시 여기에서 다른 카메라들도 순회하며 초기화 가능)
    }
}
