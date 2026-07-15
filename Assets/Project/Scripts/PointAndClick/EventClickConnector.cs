using System;
using UnityEngine;

public class EventClickConnector : MonoBehaviour
{
    [SerializeField] private TelevisionSequence televisionSequence;

    private void OnEnable()
    {
        EventClick.OnObjectClicked += HandleObjectClicked;
    }

    private void OnDisable()
    {
        EventClick.OnObjectClicked -= HandleObjectClicked;
    }

    private void HandleObjectClicked(ClickEventData data)
    {
        televisionSequence.InteractTelevision();
    }
}
