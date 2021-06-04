using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/**
 * NETWORK BEHAVIOUR ORDER CALL
 * --- ON START AUTHORITY >> SET AUTHORITY
 * --- ON START CLIENT >> SET CLIENT
 * --- ON START LOCAL PLAYER >> SET NETWORK CONNECTION TO SERVER
 */



public class NetClient : NetworkBehaviour
{
    public static NetClient _ins;
    
    #region ============ Unity and K Callback

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    #endregion

    #region ==========  ===== NETWORK BEHAVIOUR region

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        H.klog1($"Do I has Authority ? {hasAuthority} - and as client? {isClient}", this.name);
        /*
        try
        {   
            H.klog($"Am I has Authority ? {hasAuthority} - and as client? {isClient}");
            H.klog($"SErver connection {connectionToServer == null}");
            H.klog($"Client connection {connectionToClient == null}");
            H.klog($"NetworkClient.Connection {NetworkClient.connection}");
        }
        catch (Exception e)
        {
            Debug.LogError("Exception in OnStartAuthority:" + e.Message + " " + e.StackTrace);
        }
        */
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
    }

    public override void OnStartLocalPlayer() //=> this is when CONNECTION TO SERVER is set
    {
        _ins = this;
        base.OnStartLocalPlayer();
        if (hasAuthority && isClient)
        {
            //H.klog($"Connection to server -On Start Local Player- {connectionToServer}");
            // Spawning msg will be sent here
            Msg_SpawnMe msg = new Msg_SpawnMe {comCode = ComCode.SpawnNCustom, colorhex = DataMan._ins.GetColor()};
            base.connectionToServer.Send(msg);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        H.klog1($"Am i client ? {isClient}", this.name);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    public override void OnStartServer()
    {
        //H.klog($"NetClient is OnStarServer, which is called on server and host");
        base.OnStartServer();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
    }

    #endregion

    #region ===================  NetClient FUNCTIONs


    #endregion
}
