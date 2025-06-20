using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    public GameObject interactionText;     // UI 텍스트
    public GameObject targetBridge;        // 나타날 다리 등
    public GameObject targetWall; //없앨부분의 벽
    private bool isPlayerNear = false;

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("상호작용 발생!");
            targetBridge.SetActive(true);         // 다리 등장
            interactionText.SetActive(false);     // 텍스트 숨김
            targetWall.SetActive(false); // 벽 제거
            // Destroy(this); ← 한 번만 작동하게 하려면 이것도 가능
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            interactionText.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            interactionText.SetActive(false);
        }
    }
}
