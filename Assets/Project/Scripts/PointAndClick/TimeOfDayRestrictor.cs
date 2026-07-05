using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NEITimeSetting
{
    public TimeOfDay timeOfDay;
    public int TaskNum;
}

public class TimeOfDayRestrictor : MonoBehaviour
{
    [Header("NEI Settings")]
    [SerializeField] private bool AvailableEveryDay = true;
    [SerializeField] private List<NEITimeSetting> activeTimeOfDays = new List<NEITimeSetting>();

    private void Awake()
    {
        if (!AvailableEveryDay)
        {
            bool isActive = false;
            TimeOfDay currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", (int)TimeOfDay.Morning);
            int currentTaskNum = PlayerPrefs.GetInt("TaskNum", 0);
            foreach (var timeSetting in activeTimeOfDays)
            {
                if (timeSetting.timeOfDay == currentTimeOfDay && timeSetting.TaskNum == currentTaskNum)
                {
                    isActive = true;
                    break;
                }
            }
            gameObject.SetActive(isActive);
            if (gameObject.TryGetComponent<EventClick_NEI>(out var eventClickNEI))
            {
                eventClickNEI.Activated = isActive;
            }
        }
    }
}
