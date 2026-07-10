using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

    /// <summary>
    /// this script manages lighting during the point and click segment of the game
    /// every day has a unique lighting profile for both morning and afternoon
    /// </summary>


[System.Serializable]
public struct LightProfile
{
    public float directionalIntensity;
    public Color directionalColor;
    public float ambientIntensity;
    public Material skyMat;
    public bool overrideSunRot;
    public Vector3 newSunRotation;
}

[System.Serializable]
public struct DayProfile
{
    public LightProfile morningProfile;
    public LightProfile afternoonProfile;
}

public class HouseLightingManager : MonoBehaviour
{
    [Header("Days")]
    [SerializeField] private int currentDay;
    [SerializeField] private TimeOfDay currentTime;
    [SerializeField] private DayProfile[] days;

    [Header("Light References")]
    [SerializeField] private Light directionalLight;

    private Vector3 directionalRotA = new Vector3(10, -90, -60);
    private Vector3 directionalRotM = new Vector3(57, -270, -60);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetDayAndTime();
    }

    private void SetDayAndTime()
    {
        currentTime = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", (int)currentTime);
        int taskCount = 0;
        taskCount = PlayerPrefs.GetInt("TaskNum", taskCount);
        currentDay = GetCurrentDay(taskCount);
        
        SetLighting();
    }

    private void SetLighting()
    {
        DayProfile day = days[currentDay];
        LightProfile profile = (currentTime == TimeOfDay.Morning) ? day.morningProfile : day.afternoonProfile;

        //set  direction light color, intensity, and rotation
        directionalLight.color = profile.directionalColor;
        directionalLight.intensity = profile.directionalIntensity;
        if (profile.overrideSunRot)
        {
            directionalLight.transform.localEulerAngles = profile.newSunRotation;
        }
        else
        { 
            Vector3 newRotation = (currentTime == TimeOfDay.Morning) ? directionalRotM : directionalRotA;
            directionalLight.transform.localEulerAngles = newRotation;
        }

        //set ambient and skybox lighting
        RenderSettings.ambientIntensity = profile.ambientIntensity;
        RenderSettings.skybox = profile.skyMat;
    }

    //current day is determined by task number
    //2 tasks per day, starting at 0
    //day 0 - 0,1
    //day 1 - 2,3
    //day 2 - 4,5
    //day 3 - 6,7
    //day 4 - 8,9

    private int GetCurrentDay(int taskCount)
    {
        if (taskCount == 0 || taskCount == 1) { return 0; }
        else if (taskCount == 2 || taskCount == 3) { return 1; }
        else if (taskCount == 4 || taskCount == 5) { return 2; }
        else if (taskCount == 6 || taskCount == 7) { return 3; }
        else if (taskCount == 8 || taskCount == 9) { return 4; }
        else { return 0; }
    }
}
