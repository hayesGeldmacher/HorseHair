using UnityEngine;

public class TurnOffEmissionTime : MonoBehaviour
{

    public Material doorWindowMat;

    public TimeCheck[] windowEmissionTimes;

    public Color brightColor;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool setWindowEmissive = false;

        bool isMorning = ((TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0) == TimeOfDay.Morning) ? true : false;
        int day = PlayerPrefs.GetInt("TaskNum", 0);

        foreach (var timeCheck in windowEmissionTimes)
        {
            if (timeCheck.timeOfDay == (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0) && timeCheck.TaskNum == PlayerPrefs.GetInt("TaskNum", 0))
            {
                setWindowEmissive = true;
                break;
            }
        }

        if (setWindowEmissive)
        {
            doorWindowMat.SetColor("_EmissionColor", brightColor);
        }
        else
        {
            doorWindowMat.SetColor("_EmissionColor", Color.black);
        }


    }

}
