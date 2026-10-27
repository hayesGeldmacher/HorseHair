using UnityEngine;

public class DisableBasedOnDay : MonoBehaviour
 
{
    [SerializeField] private TimeCheck[] timeCheck;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool enable = false;
        foreach (var timeCheck in timeCheck)
        {
            if (timeCheck.timeOfDay == (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0) && timeCheck.TaskNum == PlayerPrefs.GetInt("TaskNum", 0))
            {
               enable = true;
                break;
            }

        }


        if (!enable)
        {
            gameObject.SetActive(false);
        }
        
    }

}
