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
    public bool isSmallWindowBright;
    public bool isFrontDoorBright;

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

    [Header("Bright Light Maps")]
    public Texture2D[] brightLightmapDir, brightLightmapColor;

    [Header("Dark Light Maps")]
    public Texture2D[] darkLightmapDir, darklightmapColor;

    [Header("Black Light Maps")]
    public Texture2D[] blackLightmapDir, blackLightmapColor;

    private LightmapData[] darkLightmap, brightLightmap, blackLightmap;

    private bool dark = false;

    [Header("Testing Input")]
    public bool testDark = false;

    [Header("Window Object References")]
    public GameObject[] windowObjectsLarge, windowObjectsSmall; //all windows need to have their materials replaced for time of day!

    [Header("Window Large Materials")]
    public Material windowLargeBright; //emissive material for large windows during the day
    public Material windowLargeDim;
    public Material windowLargeDark; //emissive material for large windows during the evening
    public Material windowLargeBlackout; //

    [Header("Window Small Materials")]
    public Material windowSmallBright; //emissive material for small windows during the day
    public Material windowSmallDark; //emissive material for small windows during the evening

    [Header("Door Object & Materials")]
    public GameObject frontDoor;
    public Material frontDoorBright;
    public Material frontDoorDark;

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
            lmdata.lightmapColor = darklightmapColor[i];

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
        int taskCount = 0;
        taskCount = PlayerPrefs.GetInt("TaskNum", taskCount);
        currentDay = GetCurrentDay(taskCount);

        LoadLightProfile();
    }

    private void Update()
    {
        if (!testDark) { return; }
        if (Input.GetMouseButtonDown(0))
        {
            SwapLightMaps(dark);
        }
    }


    public void LoadLightProfile()
    {
        DayProfile day = days[currentDay];
        LightProfile profile = (currentTime == TimeOfDay.Morning) ? day.morningProfile : day.afternoonProfile;

        Material newMatLarge = GetWindowLarge(profile.largeWindowState);
        foreach (GameObject window in windowObjectsLarge)
        {
            MeshRenderer renderer = window.GetComponent<MeshRenderer>();
            renderer.material = newMatLarge;
        }

        //assign materials to small windows
        Material newMaterialSmall = (profile.isSmallWindowBright ? windowSmallBright : windowSmallDark);
        foreach (GameObject window in windowObjectsSmall)
        {
            MeshRenderer renderer = window.GetComponent<MeshRenderer>();
            renderer.material = newMaterialSmall;
        }

        Material newMaterialDoor = (profile.isFrontDoorBright ? frontDoorBright : frontDoorDark);
        MeshRenderer doorRenderer = frontDoor.GetComponent<MeshRenderer>();
        doorRenderer.material = newMaterialDoor;

        //finall, set the lightmaps!
        SetLightMap(profile.lightMapState);
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


    private void SwapLightMaps(bool isDark)
    {
        LightmapSettings.lightmaps = (isDark ? brightLightmap : darkLightmap);
        SwapWindowsMaterials(isDark);
        dark = !dark;
        
    }

    void SwapWindowsMaterials(bool isDark)
    {
        //assign new materials to large windows
        Material newMaterialLarge = (isDark ? windowLargeBright : windowLargeDark);
        foreach (GameObject window in windowObjectsLarge)
        {
            MeshRenderer renderer = window.GetComponent<MeshRenderer>();
            renderer.material = newMaterialLarge;
        }

        //assign materials to small windows
        Material newMaterialSmall = (isDark ? windowSmallBright : windowSmallDark);
        foreach(GameObject window in windowObjectsSmall)
        {
            MeshRenderer renderer = window.GetComponent<MeshRenderer>();
            renderer.material = newMaterialSmall;
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
