using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles lighting changes in HouseLayout scene depending on day and time
/// </summary>

//enum for determining current baked light maps
[System.Serializable]
public enum LightMapState
{
    Morning,
    Evening,
    Blackout,
    Other
}

//enum for determining current large window materials
[System.Serializable]
public enum LargeWindowState 
{ 
    Bright,
    Dim,
    Dark,
    Blackout
}

//struct for time-specific lighting data
[System.Serializable]
public struct LightProfile
{
    public LightMapState lightMapState;
    public LargeWindowState largeWindowState;
    public bool alterReflection;
    [Range(0.0f, 1.0f)]
    public float reflectionIntensity;

}

//struct for day-specific lighting data
[System.Serializable]
public struct DayProfile
{
    public LightProfile morningProfile;
    public LightProfile afternoonProfile;
}


public class LightMapController : MonoBehaviour
{
    #region Singleton

    public static LightMapController instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of lightMapController present in scene");
            return;
        }

        instance = this;

        InitializeLightMaps();
    }
    #endregion

    [Header("Days")]
    [SerializeField] private int currentDay;
    [SerializeField] private TimeOfDay currentTime;
    [SerializeField] private DayProfile[] days;

    [Header("Testing Input")]
    public bool testingInput = false;
    private bool dark = false;
    [SerializeField] private int taskCount;

    [Header("Bright Light Maps")]
    [SerializeField] private Texture2D[] brightLightmapDir;
    [SerializeField] private Texture2D[] brightLightmapColor;

    [Header("Dark Light Maps")]
    [SerializeField] private Texture2D[] darkLightmapDir;
    [SerializeField] private Texture2D[] darkLightmapColor;

    [Header("Black Light Maps")]
    [SerializeField] private Texture2D[] blackLightmapDir;
    [SerializeField] private Texture2D[] blackLightmapColor;

    private LightmapData[] darkLightmap, brightLightmap, blackLightmap;

    [Header("Window Object References")]
    public GameObject[] windowObjectsLarge, windowObjectsSmall; //all windows need to have their materials replaced for time of day!

    [Header("Window Large Materials")]
    public Material windowLargeBright; //emissive material for large windows during the day
    public Material windowLargeDim;
    public Material windowLargeDark; //emissive material for large windows during the evening
    public Material windowLargeBlackout; //


    public delegate void ChangeLights(int day, bool isMorning);
    public ChangeLights OnChangeLights;

    void Start() 
    {
        SetTimeAndDay();
    }

    private void InitializeLightMaps()
    {
        List<LightmapData> dlightmap = new List<LightmapData>();
        for (int i = 0; i < darkLightmapDir.Length; i++)
        {
            LightmapData lmdata = new LightmapData();
            lmdata.lightmapDir = darkLightmapDir[i];
            lmdata.lightmapColor = darkLightmapColor[i];

            dlightmap.Add(lmdata);
        }

        //create lightmap data for the dark scene
        darkLightmap = dlightmap.ToArray();

        List<LightmapData> blightmap = new List<LightmapData>();
        for (int i = 0; i < brightLightmapDir.Length; i++)
        {
            LightmapData lmdata = new LightmapData();
            lmdata.lightmapDir = brightLightmapDir[i];
            lmdata.lightmapColor = brightLightmapColor[i];

            blightmap.Add(lmdata);
        }

        //create lightmap data for the light scene
        brightLightmap = blightmap.ToArray();
    }

    private void SetTimeAndDay()
    {
        currentTime = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", (int)currentTime);
        taskCount = 0;
        taskCount = PlayerPrefs.GetInt("TaskNum", taskCount);
        currentDay = GetCurrentDay(taskCount);

        bool morning = (currentTime == TimeOfDay.Morning) ? true : false;
        LoadLightProfile(currentDay, morning);
    }

    private void TickTimeAndDay()
    {
        currentTime = (currentTime == TimeOfDay.Morning) ? TimeOfDay.Afternoon : TimeOfDay.Morning;
        taskCount++;
        if(taskCount > 9)
        {
            taskCount = 0;
            currentTime = TimeOfDay.Morning;
        }
        currentDay = GetCurrentDay(taskCount);

    }

    private void Update()
    {
        if (!testingInput) { return; }
        if (Input.GetMouseButtonDown(1))
        {
            TickTimeAndDay();
            bool morning = (currentTime == TimeOfDay.Morning) ? true : false;
            LoadLightProfile(currentDay, morning);
            //LoadLightProfile();
        }
    }


    public void LoadLightProfile(int dayNum, bool morning)
    {
        DayProfile day = days[dayNum];
        LightProfile profile = (morning) ? day.morningProfile : day.afternoonProfile;

        Material newMatLarge = GetWindowLarge(profile.largeWindowState);
        foreach (GameObject window in windowObjectsLarge)
        {
            MeshRenderer renderer = window.GetComponent<MeshRenderer>();
            renderer.material = newMatLarge;
        }

        //finall, set the lightmaps!
        SetLightMap(profile.lightMapState);

        float newReflectionIntensity = (profile.alterReflection ? profile.reflectionIntensity : 1.0f);
        RenderSettings.reflectionIntensity = newReflectionIntensity;

        bool isMorning = (currentTime == TimeOfDay.Morning) ? true : false;
        OnChangeLights?.Invoke(currentDay, isMorning);
    }

    private Material GetWindowLarge(LargeWindowState windowState)
    {
        
        switch (windowState)
        {
            case LargeWindowState.Bright:
                    return windowLargeBright;
            case LargeWindowState.Dim:
                    return windowLargeDim;
            case LargeWindowState.Dark:
                    return windowLargeDark;
            case LargeWindowState.Blackout:
                    return windowLargeBlackout;
        }

        return windowLargeBright;
    }

    private void SetLightMap(LightMapState state)
    {
        switch (state)
        {
            case LightMapState.Morning:
                LightmapSettings.lightmaps = brightLightmap;
                break;
            case LightMapState.Evening:
                LightmapSettings.lightmaps = darkLightmap;
                break;
            case LightMapState.Blackout:
                LightmapSettings.lightmaps = blackLightmap;
                break;  
        }
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
