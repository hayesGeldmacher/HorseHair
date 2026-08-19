using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;


public class FighterMoveDialogueTrigger : MonoBehaviour
{

    [System.Serializable]
    public class MoveDialogueRule
    {
        [Header("Rule")]
        public string ruleName;

        [Tooltip("Exact move sequence Use Any if move type does not matter")]
        public List<FighterMoveType> moveSequence = new List<FighterMoveType>();

        [Tooltip("Exact result sequence Use Any if result does not matter Can be left empty")]
        public List<FighterMoveResult> resultSequence = new List<FighterMoveResult>();

        [Tooltip("How quickly the sequence must happen")]
        public float maxSequenceTime = 3f;

        public DialogueTrigger trigger;

    }

    private struct MoveRecord
    {
        public FighterMoveType moveType;
        public FighterMoveResult result;
        public float time;

        public MoveRecord(FighterMoveType moveType, FighterMoveResult result, float time)
        {
            this.moveType = moveType;
            this.result = result;
            this.time = time;
        }
    }

    [Header("References")]
    [SerializeField] private FightCharacter fighterToWatch;
    [SerializeField] private FightRoundManager roundManager;

    [Header("Dialogue Rules")]
    [SerializeField] private List<MoveDialogueRule> rules = new List<MoveDialogueRule>();

    [Header("Display")]
    [SerializeField] private float dialogueVisibleTime = 2f;

    public bool isTalking = false; //is this script talking currently? 


    private readonly List<MoveRecord> moveHistory = new List<MoveRecord>();
    private float dialogueTimer;
    private bool wasTutorialActive;

    private void Start()
    {

        if (roundManager == null)
        {
            roundManager = FindFirstObjectByType<FightRoundManager>(
                FindObjectsInactive.Include);
        }

        wasTutorialActive = IsTutorialActive();
    }

    private void OnEnable()
    {
        if (fighterToWatch != null)
            fighterToWatch.MovePerformed += OnMovePerformed;
    }

    private void OnDisable()
    {
        if (fighterToWatch != null)
            fighterToWatch.MovePerformed -= OnMovePerformed;
    }

    private void Update()
    {
        bool tutorialActive = IsTutorialActive();

        if (tutorialActive != wasTutorialActive)
        {
            moveHistory.Clear();
            wasTutorialActive = tutorialActive;
        }

        if (tutorialActive)
            return;

        UpdateRuleCooldowns();
    }

    private bool IsTutorialActive()
    {
        return roundManager != null && roundManager.IsTutorialPhaseActive;
    }

    private void OnMovePerformed(
        FightCharacter _,
        FighterMoveType moveType,
        FighterMoveResult result)
    {
        if (IsTutorialActive())
            return;

        moveHistory.Add(new MoveRecord(moveType, result, Time.time));
        TrimOldMoveHistory();
        CheckRules();
    }

    private void CheckRules()
    {
        foreach (MoveDialogueRule rule in rules)
        {
            if (!CanRuleTrigger(rule))
                continue;

            if (DoesHistoryMatchRule(rule))
            {
                FGDialogueManager.instance.TriggerDialogue(rule.trigger);
                if (Random.value <= rule.trigger.triggerChance)
                {
                    // TriggerDialogue(rule.trigger);
                    return;
                }

                rule.trigger.cooldownTimer = rule.trigger.cooldown;
            }
        }
    }

    private bool CanRuleTrigger(MoveDialogueRule rule)
    {
        if (rule == null)
            return false;

        if (rule.moveSequence == null || rule.moveSequence.Count == 0)
            return false;

        if (rule.trigger.cooldownTimer > 0f)
            return false;

        if (rule.trigger.triggerOnlyOnce && rule.trigger.hasTriggered)
            return false;

        return true;
    }

    private bool DoesHistoryMatchRule(MoveDialogueRule rule)
    {
        int sequenceCount = rule.moveSequence.Count;

        if (moveHistory.Count < sequenceCount)
            return false;

        int historyStartIndex = moveHistory.Count - sequenceCount;

        float firstMoveTime = moveHistory[historyStartIndex].time;
        float lastMoveTime = moveHistory[moveHistory.Count - 1].time;

        if (lastMoveTime - firstMoveTime > rule.maxSequenceTime)
            return false;

        for (int i = 0; i < sequenceCount; i++)
        {
            MoveRecord record = moveHistory[historyStartIndex + i];

            FighterMoveType requiredMove = rule.moveSequence[i];
            FighterMoveResult requiredResult = GetRequiredResult(rule, i);

            if (!DoesMoveTypeMatch(requiredMove, record.moveType))
                return false;

            if (requiredResult != FighterMoveResult.Any && record.result != requiredResult)
                return false;
        }

        return true;
    }

    private bool DoesMoveTypeMatch(FighterMoveType requiredMove, FighterMoveType actualMove)
    {
        if (requiredMove == FighterMoveType.Any)
            return true;

        if (requiredMove == actualMove)
            return true;

        if (requiredMove == FighterMoveType.AnyPunch)
        {
            return actualMove == FighterMoveType.StandingPunch
                || actualMove == FighterMoveType.CrouchingPunch
                || actualMove == FighterMoveType.JumpingPunch;
        }

        if (requiredMove == FighterMoveType.AnyKick)
        {
            return actualMove == FighterMoveType.StandingKick
                || actualMove == FighterMoveType.CrouchingKick
                || actualMove == FighterMoveType.JumpingKick;
        }

        return false;
    }

    private FighterMoveResult GetRequiredResult(MoveDialogueRule rule, int index)
    {
        if (rule.resultSequence == null)
            return FighterMoveResult.Any;

        if (index >= rule.resultSequence.Count)
            return FighterMoveResult.Any;

        return rule.resultSequence[index];
    }



    private void UpdateRuleCooldowns()
    {
        foreach (MoveDialogueRule rule in rules)
        {
            if (rule.trigger.cooldownTimer > 0f)
                rule.trigger.cooldownTimer -= Time.deltaTime;
        }
    }

    private void TrimOldMoveHistory()
    {
        float longestAllowedTime = GetLongestRuleTime();

        for (int i = moveHistory.Count - 1; i >= 0; i--)
        {
            if (Time.time - moveHistory[i].time > longestAllowedTime)
                moveHistory.RemoveAt(i);
        }
    }

    private float GetLongestRuleTime()
    {
        float longestTime = 0f;

        foreach (MoveDialogueRule rule in rules)
        {
            if (rule != null && rule.maxSequenceTime > longestTime)
                longestTime = rule.maxSequenceTime;
        }

        return longestTime;
    }

    public void ResetDialogueTriggers()
    {
        moveHistory.Clear();
        dialogueTimer = 0f;

        foreach (MoveDialogueRule rule in rules)
        {
            rule.trigger.cooldownTimer = 0f;
            rule.trigger.hasTriggered = false;
        }

        // HideDialogue();
    }
}