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
    public TaskList taskList;
}

[System.Serializable]
public class TaskList
{
    public EventClick_GoalItem goalItem;
    public Camera_Environment startingPosition;
    public string GoalText;
    public string ThoughtText;
}

public class EventManager : MonoBehaviour
{
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
    [SerializeField] private int currentTaskNum = 0;
    [SerializeField] private Tasks[] tasks;

    private Dictionary<(TimeOfDay, int), TaskList> tasksList =
        new Dictionary<(TimeOfDay, int), TaskList>();

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
            if (task.taskList.goalItem == null || task.taskList.startingPosition == null)
                continue;
            tasksList[(task.timeOfDay, task.TaskNum)] = task.taskList;
            task.taskList.goalItem.Deactivate();
        }

        if (tasksList.ContainsKey((currentTimeOfDay, currentTaskNum)))
        {
            PlayerPrefs.SetString("Environment", 
                tasksList[(currentTimeOfDay, currentTaskNum)].startingPosition.name);
            PlayerPrefs.SetString("Goal", 
                tasksList[(currentTimeOfDay, currentTaskNum)].GoalText);
            PlayerPrefs.SetString("Thoughts", 
                tasksList[(currentTimeOfDay, currentTaskNum)].ThoughtText);
        }
    }

    private void Start()
    {
        StartTask();
    }

    private void StartTask()
    {
        tasksList[(currentTimeOfDay, currentTaskNum)].goalItem.Activate();
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
            PlayerPrefs.SetString("Environment", 
                tasksList[(currentTimeOfDay, currentTaskNum)].startingPosition.name);
            PlayerPrefs.SetString("Goal", 
                tasksList[(currentTimeOfDay, currentTaskNum)].GoalText);
            PlayerPrefs.SetString("Thoughts", 
                tasksList[(currentTimeOfDay, currentTaskNum)].ThoughtText);
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
        PlayerPrefs.DeleteKey("Thoughts");
    }
}
