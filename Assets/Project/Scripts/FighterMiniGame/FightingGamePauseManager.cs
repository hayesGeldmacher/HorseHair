using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FightingGamePauseManager : MonoBehaviour
{
    [Header("Fighting Game")]
    [SerializeField] private GameObject fightingGameParent;

    [Header("Pause UI")]
    [SerializeField] private GameObject fightingGamePauseScreen;
    [SerializeField] private GameObject controlsPanel;

    [Header("Default UI Selection")]
    [SerializeField] private Selectable firstPauseSelection;
    [SerializeField] private Selectable firstControlsSelection;

    public bool IsPaused { get; private set; }

    private MonoBehaviour[] scripts;
    private bool[] previousScriptStates;

    private Animator[] animators;
    private float[] previousAnimatorSpeeds;

    private Rigidbody2D[] rigidbodies;
    private bool[] previousRigidbodyStates;

    private AudioSource[] audioSources;
    private bool[] audioWasPlaying;

    private Coroutine selectionCoroutine;

    private void Start()
    {
        if (fightingGamePauseScreen != null)
        {
            fightingGamePauseScreen.SetActive(false);
        }

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) ||
            Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            GoBack();
        }
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseFightingGame();
        }
    }

    private void PauseFightingGame()
    {
        if (fightingGameParent == null ||
            fightingGamePauseScreen == null)
        {
            return;
        }

        IsPaused = true;

        fightingGamePauseScreen.SetActive(true);

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }

        FindFightingGameComponents();

        PauseScripts();
        PauseAnimators();
        PausePhysics();
        PauseAudio();

        SelectMenuItem(firstPauseSelection);
    }

    public void ResumeGame()
    {
        if (!IsPaused)
        {
            return;
        }

        IsPaused = false;

        if (selectionCoroutine != null)
        {
            StopCoroutine(selectionCoroutine);
            selectionCoroutine = null;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        ResumeScripts();
        ResumeAnimators();
        ResumePhysics();
        ResumeAudio();

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }

        if (fightingGamePauseScreen != null)
        {
            fightingGamePauseScreen.SetActive(false);
        }
    }

    public void ShowControls()
    {
        if (!IsPaused ||
            controlsPanel == null ||
            fightingGamePauseScreen == null)
        {
            return;
        }

        fightingGamePauseScreen.SetActive(false);
        controlsPanel.SetActive(true);

        SelectMenuItem(firstControlsSelection);
    }

    public void HideControls()
    {
        if (!IsPaused ||
            controlsPanel == null ||
            fightingGamePauseScreen == null)
        {
            return;
        }

        controlsPanel.SetActive(false);
        fightingGamePauseScreen.SetActive(true);

        SelectMenuItem(firstPauseSelection);
    }

    public void GoBack()
    {
        if (!IsPaused)
        {
            return;
        }

        if (controlsPanel != null &&
            controlsPanel.activeInHierarchy)
        {
            HideControls();
        }
        else
        {
            ResumeGame();
        }
    }

    private void FindFightingGameComponents()
    {
        scripts =
            fightingGameParent.GetComponentsInChildren<MonoBehaviour>(true);

        animators =
            fightingGameParent.GetComponentsInChildren<Animator>(true);

        rigidbodies =
            fightingGameParent.GetComponentsInChildren<Rigidbody2D>(true);

        audioSources =
            fightingGameParent.GetComponentsInChildren<AudioSource>(true);


        previousScriptStates = new bool[scripts.Length];
        previousAnimatorSpeeds = new float[animators.Length];
        previousRigidbodyStates = new bool[rigidbodies.Length];
        audioWasPlaying = new bool[audioSources.Length];
    }

    private void PauseScripts()
    {
        for (int i = 0; i < scripts.Length; i++)
        {
            MonoBehaviour script = scripts[i];

            if (ShouldStayEnabled(script))
            {
                continue;
            }

            previousScriptStates[i] = script.enabled;
            script.enabled = false;
        }
    }

    private void ResumeScripts()
    {
        if (scripts == null ||
            previousScriptStates == null)
        {
            return;
        }

        for (int i = 0; i < scripts.Length; i++)
        {
            MonoBehaviour script = scripts[i];

            if (ShouldStayEnabled(script))
            {
                continue;
            }

            if (script != null)
            {
                script.enabled = previousScriptStates[i];
            }
        }
    }

    private bool ShouldStayEnabled(MonoBehaviour script)
    {
        if (script == null)
        {
            return true;
        }

        if (script == this)
        {
            return true;
        }

        if (script is RenderTextureUIInput)
        {
            return true;
        }

        if (script is BaseRaycaster)
        {
            return true;
        }

        if (script is EventSystem)
        {
            return true;
        }

        if (script is BaseInputModule)
        {
            return true;
        }

        if (script.GetComponentInParent<Canvas>(true) != null)
        {
            return true;
        }

        if (IsInsidePauseUI(script.transform))
        {
            return true;
        }

        return false;
    }

    private void PauseAnimators()
    {
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];

            if (animator == null ||
                IsUIObject(animator.transform) ||
                IsInsidePauseUI(animator.transform))
            {
                continue;
            }

            previousAnimatorSpeeds[i] = animator.speed;
            animator.speed = 0f;
        }
    }

    private void ResumeAnimators()
    {
        if (animators == null ||
            previousAnimatorSpeeds == null)
        {
            return;
        }

        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];

            if (animator == null ||
                IsUIObject(animator.transform) ||
                IsInsidePauseUI(animator.transform))
            {
                continue;
            }

            animator.speed = previousAnimatorSpeeds[i];
        }
    }

    private void PausePhysics()
    {
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody2D body = rigidbodies[i];

            if (body == null)
            {
                continue;
            }

            previousRigidbodyStates[i] = body.simulated;
            body.simulated = false;
        }
    }

    private void ResumePhysics()
    {
        if (rigidbodies == null ||
            previousRigidbodyStates == null)
        {
            return;
        }

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody2D body = rigidbodies[i];

            if (body != null)
            {
                body.simulated = previousRigidbodyStates[i];
            }
        }
    }

    private void PauseAudio()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            AudioSource audioSource = audioSources[i];

            if (audioSource == null)
            {
                continue;
            }

            audioWasPlaying[i] = audioSource.isPlaying;

            if (audioWasPlaying[i])
            {
                audioSource.Pause();
            }
        }
    }

    private void ResumeAudio()
    {
        if (audioSources == null ||
            audioWasPlaying == null)
        {
            return;
        }

        for (int i = 0; i < audioSources.Length; i++)
        {
            AudioSource audioSource = audioSources[i];

            if (audioSource != null &&
                audioWasPlaying[i])
            {
                audioSource.UnPause();
            }
        }
    }


    private bool IsUIObject(Transform objectTransform)
    {
        return objectTransform != null &&
               objectTransform.GetComponentInParent<Canvas>(true) != null;
    }

    private bool IsInsidePauseUI(Transform objectTransform)
    {
        if (objectTransform == null)
        {
            return false;
        }

        if (IsInsideObject(objectTransform, fightingGamePauseScreen))
        {
            return true;
        }

        if (IsInsideObject(objectTransform, controlsPanel))
        {
            return true;
        }

        return false;
    }

    private bool IsInsideObject(
        Transform objectTransform,
        GameObject parentObject)
    {
        if (objectTransform == null ||
            parentObject == null)
        {
            return false;
        }

        Transform parentTransform = parentObject.transform;

        return objectTransform == parentTransform ||
               objectTransform.IsChildOf(parentTransform);
    }

    private void SelectMenuItem(Selectable target)
    {
        if (target == null)
        {
            return;
        }

        if (selectionCoroutine != null)
        {
            StopCoroutine(selectionCoroutine);
        }

        selectionCoroutine =
            StartCoroutine(SelectMenuItemNextFrame(target));
    }

    private IEnumerator SelectMenuItemNextFrame(Selectable target)
    {
        yield return null;

        selectionCoroutine = null;

        if (EventSystem.current == null ||
            target == null ||
            !target.gameObject.activeInHierarchy ||
            !target.IsInteractable())
        {
            yield break;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target.gameObject);
    }
}