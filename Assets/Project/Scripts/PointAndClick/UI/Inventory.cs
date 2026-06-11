using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Slot[] _slots;

    public void AddItem(Sprite itemSprite, string itemName, int index)
    {
        _slots[index].SetItem(itemSprite, itemName);
    }

    public void RemoveItem(int index)
    {
        _slots[index].ClearSlot();
    }
}
