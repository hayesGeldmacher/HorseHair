using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
    #endregion

    public Texture2D[] darkLightmapDir, darklightmapColor;
    public Texture2D[] brightLightmapDir, brightLightmapColor;

    private LightmapData[] darkLightmap, brightLightmap;
    public bool dark = false;

    public GameObject[] windowObjectsLarge, windowObjectsSmall; //all windows need to have their materials replaced for time of day!
    public Material windowLargeBright; //emissive material for large windows during the day
    public Material windowLargeDark; //emissive material for large windows during the evening

    public Material windowSmallBright; //emissive material for small windows during the day
    public Material windowSmallDark; //emissive material for small windows during the evening

    void Start() 
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
        for(int i = 0; i < brightLightmapDir.Length; i++)
        {
            LightmapData lmdata = new LightmapData();
            lmdata.lightmapDir = brightLightmapDir[i];
            lmdata.lightmapColor = brightLightmapColor[i];

            blightmap.Add(lmdata);
        }
    
        //create lightmap data for the light scene
        brightLightmap = blightmap.ToArray();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SwapLightMaps(dark);
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
}
