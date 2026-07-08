using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FightingGamePauseManager : MonoBehaviour
{
    [SerializeField] private GameObject fightingGameFolder;

    [SerializeField] private GameObject fightingGamePauseScreen;
    [SerializeField] private GameObject controlsPanel;

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

    private ParticleSystem[] particleSystems;
    private bool[] particleWasPlaying;

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
        if (fightingGameFolder == null ||
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
        if (!IsPaused || controlsPanel == null)
        {
            return;
        }

        controlsPanel.SetActive(true);
        SelectMenuItem(firstControlsSelection);
    }

    public void HideControls()
    {
        if (!IsPaused || controlsPanel == null)
        {
            return;
        }

        controlsPanel.SetActive(false);
        SelectMenuItem(firstPauseSelection);
    }

    private void FindFightingGameComponents()
    {
        scripts =
            fightingGameFolder.GetComponentsInChildren<MonoBehaviour>(true);

        animators =
            fightingGameFolder.GetComponentsInChildren<Animator>(true);

        rigidbodies =
            fightingGameFolder.GetComponentsInChildren<Rigidbody2D>(true);

        audioSources =
            fightingGameFolder.GetComponentsInChildren<AudioSource>(true);

        particleSystems =
            fightingGameFolder.GetComponentsInChildren<ParticleSystem>(true);

        previousScriptStates = new bool[scripts.Length];
        previousAnimatorSpeeds = new float[animators.Length];
        previousRigidbodyStates = new bool[rigidbodies.Length];
        audioWasPlaying = new bool[audioSources.Length];
        particleWasPlaying = new bool[particleSystems.Length];
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
        if (scripts == null)
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

        if (script.GetComponentInParent<Canvas>(true) != null)
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

        if (IsInsidePauseScreen(script.transform))
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
                IsInsidePauseScreen(animator.transform))
            {
                continue;
            }

            previousAnimatorSpeeds[i] = animator.speed;
            animator.speed = 0f;
        }
    }

    private void ResumeAnimators()
    {
        if (animators == null)
        {
            return;
        }

        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];

            if (animator == null ||
                IsUIObject(animator.transform) ||
                IsInsidePauseScreen(animator.transform))
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
        if (rigidbodies == null)
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
        if (audioSources == null)
        {
            return;
        }

        for (int i = 0; i < audioSources.Length; i++)
        {
            AudioSource audioSource = audioSources[i];

            if (audioSource != null && audioWasPlaying[i])
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

    private bool IsInsidePauseScreen(Transform objectTransform)
    {
        if (fightingGamePauseScreen == null ||
            objectTransform == null)
        {
            return false;
        }

        Transform pauseScreenTransform =
            fightingGamePauseScreen.transform;

        return objectTransform == pauseScreenTransform ||
               objectTransform.IsChildOf(pauseScreenTransform);
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