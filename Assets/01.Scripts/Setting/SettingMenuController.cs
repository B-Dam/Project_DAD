using UnityEngine;
using UnityEngine.UI;

public class SettingMenuController : MonoBehaviour
{
    public enum MenuType { Sound, Save, Load }

    [Header("메뉴 버튼")]
    [SerializeField] private Image soundButtonImage;
    [SerializeField] private Image saveButtonImage;
    [SerializeField] private Image loadButtonImage;
    
    [Header("버튼 색상")]
    [SerializeField] private Color normalColor   = Color.white;
    [SerializeField] private Color selectedColor = Color.green;

    [Header("내용 패널")]
    [SerializeField] private GameObject soundContent;
    [SerializeField] private GameObject saveContent;
    [SerializeField] private GameObject loadContent;
    
    [Header("세이브 로드 매니저")]
    [SerializeField] private SaveLoadManager saveLoadManager;
    
    [Header("설정창 전체 Panel")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        settingsPanel.SetActive(false);
        // 시작할 때 기본 메뉴 선택
        ShowMenu(MenuType.Sound);
    }
    
    private void Update()
    {
        // Esc 키를 누르면 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isOn = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isOn);
            if (!isOn) ShowMenu(MenuType.Sound); // 열 때 기본 탭 지정
        }
    }

    // 버튼 OnClick 으로 연결
    public void OnClickSound() => ShowMenu(MenuType.Sound);
    public void OnClickSave()
    {
        ShowMenu(MenuType.Save);
        saveLoadManager.SwitchMode(SaveLoadManager.Mode.Save);
    }

    public void OnClickLoad()
    {
        ShowMenu(MenuType.Load);
        saveLoadManager.SwitchMode(SaveLoadManager.Mode.Load);
    }

    private void ShowMenu(MenuType type)
    {
        // 오른쪽 콘텐츠 활성/비활성
        soundContent.SetActive(type == MenuType.Sound);
        saveContent.SetActive(type == MenuType.Save);
        loadContent.SetActive(type == MenuType.Load);
        
        // 버튼 색상 강조
        soundButtonImage.color = (type == MenuType.Sound) ? selectedColor : normalColor;
        saveButtonImage.color = (type == MenuType.Save) ? selectedColor : normalColor;
        loadButtonImage.color = (type == MenuType.Load) ? selectedColor : normalColor;
    }
}