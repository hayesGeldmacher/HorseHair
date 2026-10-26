using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    [Header("Audio Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider interactablesSlider;

    private void Start()
    {
        if (mixer == null)
        {
            Debug.LogError("AudioSettings: assign AM_General.", this);
            return;
        }

        SetupSlider(musicSlider, "MusicVolume");
        SetupSlider(sfxSlider, "SFXVolume");
        SetupSlider(interactablesSlider, "InteractablesVolume");
    }

    private void SetupSlider(Slider slider, string parameter)
    {
        string key = "Settings.Audio." + parameter;

        float savedVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(key, 0.5f));

        ApplyVolume(parameter, savedVolume);

        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        slider.SetValueWithoutNotify(savedVolume);

        slider.onValueChanged.AddListener(value =>
        {
            value = Mathf.Clamp01(value);

            ApplyVolume(parameter, value);
            PlayerPrefs.SetFloat(key, value);
        });
    }

    private void ApplyVolume(string parameter, float value)
    {
        float gain = Mathf.Clamp01(value) * 2f;

        float decibels = gain <= 0f
            ? -80f
            : Mathf.Max(-80f, 20f * Mathf.Log10(gain));

        if (!mixer.SetFloat(parameter, decibels))
        {
            Debug.LogError(
                "AudioSettings: missing exposed parameter: " +
                parameter,
                this);
        }
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
            PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}