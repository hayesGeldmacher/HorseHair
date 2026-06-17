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
}

public class EventManager : MonoBehaviour
{
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
    [SerializeField] private int currentTaskNum = 0;
    [SerializeField] private Tasks[] tasks;

    [Header("Scenes")]
    [SerializeField] private string fightingGameScene;
    [SerializeField] private string houseScene;

    private Dictionary<(TimeOfDay, int), (EventClick_GoalItem, Camera_Environment)> tasksList = 
        new Dictionary<(TimeOfDay, int), (EventClick_GoalItem, Camera_Environment)>();

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
        currentTaskNum = PlayerPrefs.GetInt("TaskNum", 0);
        currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0);

        foreach (var task in tasks)
        {
            if (task.goalItem == null || task.startingPosition == null)
                continue;
            tasksList[(task.timeOfDay, task.TaskNum)] = (task.goalItem, task.startingPosition);
            task.goalItem.Deactivate();
        }

        if (tasksList.ContainsKey((currentTimeOfDay, currentTaskNum)))
            PlayerPrefs.SetString("Environment", tasksList[(currentTimeOfDay, currentTaskNum)].Item2.name);
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
        string nextSceneName;
        if (currentTimeOfDay == TimeOfDay.Afternoon)
        {
            currentTaskNum++;
            currentTimeOfDay = TimeOfDay.Morning;
            nextSceneName = fightingGameScene;
        }
        else
        {
            currentTimeOfDay = TimeOfDay.Afternoon;
            nextSceneName = houseScene;
        }

        PlayerPrefs.SetInt("TaskNum", currentTaskNum);
        PlayerPrefs.SetInt("TimeOfDay", (int)currentTimeOfDay);
        if (tasksList.ContainsKey((currentTimeOfDay, currentTaskNum)))
            PlayerPrefs.SetString("Environment", tasksList[(currentTimeOfDay, currentTaskNum)].Item2.name);
        PlayerPrefs.Save();

        SceneManager.LoadScene(nextSceneName);
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("TaskNum");
        PlayerPrefs.DeleteKey("Environment");
        PlayerPrefs.DeleteKey("TimeOfDay");
    }
}
