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
}

public class EventManager : MonoBehaviour
{
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
    [SerializeField] private int currentTaskNum = 0;
    [SerializeField] private Tasks[] tasks;

    [Header("Scenes")]
    [SerializeField] private string fightingGameScene;
    [SerializeField] private string houseScene;

    private Dictionary<(TimeOfDay, int), EventClick_GoalItem> tasksList = 
        new Dictionary<(TimeOfDay, int), EventClick_GoalItem>();

    private void OnEnable()
    {
        EventClick_GoalItem.GoalCompleted += HandleGoalCompleted;
    }

    private void OnDisable()
    {
        EventClick_GoalItem.GoalCompleted -= HandleGoalCompleted;
    }

    private void Start()
    {
        currentTaskNum = PlayerPrefs.GetInt("TaskNum", 0);
        currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", 0);

        foreach (var task in tasks)
        {
            tasksList[(task.timeOfDay, task.TaskNum)] = task.goalItem;
            task.goalItem.Deactivate();
        }

        StartTask();
    }

    private void StartTask()
    {
        tasksList[(currentTimeOfDay, currentTaskNum)].Activate();
    }

    private void HandleGoalCompleted(GoalCompletionData data)
    {
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
        PlayerPrefs.Save();

        SceneManager.LoadScene(nextSceneName);
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("TaskNum");
        PlayerPrefs.DeleteKey("TimeOfDay");
    }
}
