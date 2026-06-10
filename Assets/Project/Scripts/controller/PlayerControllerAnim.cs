using UnityEngine;

public class PlayerControllerAnim : MonoBehaviour
{

    [Header("movement fields")]
    [SerializeField] private float horizontal;
    [SerializeField] private float vertical;


    [Header("animation fields")]
    [SerializeField] private Animator controllerAnim;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InputUpdate();
        AnimationUpdate();
    }

    private void InputUpdate()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    private void AnimationUpdate()
    {
        controllerAnim.SetFloat("horizontal", horizontal);
        controllerAnim.SetFloat("vertical", vertical);
    }
}
