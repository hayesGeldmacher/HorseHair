using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemBox : MonoBehaviour
{
    [SerializeField] private TMP_Text itemText;
    [SerializeField] CanvasGroup fadeGroup;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        HideTextbox();
    }

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        _rectTransform.position = mousePos;
    }

    public void ShowTextbox(string text)
    {
        itemText.text = text;
        fadeGroup.alpha = 1;
    }

    public void HideTextbox()
    {
        fadeGroup.alpha = 0;
    }
}
