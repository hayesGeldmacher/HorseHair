using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemText;

    private void Start()
    {
        ClearSlot();
    }

    public void SetItem(Sprite itemSprite, string itemName)
    {
        _itemImage.sprite = itemSprite;
        _itemText.text = itemName;
    }

    public void ClearSlot()
    {
        _itemImage.sprite = null;
        _itemText.text = string.Empty;
    }   
}
