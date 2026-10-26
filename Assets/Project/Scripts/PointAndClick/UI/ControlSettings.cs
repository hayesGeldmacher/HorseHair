using UnityEngine;
using UnityEngine.UI;

public class ControlSettings : MonoBehaviour
{
    [SerializeField] private Slider mouseSensitivity;
    [SerializeField] private Slider controllerSensitivity;
    [SerializeField] private Toggle mouseInvertY;
    [SerializeField] private Toggle controllerInvertY;

    private const string MouseKey = "Settings.MouseSensitivity";
    private const string ControllerKey = "Settings.ControllerSensitivity";
    private const string MouseInvertKey = "Settings.MouseInvertY";
    private const string ControllerInvertKey = "Settings.ControllerInvertY";

    public static float MouseSensitivity =>
        Mathf.Clamp(
            PlayerPrefs.GetFloat(MouseKey, 1.625f),
            0.25f,
            3f);

    public static float ControllerSensitivity =>
        Mathf.Clamp(PlayerPrefs.GetFloat(ControllerKey, 1f), 0.25f, 3f);

    public static bool MouseInvertY =>
        PlayerPrefs.GetInt(MouseInvertKey, 0) == 1;

    public static bool ControllerInvertY =>
        PlayerPrefs.GetInt(ControllerInvertKey, 0) == 1;

    private void Start()
    {
        SetupSlider(mouseSensitivity, MouseSensitivity);
        SetupSlider(controllerSensitivity, ControllerSensitivity);

        mouseInvertY.SetIsOnWithoutNotify(MouseInvertY);
        controllerInvertY.SetIsOnWithoutNotify(ControllerInvertY);

        mouseSensitivity.onValueChanged.AddListener(value =>
            PlayerPrefs.SetFloat(MouseKey, value));

        controllerSensitivity.onValueChanged.AddListener(value =>
            PlayerPrefs.SetFloat(ControllerKey, value));

        mouseInvertY.onValueChanged.AddListener(value =>
            PlayerPrefs.SetInt(MouseInvertKey, value ? 1 : 0));

        controllerInvertY.onValueChanged.AddListener(value =>
            PlayerPrefs.SetInt(ControllerInvertKey, value ? 1 : 0));
    }

    private static void SetupSlider(Slider slider, float value)
    {
        slider.minValue = 0.25f;
        slider.maxValue = 3f;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(value);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}