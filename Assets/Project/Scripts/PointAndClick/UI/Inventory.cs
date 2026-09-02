using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum PhoneState
{
    None,
    Inventory,
    Tasks
}

public class Inventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private Slot[] _slots;
    [SerializeField] private GameObject inventoryPanel;
    [Header("Tasks")]
    [SerializeField] private GameObject taskPanel;
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Animator phoneAnimation;
    [SerializeField] private Button inventoryBtn;
    [SerializeField] private Button taskBtn;
    [SerializeField] private Button phoneBtn;

    private Coroutine _fadeCoroutine;
    private PhoneState currentPhoneState = PhoneState.None;

    public void OnClickTask()
    {
        inventoryPanel.SetActive(false);
        taskPanel.SetActive(true);
        currentPhoneState = PhoneState.Tasks;
        taskBtn.Select();
    }

    public void OnClickInventory()
    {
        inventoryPanel.SetActive(true);
        taskPanel.SetActive(false);
        currentPhoneState = PhoneState.Inventory;
        inventoryBtn.Select();
    }

    public void OnClickPhone()
    {
        inventoryPanel.SetActive(true);
        taskPanel.SetActive(false);
        currentPhoneState = PhoneState.Inventory;
        phoneBtn.Select();
    }

    private void Start()
    {
        fadeGroup.alpha = 0;
        currentPhoneState = PhoneState.Tasks;
        inventoryPanel.SetActive(false);
        taskPanel.SetActive(true);
    }

    public void AddItem(Sprite itemSprite, string itemName, int index)
    {
        _slots[index].SetItem(itemSprite, itemName);
    }

    public void RemoveItem(int index)
    {
        _slots[index].ClearSlot();
    }

    public void ShowInventory(PhoneState phoneState)
    {
        phoneAnimation.SetTrigger("Open Phone");
        StartCoroutine(ShowPhoneScreen(phoneState));
    }

    public IEnumerator ShowPhoneScreen(PhoneState phoneState)
    {
        yield return new WaitUntil(() => 
        phoneAnimation.GetCurrentAnimatorStateInfo(0).IsName("Opened"));
        fadeGroup.alpha = 1;
        switch (phoneState)
        {
            case PhoneState.Inventory:
                OnClickInventory();
                break;
            case PhoneState.Tasks:
                OnClickTask();
                break;
            default:
                break;
        }
    }

    public void HideInventory()
    {
        StartFade(0f);
    }

    public void HideInventoryInstant()
    {
        fadeGroup.alpha = 0;
        phoneAnimation.SetTrigger("Close Phone");
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
