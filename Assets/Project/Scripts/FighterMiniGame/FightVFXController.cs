using System.Collections;
using UnityEngine;

public class FighterVFXController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FightCharacter fightCharacter;
    [SerializeField] private FightRoundManager roundManager;

    [Tooltip("This fighter's own VFX spawn points. Used for Special VFX.")]
    [SerializeField] private FighterVFXSpawnPoints selfVFXPoints;

    [Tooltip("Opponent transform. Used for normal hit / block / grab VFX.")]
    [SerializeField] private Transform opponentTransform;

    [Tooltip("Opponent's VFX spawn points. Used for normal hit / block / grab VFX.")]
    [SerializeField] private FighterVFXSpawnPoints opponentVFXPoints;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject blockVFX;
    [SerializeField] private GameObject grabVFX;
    [SerializeField] private GameObject specialVFX;

    [Header("Normal VFX Scale")]
    [Tooltip("Scale multiplier for normal hit, block, and grab VFX.")]
    [SerializeField] private float normalVFXScale = 2f;

    [Header("Special VFX Scale")]
    [Tooltip("Scale multiplier for special attack VFX.")]
    [SerializeField] private float specialVFXScale = 2f;

    [Header("VFX Cleanup")]
    [SerializeField] private float VFXDestroyDelay = 2f;

    [Header("Special VFX")]
    [Tooltip("Delay before spawning the special attack VFX.")]
    [SerializeField] private float specialVFXDelay = 0f;

    [SerializeField] private bool debugSpecialVFX = false;

    private FighterSuperMeter fighterSuperMeter;
    private int lastSpecialUses = -1;
    private Coroutine specialVFXCoroutine;

    private void Reset()
    {
        fightCharacter = GetComponent<FightCharacter>();
        fighterSuperMeter = GetComponent<FighterSuperMeter>();

        selfVFXPoints = GetComponent<FighterVFXSpawnPoints>();

        if (selfVFXPoints == null)
            selfVFXPoints = GetComponentInChildren<FighterVFXSpawnPoints>();
    }

    private void Awake()
    {
        AssignMissingReferences();
        CacheCurrentSpecialUses();
    }

    private void OnEnable()
    {
        AssignMissingReferences();
        CacheCurrentSpecialUses();

        if (fightCharacter != null)
            fightCharacter.MovePerformed += OnMovePerformed;
    }

    private void OnDisable()
    {
        if (fightCharacter != null)
            fightCharacter.MovePerformed -= OnMovePerformed;

        if (specialVFXCoroutine != null)
        {
            StopCoroutine(specialVFXCoroutine);
            specialVFXCoroutine = null;
        }
    }

    private void AssignMissingReferences()
    {
        if (fightCharacter == null)
            fightCharacter = GetComponent<FightCharacter>();

        if (fighterSuperMeter == null)
            fighterSuperMeter = GetComponent<FighterSuperMeter>();

        if (fighterSuperMeter == null)
            fighterSuperMeter = GetComponentInChildren<FighterSuperMeter>();

        if (selfVFXPoints == null)
            selfVFXPoints = GetComponent<FighterVFXSpawnPoints>();

        if (selfVFXPoints == null)
            selfVFXPoints = GetComponentInChildren<FighterVFXSpawnPoints>();

        if (opponentVFXPoints == null && opponentTransform != null)
        {
            opponentVFXPoints = opponentTransform.GetComponent<FighterVFXSpawnPoints>();

            if (opponentVFXPoints == null)
                opponentVFXPoints = opponentTransform.GetComponentInChildren<FighterVFXSpawnPoints>();
        }

        if (roundManager == null)
            roundManager = FindAnyObjectByType<FightRoundManager>();
    }

    private void CacheCurrentSpecialUses()
    {
        if (fighterSuperMeter == null)
            return;

        lastSpecialUses = fighterSuperMeter.GetCurrentSpecialUses();
    }

    private void OnMovePerformed(FightCharacter attacker, FighterMoveType moveType, FighterMoveResult result)
    {
        AssignMissingReferences();

        if (moveType == FighterMoveType.Special)
        {
            HandleSpecialVFX();
            return;
        }

        if (result == FighterMoveResult.Hit)
        {
            Vector3 spawnPosition = GetOpponentHitSpawnPosition(moveType);
            GameObject prefab = GetHitPrefab(moveType);
            SpawnNormalVFX(prefab, spawnPosition);
        }
        else if (result == FighterMoveResult.Blocked)
        {
            Vector3 spawnPosition = GetOpponentBlockSpawnPosition(moveType);
            SpawnNormalVFX(blockVFX, spawnPosition);
        }
    }

    private void HandleSpecialVFX()
    {
        if (fighterSuperMeter == null)
        {
            if (debugSpecialVFX)
                Debug.LogWarning(gameObject.name + " has no FighterSuperMeter. Special VFX will not spawn.");

            return;
        }

        int currentSpecialUses = fighterSuperMeter.GetCurrentSpecialUses();

        bool superMeterWasSpent = currentSpecialUses < lastSpecialUses;

        if (debugSpecialVFX)
        {
            Debug.Log(
                gameObject.name +
                " Special VFX check. Last Uses: " +
                lastSpecialUses +
                ", Current Uses: " +
                currentSpecialUses +
                ", Was Spent: " +
                superMeterWasSpent
            );
        }

        lastSpecialUses = currentSpecialUses;

        bool tutorialActive =
            roundManager != null && roundManager.IsTutorialPhaseActive;

        if (!superMeterWasSpent && !tutorialActive)
            return;

        StartSpecialVFXSpawn();
    }

    private void StartSpecialVFXSpawn()
    {
        if (specialVFXCoroutine != null)
            StopCoroutine(specialVFXCoroutine);

        specialVFXCoroutine = StartCoroutine(SpawnSpecialVFXAfterDelay());
    }

    private IEnumerator SpawnSpecialVFXAfterDelay()
    {
        if (specialVFXDelay > 0f)
            yield return new WaitForSeconds(specialVFXDelay);

        SpawnSpecialVFX();

        specialVFXCoroutine = null;
    }

    private void SpawnSpecialVFX()
    {
        Vector3 spawnPosition = GetSelfSpecialSpawnPosition();
        SpawnSpecialVFX(specialVFX, spawnPosition);

        if (debugSpecialVFX)
            Debug.Log(gameObject.name + " spawned Special VFX.");
    }

    private Vector3 GetSelfSpecialSpawnPosition()
    {
        if (selfVFXPoints != null)
        {
            Transform point = selfVFXPoints.GetHitPoint(FighterMoveType.Special);
            return point.position;
        }

        return transform.position;
    }

    private Vector3 GetOpponentHitSpawnPosition(FighterMoveType moveType)
    {
        if (opponentVFXPoints != null)
        {
            Transform point = opponentVFXPoints.GetHitPoint(moveType);
            return point.position;
        }

        return GetFallbackOpponentPosition();
    }

    private Vector3 GetOpponentBlockSpawnPosition(FighterMoveType moveType)
    {
        if (opponentVFXPoints != null)
        {
            Transform point = opponentVFXPoints.GetBlockPoint(moveType);
            return point.position;
        }

        return GetFallbackOpponentPosition();
    }

    private GameObject GetHitPrefab(FighterMoveType moveType)
    {
        if (moveType == FighterMoveType.Grab && grabVFX != null)
            return grabVFX;

        return hitVFX;
    }

    private Vector3 GetFallbackOpponentPosition()
    {
        if (opponentTransform != null)
            return opponentTransform.position;

        return transform.position;
    }

    private void SpawnNormalVFX(GameObject prefab, Vector3 spawnPosition)
    {
        if (prefab == null)
            return;

        Quaternion spawnRotation = Quaternion.identity;

        GameObject vfxInstance = Instantiate(prefab, spawnPosition, spawnRotation);
        vfxInstance.transform.localScale *= normalVFXScale;

        Destroy(vfxInstance, VFXDestroyDelay);
    }

    private void SpawnSpecialVFX(GameObject prefab, Vector3 spawnPosition)
    {
        if (prefab == null)
            return;

        Quaternion spawnRotation = Quaternion.identity;

        GameObject vfxInstance = Instantiate(prefab, spawnPosition, spawnRotation);
        vfxInstance.transform.localScale *= specialVFXScale;

        Destroy(vfxInstance, VFXDestroyDelay);
    }
}