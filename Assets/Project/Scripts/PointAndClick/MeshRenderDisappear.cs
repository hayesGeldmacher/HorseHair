using UnityEngine;

public class MeshRenderDisappear : MonoBehaviour
{

    [SerializeField] private MeshRenderer render;
    [SerializeField] private GameObject gObject;

    public void DisableMesh()
    {
        if (render != null)
        {
            render.enabled = false;
            if (gObject != null) { gObject.SetActive(false); }
            Debug.Log("disabled mesh renderer!");
        }
        else 
        {
            Debug.Log("Mesh Renderer Component is not attached in MeshRenderDisappear!");
        
        }
    }

}
