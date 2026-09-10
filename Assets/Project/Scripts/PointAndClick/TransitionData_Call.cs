using UnityEngine;

public class TransitionData_Call : MonoBehaviour
{
    [SerializeField] private PlayerController_PointAndClick playerController;
    [SerializeField] private string nextSceneName;
    [SerializeField] private float delay = 0;
    [SerializeField] private bool useSpecialBlink = false;

    private void Awake()
    {
        PlayerController_PointAndClick.TransitionCall += OnTransitionData_Call;
    }

    private void OnDestroy()
    {
        PlayerController_PointAndClick.TransitionCall -= OnTransitionData_Call;
    }

    private void OnTransitionData_Call()
    {
        PlayerPrefs.SetInt("TimeOfDay", (int)TimeOfDay.Afternoon);
        Debug.Log("Set int to timeofday afternoon!");

        StartCoroutine(playerController.EndingSequence(null, nextSceneName, useSpecialBlink, delay));
    }
}
