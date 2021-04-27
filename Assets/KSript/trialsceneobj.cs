using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class trialsceneobj : NetworkBehaviour
{
    [SyncVar(hook = nameof(disthis))] public int ranum = 0;
    private Text tefield;
    public override void OnStartServer()
    {
        H.klog($"CAPSULE OBJ ---- SERVER --- STARTED");
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        H.klog($"CAPSULT OBJ ---- CLIENT ---- STARTED");
        base.OnStartClient();
    }

    public override void OnStartLocalPlayer()
    {
        H.klog($"CAPSULT OBJ ---- LOCAL CLIENT ---- STARTED");
        base.OnStartLocalPlayer();
    }

    public void UIButtClick()
    {
        H.klog($"Button on CAPSULE CLICKED");
    }

    IEnumerator NumChange()
    {
        while (true)
        {
            int num = Random.Range(0, 10000000);
            
            ranum = num;
            yield return new WaitForSeconds(1f);
        }
    }

    private void disthis(int old, int neum)
    {
        if(tefield!= null)
            tefield.text = neum.ToString();
    }
    // Start is called before the first frame update
    void Start()
    {
        tefield = this.GetComponentInChildren<Text>();
        if (isServer)
        {
            H.klog($"Content of text field ON SERVER this {tefield.text}");
            StartCoroutine(NumChange());
        }
        else
        {
            H.klog($"Content of text field ON == CLIENT == this {tefield.text}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
