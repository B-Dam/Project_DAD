using UnityEngine;

public class BoomTrigger : MonoBehaviour
{
    [Header("폭발 시 UI")]
    public GameObject resetUI; // 리셋 버튼 포함한 UI 패널

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")||other.CompareTag("Box"))
        {
            Time.timeScale = 0f;
            Debug.Log("Boom!");

            if (resetUI != null)
                resetUI.SetActive(true);
            else
                Debug.LogWarning("[BoomTrigger] resetUI가 할당되지 않았습니다.");
        }
    }
}
