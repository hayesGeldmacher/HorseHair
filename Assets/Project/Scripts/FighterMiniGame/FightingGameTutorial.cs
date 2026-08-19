using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Coaches the player through the basic controls, add this only to the tutorial scene
/// </summary>
public class FightingGameTutorial : MonoBehaviour
{
    private enum TutorialStep
    {
        MoveLeftAndRight,
        Quickstep,
        JumpAndCrouch,
        Punch,
        Kick,
        Grab,
        Block,
        Special
    }

    [System.Serializable]
    private class TutorialLesson
    {
        public TutorialStep step;
        [TextArea(2, 4)] public string instruction;
        [TextArea(1, 3)] public string successResponse = "Awesome!";
    }

    [Header("References")]
    [SerializeField] private FightCharacter player;
    [SerializeField] private FGTutorialDialogue dialogueUI;
    [Tooltip("Enemy.")]
    [SerializeField] private FightCharacterAI opponentAI;

    [Header("Starting")]
    [Tooltip("Automatically begins when the player becomes active after the controls screen.")]
    [SerializeField] private bool beginWhenRoundBecomesActive = true;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float openingDelay = 0.5f;
    [SerializeField, Min(0f)] private float openingMessageTime = 2.5f;
    [SerializeField, Min(0f)] private float successMessageTime = 1.25f;

    [Header("Opening")]
    [TextArea(2, 4)]
    [SerializeField]
    private string openingMessage =
        "Okay, I know it's been a while. Let me remind you how to play.";

    [Header("Movement Detection")]
    [Tooltip("How far the player must travel in each direction.")]
    [SerializeField, Min(0.01f)] private float requiredMovementDistance = 0.5f;

    [Header("Lessons (top to bottom)")]
    [SerializeField] private List<TutorialLesson> lessons = new List<TutorialLesson>();

    [Header("Completion")]
    [TextArea(2, 4)]
    [SerializeField] private string finalMessage = "Great work! You know all the basic moves.";
    [SerializeField, Min(0f)] private float finalMessageTime = 2.5f;
    [SerializeField] private UnityEvent onTutorialCompleted;

    public event System.Action TutorialCompleted;

    public bool IsCompleted
    {
        get { return tutorialCompleted; }
    }

    private int lessonIndex;
    private bool tutorialStarted;
    private bool acceptingAction;
    private bool tutorialCompleted;
    private bool movedLeft;
    private bool movedRight;
    private bool jumped;
    private bool crouched;
    private float leftDistance;
    private float rightDistance;
    private float previousPlayerX;
    private bool subscribedToPlayer;
    private FightCharacter opponent;
    private bool subscribedToOpponent;

    private void Reset()
    {
        lessons = CreateDefaultLessons();
    }

    private void Awake()
    {
        if (player == null)
            player = FindAnyObjectByType<FightCharacter>(FindObjectsInactive.Include);

        if (dialogueUI == null)
            dialogueUI = FindAnyObjectByType<FGTutorialDialogue>(FindObjectsInactive.Include);

        if (opponentAI == null)
            opponentAI = FindAnyObjectByType<FightCharacterAI>(FindObjectsInactive.Include);

        if (opponentAI != null)
            opponent = opponentAI.GetComponent<FightCharacter>();


        SetOpponentDummyMode(true);

        if (lessons == null || lessons.Count == 0)
            lessons = CreateDefaultLessons();
    }

    private void OnEnable()
    {
        SubscribeToPlayer();
        SubscribeToOpponent();
    }

    private void Start()
    {
        if (player == null || dialogueUI == null)
        {
            Debug.LogError("FightingGameTutorial needs a player and FGTutorialDialogue reference.", this);
            return;
        }

        previousPlayerX = player.transform.position.x;
    }

    public void BeginTutorial()
    {
        if (tutorialStarted || tutorialCompleted || !isActiveAndEnabled)
            return;

        if (player == null)
            player = FindAnyObjectByType<FightCharacter>();

        if (dialogueUI == null)
            dialogueUI = FindAnyObjectByType<FGTutorialDialogue>();

        if (player == null || dialogueUI == null)
        {
            Debug.LogError(
                "Tutorial could not start. Assign Player and Dialogue UI on FightingGameTutorial.",
                this);
            return;
        }

        tutorialStarted = true;
        Debug.Log("Fighting game tutorial started.", this);
        StartCoroutine(TutorialOpeningRoutine());
    }

    private void OnDisable()
    {
        if (player != null && subscribedToPlayer)
            player.MovePerformed -= OnMovePerformed;

        if (opponent != null && subscribedToOpponent)
            opponent.MovePerformed -= OnOpponentMovePerformed;

        subscribedToPlayer = false;
        subscribedToOpponent = false;
    }

    private void Update()
    {
        if (!tutorialStarted)
        {
            if (player == null)
                player = FindAnyObjectByType<FightCharacter>(FindObjectsInactive.Include);

            SubscribeToPlayer();
            SubscribeToOpponent();

            if (beginWhenRoundBecomesActive && player != null && player.IsRoundActive)
                BeginTutorial();

            return;
        }

        if (!acceptingAction || tutorialCompleted)
            return;

        TrackHorizontalMovement();

        TutorialStep step = lessons[lessonIndex].step;

        if (step == TutorialStep.MoveLeftAndRight && movedLeft && movedRight)
            CompleteCurrentLesson();
        else if (step == TutorialStep.Quickstep && player.IsQuickstepping)
            CompleteCurrentLesson();
        else if (step == TutorialStep.JumpAndCrouch)
        {
            if (!player.IsGrounded)
                jumped = true;

            if (player.IsCrouching)
                crouched = true;

            if (jumped && crouched)
                CompleteCurrentLesson();
        }
    }

