using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TimeOfDay
{
    Morning,
    Afternoon
}

[System.Serializable]
public class Tasks
{
    public TimeOfDay timeOfDay;
    public int TaskNum;
    public EventClick_GoalItem goalItem;
    public Camera_Environment startingPosition;
    public string GoalText;
}

public class EventManager : MonoBehaviour
{
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
    [SerializeField] private int currentTaskNum = 0;
    [SerializeField] private Tasks[] tasks;

    private Dictionary<(TimeOfDay, int), (EventClick_GoalItem, Camera_Environment, string)> tasksList = 
        new Dictionary<(TimeOfDay, int), (EventClick_GoalItem, Camera_Environment, string)>();

    private void OnEnable()
    {
        EventClick_GoalItem.GoalCompleted += HandleGoalCompleted;
    }

    private void OnDisable()
    {
        EventClick_GoalItem.GoalCompleted -= HandleGoalCompleted;
    }

    private void Awake()
    {
        currentTaskNum = PlayerPrefs.GetInt("TaskNum", currentTaskNum);
        currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", (int)currentTimeOfDay);

        foreach (var task in tasks)
        {
            if (task.goalItem == null || task.startingPosition == null)
                continue;
            tasksList[(task.timeOfDay, task.TaskNum)] = (task.goalItem, task.startingPosition, task.GoalText);
            task.goalItem.Deactivate();
        }

        if (tasksList.ContainsKey((currentTimeOfDay, currentTaskNum)))
        {
            PlayerPrefs.SetString("Environment", tasksList[(currentTimeOfDay, currentTaskNum)].Item2.name);
            PlayerPrefs.SetString("Goal", tasksList[(currentTimeOfDay, currentTaskNum)].Item3);
        }
    }

    private void Start()
    {
        StartTask();
    }

    private void StartTask()
    {
        tasksList[(currentTimeOfDay, currentTaskNum)].Item1.Activate();
    }

    private void HandleGoalCompleted(GoalCompletionData data)
    {
        if (!data.IsCompleted)
            return;
        if (currentTimeOfDay == TimeOfDay.Afternoon)
        {
            currentTaskNum++;
            currentTimeOfDay = TimeOfDay.Morning;
        }
        else
        {
            currentTimeOfDay = TimeOfDay.Afternoon;
        }

        PlayerPrefs.SetInt("TaskNum", currentTaskNum);
        PlayerPrefs.SetInt("TimeOfDay", (int)currentTimeOfDay);
        if (tasksList.ContainsKey((currentTimeOfDay, currentTaskNum)))
        {
            PlayerPrefs.SetString("Environment", tasksList[(currentTimeOfDay, currentTaskNum)].Item2.name);
            PlayerPrefs.SetString("Goal", tasksList[(currentTimeOfDay, currentTaskNum)].Item3);
        }
        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("TaskNum");
        PlayerPrefs.DeleteKey("Environment");
        PlayerPrefs.DeleteKey("TimeOfDay");
        PlayerPrefs.DeleteKey("Goal");
    }
}
