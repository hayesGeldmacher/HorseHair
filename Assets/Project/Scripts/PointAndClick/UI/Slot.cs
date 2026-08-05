using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemText;
    [SerializeField] private CanvasGroup _group;

    private void Start()
    {
        ClearSlot();
    }

    public void SetItem(Sprite itemSprite, string itemName)
    {
        _group.alpha = 1;
        _itemImage.sprite = itemSprite;
        _itemText.text = itemName;
    }

    public void ClearSlot()
    {
        _group.alpha = 0;
        _itemImage.sprite = null;
        _itemText.text = string.Empty;
    }   

    public string GetName()
    {
        return _itemText.text;  
    }
}
