using System.Collections;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Slot[] _slots;
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine _fadeCoroutine;

    private void Start()
    {
        fadeGroup.alpha = 0;
    }

    public void AddItem(Sprite itemSprite, string itemName, int index)
    {
        _slots[index].SetItem(itemSprite, itemName);
    }

    public void RemoveItem(int index)
    {
        _slots[index].ClearSlot();
    }

    public void ShowInventory()
    {
        fadeGroup.alpha = 1;
    }

    public void HideInventory()
    {
        StartFade(0f);
    }

    public void HideInventoryInstant()
    {
        fadeGroup.alpha = 0;
    }

    private void StartFade(float targetAlpha)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }

    public bool CheckItemInInventory(string itemName)
    {
        foreach (var slot in _slots)
        {
            if (slot.GetName() == itemName)
            {
                return true;
            }
        }
        return false;
    }
}
