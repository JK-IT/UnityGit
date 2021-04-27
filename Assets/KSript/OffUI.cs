using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OffUI : MonoBehaviour
{
    private KnetMan netman = null;

    public void StartHost()
    {
        netman.StartHost();
    }

    public void StartServer()
    {
        netman.StartServer();
    }

    public void StartClient()
    {
        netman.StartClient();
    }

    // Start is called before the first frame update
    void Start()
    {
        netman = FindObjectOfType<KnetMan>();
        if (netman == null)
        {
            this.gameObject.GetComponentInChildren<Canvas>().enabled = false;
            Debug.Log(" ----- Cannot find Network Manager Instance ---------- ");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
