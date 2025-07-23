using UnityEngine;
using UnityEngine.UI;

public class SpecialAbilityUI : MonoBehaviour
{
    [SerializeField] private GameObject specialPanel; // 카드 3장 선택 창
    
    public static SpecialAbilityUI Instance { get; private set; }
    
    void Awake() => Instance = this;
    
    void Update()
    {
        // ESC 키로 언제든 패널 닫기
        if (specialPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            specialPanel.SetActive(false);
    }

    public void ShowSpecialPanel()
    {
        specialPanel.SetActive(true);
    }
}