using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MusicDay 
{
    public MusicTimeSlot morningSlot;
    public MusicTimeSlot afternoonSlot;
}

[System.Serializable]
public struct MusicTimeSlot
{
    public AudioClip track;
    public bool startOnAwake;
}

public class MusicManagerHouse : MonoBehaviour
{

    /// <summary>
    /// This script manages the background music that plays during the house scenes
    /// </summary>

    [Header("References")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioVolumeFade volumeFade;

    [Header("Tracks Per Day")]
    public List<MusicDay> musicDays = new List<MusicDay>();
    [SerializeField] private MusicTimeSlot currentSlot;
    [SerializeField] private bool isMorning;
    [SerializeField] private int day;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isMorning = ((TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0) == TimeOfDay.Morning) ? true : false;
        day = PlayerPrefs.GetInt("TaskNum", 0);
        GetCurrentSlot();
    }

    private void GetCurrentSlot()
    {
        //don't progress if there are not listed tracks for the day
        if(day > musicDays.Count){ 
            if(source != null) { source.Stop(); }
            return; 
        }  
        MusicDay currentDay = musicDays[day];
        currentSlot = (isMorning) ? currentDay.morningSlot : currentDay.afternoonSlot;
        if(currentSlot.track != null)
        {
            volumeFade.SetAudioClip(currentSlot.track);
        }
        if(currentSlot.track != null && currentSlot.startOnAwake) { volumeFade.StartFadeIn(true, true); }
    }

   

}
