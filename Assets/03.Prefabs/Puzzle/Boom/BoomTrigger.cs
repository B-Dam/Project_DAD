using UnityEngine;

public class BoomTrigger : MonoBehaviour
{
    public GameObject resetUI; // 리셋 버튼 포함한 UI 패널

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Time.timeScale = 0f;
            Debug.Log("Boom!");
        }
    }
}
