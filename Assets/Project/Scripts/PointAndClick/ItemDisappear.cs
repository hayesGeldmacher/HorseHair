using System.Numerics;
using UnityEngine;

public class ItemDisappear : MonoBehaviour
{
    [SerializeField] private GameObject alternativeItem;
    [Header("Day Limitter")]
    [SerializeField] private bool availableEveryday = true;
    [SerializeField] private TimeCheck[] availableTime;

    private bool isUsed = true;

    private void Awake()
    {
        if (!availableEveryday)
        {
            bool isAvailable = false;
            foreach (var timeCheck in availableTime)
            {
                if (timeCheck.timeOfDay == (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0) && timeCheck.TaskNum == PlayerPrefs.GetInt("TaskNum", 0))
                {
                    isAvailable = true;
                    break;
                }
            }
            isUsed = isAvailable;

        }
    }

    public void ActivateOrDeactivate(bool state)
    {
        if (!isUsed)
            state = false;

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
