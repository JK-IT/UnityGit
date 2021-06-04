using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerRegulator : NetworkBehaviour
{
    public static PlayerRegulator _ins;
    
    #region ================== SYNC VALUE

    [SyncVar]
    public string colorhex;
    

    #endregion

    #region =================== Unity Callback

        // Start is called before the first frame update
        void Start()
        {
            if (isServer)
            {
                H.klog1($"I am spawning on Server, colorhex {colorhex}", this.name);
            }
            else
            {
                H.klog1($"I am spawning on Client, colorhex {colorhex}", this.name);
                if (this.gameObject.GetComponent<SpriteRenderer>() == null)
                {
                    H.klog($"Why i have not body");
                }
                else
                {
                    bool sec = ColorUtility.TryParseHtmlString($"#{colorhex}", out Color c);
                    H.klog($"I got a body with color {c.ToString()} cuz parsing color {sec}");
                    this.gameObject.GetComponent<SpriteRenderer>().color = c;
                }
            }

            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        #endregion

    #region ================ Network Behaviour Section

    public override void OnStartServer()
    {
        base.OnStartServer();
        //H.klog1($"I == start On Server, syncvar color hex = {colorhex}", this.name);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        H.klog2($"Do i have authority? {hasAuthority}", this.name);
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        //H.klog1($"I start On client, synvar color = {colorhex}", this.name);
        try
        {
            Color c;
            //ColorUtility.TryParseHtmlString(colorhex, out c);
            //this.gameObject.GetComponent<SpriteRenderer>().color = c;
        }
        catch (Exception e)
        {
            Debug.LogError("Error - " + e.Message + " " + e.StackTrace);
            throw;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }
    
    /*
     * This will not be called, cuz it is spawn from NETCLIENT
     * This will only be called if you use AddPlayerForConnection
     */
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    #endregion

    
    #region ==================== Help Func Region

    // Hook method to set player color
    void SetPlayerColor(string old, string val)
    {
        H.klog2($"Parsing color from server", this.name, val);
        Color ncolor;
        if (!ColorUtility.TryParseHtmlString(val, out ncolor))
        {
            H.klog2($"Failed to parse color", this.name, ColorUtility.ToHtmlStringRGBA(ncolor));
        }
        this.gameObject.GetComponent<SpriteRenderer>().color = ncolor;
    }

    #endregion
    
    
}
