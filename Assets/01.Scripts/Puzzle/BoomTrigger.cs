using UnityEngine;

public class BoomTrigger : MonoBehaviour
{
    [Header("폭발 시 UI")]
    public Canvas canvas; // 리셋 버튼 포함한 UI 패널
    public GameObject boomEffectPrefab; // 폭탄 효과로 띄울 이미지 프리팹

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")||other.CompareTag("Box"))
        {
            Time.timeScale = 0f;
            Debug.Log("Boom!");
            AudioManager.Instance.PlaySFX("TouchTrap");

            if (canvas == null)
            {
                //canvas = FindFirstObjectByType<Canvas>();
                canvas = GameObject.Find("UICanvas")?.GetComponent<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError(" Canvas를 찾을 수 없습니다!");
                    return;
                }
            }

            // 폭발 이미지 띄우기
            if (boomEffectPrefab != null)
            {
                Camera cam = Camera.main;
                if (cam == null)
                {
                    Debug.LogWarning("[BoomTrigger] 카메라 없음");
                    return;
                }

                // 1. 폭탄 위치를 스크린 좌표로 변환
                Vector2 screenPos = cam.WorldToScreenPoint(transform.position);

                // 2. 프리팹을 캔버스 하위에 생성
                GameObject effect = Instantiate(boomEffectPrefab, canvas.transform);

                // 3. UI 위치 설정
                effect.GetComponent<RectTransform>().position = screenPos;
            }
        }
    }
}
