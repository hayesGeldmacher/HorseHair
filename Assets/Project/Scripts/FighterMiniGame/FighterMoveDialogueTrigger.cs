using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        [TextArea]
        public string dialogueLine;

        public float cooldown = 2f;
        public bool triggerOnlyOnce;

        [HideInInspector] public float cooldownTimer;
        [HideInInspector] public bool hasTriggered;
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
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Dialogue Rules")]
    [SerializeField] private List<MoveDialogueRule> rules = new List<MoveDialogueRule>();

    [Header("Display")]
    [SerializeField] private float dialogueVisibleTime = 2f;

    private readonly List<MoveRecord> moveHistory = new List<MoveRecord>();
    private float dialogueTimer;

    private void Start()
    {
        HideDialogue();
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
        UpdateRuleCooldowns();
        UpdateDialogueTimer();
    }

    private void OnMovePerformed(
        FightCharacter _,
        FighterMoveType moveType,
        FighterMoveResult result)
    {
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
                TriggerDialogue(rule);
                return;
            }
        }
    }

    private bool CanRuleTrigger(MoveDialogueRule rule)
    {
        if (rule == null)
            return false;

        if (rule.moveSequence == null || rule.moveSequence.Count == 0)
            return false;

        if (rule.cooldownTimer > 0f)
            return false;

        if (rule.triggerOnlyOnce && rule.hasTriggered)
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

    private void TriggerDialogue(MoveDialogueRule rule)
    {
        if (dialogueText != null)
            dialogueText.text = rule.dialogueLine;

        ShowDialogue();

        dialogueTimer = dialogueVisibleTime;
        rule.cooldownTimer = rule.cooldown;
        rule.hasTriggered = true;
    }

    private void ShowDialogue()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);
    }

    private void HideDialogue()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);
    }

    private void UpdateDialogueTimer()
    {
        if (dialogueTimer <= 0f)
            return;

        dialogueTimer -= Time.deltaTime;

        if (dialogueTimer <= 0f)
            HideDialogue();
    }

    private void UpdateRuleCooldowns()
    {
        foreach (MoveDialogueRule rule in rules)
        {
            if (rule.cooldownTimer > 0f)
                rule.cooldownTimer -= Time.deltaTime;
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
            rule.cooldownTimer = 0f;
            rule.hasTriggered = false;
        }

        HideDialogue();
    }
}