    private void SubscribeToPlayer()
    {
        if (player == null || subscribedToPlayer)
            return;

        player.MovePerformed += OnMovePerformed;
        subscribedToPlayer = true;
    }

    private void SubscribeToOpponent()
    {
        if (opponent == null || subscribedToOpponent)
            return;

        opponent.MovePerformed += OnOpponentMovePerformed;
        subscribedToOpponent = true;
    }

    private void OnOpponentMovePerformed(
        FightCharacter fighter,
        FighterMoveType moveType,
        FighterMoveResult result)
    {
        if (!acceptingAction || tutorialCompleted || fighter != opponent)
            return;

        if (lessons[lessonIndex].step == TutorialStep.Block
            && result == FighterMoveResult.Blocked)
        {
            SetOpponentDummyMode(true);
            CompleteCurrentLesson();
        }
    }

    private IEnumerator TutorialOpeningRoutine()
    {
        yield return new WaitForSeconds(openingDelay);

        dialogueUI.Show(openingMessage);
        yield return new WaitForSeconds(openingMessageTime);

        ShowCurrentInstruction();
    }

    private void ShowCurrentInstruction()
    {
        ResetLessonProgress();
        TutorialLesson lesson = lessons[lessonIndex];

        if (lesson.step == TutorialStep.Block)
            SetOpponentDummyMode(false);

        dialogueUI.Show(lesson.instruction);
        acceptingAction = true;
    }

    private void SetOpponentDummyMode(bool dummyMode)
    {
        if (opponentAI == null)
            return;

        if (dummyMode && opponent != null)
        {
            opponent.SetAIInput(
                0f,
                false,
                false,
                false,
                false,
                false,
                false);
        }

        opponentAI.enabled = !dummyMode;
    }

    private void TrackHorizontalMovement()
    {
        float currentX = player.transform.position.x;
        float deltaX = currentX - previousPlayerX;
        previousPlayerX = currentX;

        if (deltaX < 0f)
            leftDistance += -deltaX;
        else if (deltaX > 0f)
            rightDistance += deltaX;

        movedLeft = leftDistance >= requiredMovementDistance;
        movedRight = rightDistance >= requiredMovementDistance;
    }

    private void OnMovePerformed(
        FightCharacter fighter,
        FighterMoveType moveType,
        FighterMoveResult result)
    {
        if (!acceptingAction || tutorialCompleted || fighter != player)
            return;

        TutorialStep step = lessons[lessonIndex].step;

        if (step == TutorialStep.Punch && IsPunch(moveType))
            CompleteCurrentLesson();
        else if (step == TutorialStep.Kick && IsKick(moveType))
            CompleteCurrentLesson();
        else if (step == TutorialStep.Grab && moveType == FighterMoveType.Grab)
            CompleteCurrentLesson();
        else if (step == TutorialStep.Special && moveType == FighterMoveType.Special)
            CompleteCurrentLesson();
    }

    private bool IsPunch(FighterMoveType moveType)
    {
        return moveType == FighterMoveType.StandingPunch
            || moveType == FighterMoveType.CrouchingPunch
            || moveType == FighterMoveType.JumpingPunch;
    }

    private bool IsKick(FighterMoveType moveType)
    {
        return moveType == FighterMoveType.StandingKick
            || moveType == FighterMoveType.CrouchingKick
            || moveType == FighterMoveType.JumpingKick;
    }

    private void CompleteCurrentLesson()
    {
        if (!acceptingAction)
            return;

        acceptingAction = false;
        StartCoroutine(AdvanceAfterResponse());
    }

    private IEnumerator AdvanceAfterResponse()
    {
        TutorialLesson lesson = lessons[lessonIndex];
        dialogueUI.Show(lesson.successResponse);

        yield return new WaitForSeconds(successMessageTime);

        lessonIndex++;

        if (lessonIndex >= lessons.Count)
        {
            tutorialCompleted = true;
            dialogueUI.Show(finalMessage);

            yield return new WaitForSecondsRealtime(finalMessageTime);

            dialogueUI.Hide();
            SetOpponentDummyMode(false);
            TutorialCompleted?.Invoke();
            onTutorialCompleted?.Invoke();
            yield break;
        }

        ShowCurrentInstruction();
    }

    private void ResetLessonProgress()
    {
        movedLeft = false;
        movedRight = false;
        jumped = false;
        crouched = false;
        leftDistance = 0f;
        rightDistance = 0f;

        if (player != null)
            previousPlayerX = player.transform.position.x;
    }

    private static List<TutorialLesson> CreateDefaultLessons()
    {
        return new List<TutorialLesson>
        {
            NewLesson(TutorialStep.MoveLeftAndRight,
                "First, move left and right.", "Nice movement!"),
            NewLesson(TutorialStep.Quickstep,
                "Now double-tap a direction to quickstep.", "Quick! I like it."),
            NewLesson(TutorialStep.JumpAndCrouch,
                "Try jumping, then crouching.", "Great! High and low."),
            NewLesson(TutorialStep.Punch,
                "Throw a punch. You can also punch while jumping or crouching.", "Solid punch!"),
            NewLesson(TutorialStep.Kick,
                "Now kick. Kicks also work while jumping or crouching.", "Awesome kick!"),
            NewLesson(TutorialStep.Grab,
                "Get close to your opponent and grab them.", "Good grab!"),
            NewLesson(TutorialStep.Block,
                "Hold away from your opponent to block.", "Perfect defense!"),
            NewLesson(TutorialStep.Special,
                "Finish with your special move.", "That was powerful!")
        };
    }

    private static TutorialLesson NewLesson(
        TutorialStep step,
        string instruction,
        string successResponse)
    {
        return new TutorialLesson
        {
            step = step,
            instruction = instruction,
            successResponse = successResponse
        };
    }

}