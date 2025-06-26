using UnityEngine;
using UnityEngine.UI;

public class TogleActive : MonoBehaviour
{
    public GameObject target;        // 토글할 오브젝트
    public Button toggleButton;      // 연결할 버튼

    void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleActive);  // 버튼에 메서드 연결
        }
        else
        {
            Debug.LogWarning("[ToggleTarget] 버튼이 연결되지 않았습니다.");
        }
    }

    public void ToggleActive()
    {
        if (target != null)
        {
            bool nextState = !target.activeSelf;
            target.SetActive(nextState);
        }
    }
}
