using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public GameObject testClearPanel;
    public Collider2D goalArea;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Box") && IsFullyInside(goalArea, other))
        {
            Debug.Log(" 박스가 골을 완전히 덮음 → 클리어!");
            testClearPanel.SetActive(true);
            //Time.timeScale = 0f;
        }
    }
    bool IsFullyInside(Collider2D inner, Collider2D outer)
    {
        return outer.bounds.Contains(inner.bounds.min) &&
               outer.bounds.Contains(inner.bounds.max);
    }
}
