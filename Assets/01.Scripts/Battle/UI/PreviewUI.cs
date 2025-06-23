using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreviewUI : MonoBehaviour
{
    public static PreviewUI Instance { get; private set; }

    [SerializeField] Image iconImage;
    [SerializeField] TextMeshProUGUI nameText;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(CardData skill)
    {
        iconImage.sprite = skill.icon;
        nameText.text    = skill.displayName;
        gameObject.SetActive(true);
    }
}