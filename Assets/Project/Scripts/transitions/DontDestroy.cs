using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DontDestroy : MonoBehaviour
{
   void Awake()
    {
        GameObject[] audioManagers = GameObject.FindGameObjectsWithTag("audioManager");

        if(audioManagers.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
}
