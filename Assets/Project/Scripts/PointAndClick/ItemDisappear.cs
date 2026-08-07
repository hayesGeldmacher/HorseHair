using UnityEngine;

public class ItemDisappear : MonoBehaviour
{
    [SerializeField] private GameObject alternativeItem;

    public void ActivateOrDeactivate(bool state)
    {
        if (alternativeItem)
        {
            if (state)
            {
                alternativeItem.SetActive(false);
                gameObject.SetActive(true);
            }
            else
            {
                alternativeItem.SetActive(true);
                gameObject.SetActive(false);
            }
        }
        else
        {
            gameObject.SetActive(state);
        }
    }
}
