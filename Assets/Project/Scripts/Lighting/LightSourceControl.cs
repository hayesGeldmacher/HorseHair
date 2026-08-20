using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script attaches to individual objects in the scene that utilize different materials for lit and unlit variations
/// This script allows the user to determine whether the object is lit or unlit at each day and time in the game
/// </summary>

[System.Serializable]
public struct LightingProfile
{
    //set up so that default values don't need to be touched!
    public bool activatedMorning;
    public bool notActivatedEvening;
}


public class LightSourceControl : MonoBehaviour
{
    [SerializeField] LightingProfile day1;
    [SerializeField] LightingProfile day2;
    [SerializeField] LightingProfile day3;
    [SerializeField] LightingProfile day4; 
    public Material darkMat;
    public Material lightMat;

    private void Start()
    {
        LightMapController.instance.OnChangeLights += SetLighting;
    }

    private void SetLighting(int day, bool isMorning)
    {
        Debug.Log("Set Lighting with day: " + day + " and isMorning: " + isMorning);

        LightingProfile profile = GetLightProfile(day);
        Material newMat = lightMat;
        if (isMorning) { 
            newMat = (profile.activatedMorning ? lightMat : darkMat);   
        }
        else
        {
            newMat = (profile.notActivatedEvening ? darkMat: lightMat);
        }

        MeshRenderer renderer = transform.GetComponent<MeshRenderer>();
        renderer.material = newMat;
    }

    private LightingProfile GetLightProfile(int index)
    {
        switch (index)
        {
            case 0: return day1;
                break;
            case 1: return day2;
                break;
            case 2: return day3;
                break;
            case 3: return day4;
                break;
        }

        return day1;
    }

}
