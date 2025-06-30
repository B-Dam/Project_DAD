using UnityEngine;
using UnityEngine.UI;

public class SettingMenuController : MonoBehaviour
{
    public enum MenuType { Sound, Save, Load }

    [Header("메뉴 버튼")]
    [SerializeField] private GameObject soundButton;
    [SerializeField] private GameObject saveButton;
    [SerializeField] private GameObject loadButton;

    [Header("내용 패널")]
    [SerializeField] private GameObject soundContent;
    [SerializeField] private GameObject saveContent;
    [SerializeField] private GameObject loadContent;

    private void Start()
    {
        // 시작할 때 기본 메뉴 선택
        ShowMenu(MenuType.Sound);
    }

    // 버튼 OnClick 으로 연결
    public void OnClickSound() => ShowMenu(MenuType.Sound);
    public void OnClickSave()  => ShowMenu(MenuType.Save);
    public void OnClickLoad()  => ShowMenu(MenuType.Load);

    private void ShowMenu(MenuType type)
    {
        // 오른쪽 콘텐츠 활성/비활성
        soundContent.SetActive(type == MenuType.Sound);
        saveContent.SetActive(type == MenuType.Save);
        loadContent.SetActive(type == MenuType.Load);
    }
}