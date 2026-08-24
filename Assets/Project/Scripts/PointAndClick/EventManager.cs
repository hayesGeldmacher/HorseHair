using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TimeOfDay
{
    Morning,
    Afternoon,
    Dream,
}

[Serializable]
public class  TimeCheck
{
    public TimeOfDay timeOfDay;
    public int TaskNum;
}

[System.Serializable]
public class Tasks
{
    public TimeOfDay timeOfDay;
    public int TaskNum;
    public EventClick_TaskGiver TaskItem;
    public string WakeUpTask;
}

public class EventManager : MonoBehaviour
{
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
    [SerializeField] private int currentTaskNum = 0;
    [SerializeField] private Tasks[] tasks;
    [SerializeField] private bool TVSet = false;

    private Dictionary<(TimeOfDay, int), EventClick_TaskGiver> tasksList =
        new Dictionary<(TimeOfDay, int), EventClick_TaskGiver>();
    private Dictionary<(TimeOfDay, int), string> wakeUpTasks = 
        new Dictionary<(TimeOfDay, int), string>();

    public static event System.Action<DialogueStorage> ThoughtDialogue;

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
        string startingTask = "";
        currentTaskNum = PlayerPrefs.GetInt("TaskNum", currentTaskNum);
        currentTimeOfDay = (TimeOfDay)PlayerPrefs.GetInt("TimeOfDay", (int)currentTimeOfDay);

        foreach (var task in tasks)
        {
            tasksList[(task.timeOfDay, task.TaskNum)] = task.TaskItem;
            task.TaskItem.task.finalPoint.Activated = false;
            task.TaskItem.Activated = false;
            wakeUpTasks[(task.timeOfDay, task.TaskNum)] = task.WakeUpTask;
        }

        if (TVSet)
        {
            PlayerPrefs.SetString("Environment",
                 tasksList[(TimeOfDay.Dream, 0)].task.startingPosition.name);
            tasksList[(TimeOfDay.Dream, 0)].Activated = true;
        }
        else
        {
            if (tasksList.ContainsKey((currentTimeOfDay, currentTaskNum)))
            {
                PlayerPrefs.SetString("Environment",
                    tasksList[(currentTimeOfDay, currentTaskNum)].task.startingPosition.name);
                tasksList[(currentTimeOfDay, currentTaskNum)].Activated = true;
                startingTask = wakeUpTasks[(currentTimeOfDay, currentTaskNum)];
            }
        }

        PlayerPrefs.SetInt("TaskNum", currentTaskNum);
        PlayerPrefs.SetInt("TimeOfDay", (int)currentTimeOfDay);
        PlayerPrefs.SetString("StartingTask", startingTask);
    }

    private void Start()
    {
        StartTask();     
    }

    private void StartTask()
    {
        if (TVSet)
        {
            tasksList[(TimeOfDay.Dream, 0)].ChangeTaskStatus(true);
            tasksList[(TimeOfDay.Dream, 0)].task.finalPoint.Activated = true;
            ThoughtDialogue?.Invoke(tasksList[(TimeOfDay.Dream, 0)].task.ThoughtText);
        }
        else
        {
            tasksList[(currentTimeOfDay, currentTaskNum)].ChangeTaskStatus(true);
            tasksList[(currentTimeOfDay, currentTaskNum)].task.finalPoint.Activated = true;
            ThoughtDialogue?.Invoke(tasksList[(currentTimeOfDay, currentTaskNum)].task.ThoughtText);
        }
    }

    private void HandleGoalCompleted(GoalCompletionData data)
    {
        //if (!data.IsCompleted)
        //    return;
        //if (currentTimeOfDay == TimeOfDay.Afternoon)
        //{
        //    currentTaskNum++;
        //    currentTimeOfDay = TimeOfDay.Morning;
        //}
        //else
        //{
        //    currentTimeOfDay = TimeOfDay.Afternoon;
        //}

        //PlayerPrefs.SetInt("TaskNum", currentTaskNum);
        //PlayerPrefs.SetInt("TimeOfDay", (int)currentTimeOfDay);
        //if (tasksList.ContainsKey((currentTimeOfDay, currentTaskNum)))
        //{
        //    PlayerPrefs.SetString("Environment",
        //        tasksList[(currentTimeOfDay, currentTaskNum)].task.startingPosition.name);
        //    //PlayerPrefs.SetString("Goal",
        //    //    tasksList[(currentTimeOfDay, currentTaskNum)].task.GoalText);
        //    PlayerPrefs.SetString("Thoughts",
        //        tasksList[(currentTimeOfDay, currentTaskNum)].task.ThoughtText);
        //}
        //PlayerPrefs.Save();
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("TaskNum");
        PlayerPrefs.DeleteKey("Environment");
        PlayerPrefs.DeleteKey("TimeOfDay");
        PlayerPrefs.DeleteKey("Goal");
        PlayerPrefs.DeleteKey("Thoughts");
        PlayerPrefs.Save();
    }
}
