using UnityEngine;

public class BackGroundLerpEffect : MonoBehaviour
{

    /// <summary>
    /// Test for a dreamlike effect where the background lazily lerps to follow the player
    /// </summary>

    [Header("References")]
    [SerializeField] private Transform backgroundTransform;
    [SerializeField] private Transform followTarget;

    [Header("Follow Axes")]
    [SerializeField] private bool followX;
    [SerializeField] private bool followY;
    [SerializeField] private bool followZ;

    [Header("")]
    [SerializeField] private float followSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
