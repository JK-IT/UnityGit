using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerRep : NetworkBehaviour
{
    private static PlayerRep ins;
    private AsyncOperation sceneloadasync = null;

    //TODO: FIX REPUI HANDLER
    //public RepUIHandler repui;
    public Action<int> flagChangedEvn;
    public Action<string> nameChangedEvn;
    #region ============ SYNC VAR OF ANY TYPE

    [SyncVar(hook = "ChangeName")] public string syncpname;
    [SyncVar(hook = "ChangeFlag")] public int syncpflag;
    #endregion

    /// <summary>
    /// This is for SYNCVAR TO CALL WHEN CHANGING FLAG OF PLAYER
    /// </summary>
    /// <param name="old"></param>
    /// <param name="nes"></param>
    private void ChangeFlag(int old, int nint)
    {
        flagChangedEvn?.Invoke(nint);
    }
    // this is for SYNCVAR when chainging name of player
    private void ChangeName(string old, string nes)
    {
        nameChangedEvn?.Invoke(nes);
    }

    /// <summary>
    /// RESET PLAYER STATE WHEN PLAYER LEAVE ROOM OR GAME
    /// RESET SYNCVAR
    /// </summary>
    public void ResetPlayerState()
    {
        syncpflag = 0;
    }
    
    #region =========  Behaviour Call BAck

    /// <summary>
    /// Get my Connection id from server and store it
    /// cannot get it from here from network behaviour
    /// so we will store it ourself
    /// </summary>
    public int myConnid;
    [Command]
    private void CmdGetmyConnId()
    {
        H.klog($"Server get conn id req from {connectionToClient}");
        TargetmyConnId(connectionToClient.connectionId);
    }

    [TargetRpc]
    private void TargetmyConnId(int connid)
    {
        PlayerRep.ins.myConnid = connid;
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        syncpname = "Sync pname--";
        syncpflag = 0;
    }

    public override void OnStartAuthority()
    {
        ins = this;
        //repui = this.gameObject.GetComponent<RepUIHandler>();
        CmdGetmyConnId();
        base.OnStartAuthority();
    }

    public override void OnStartLocalPlayer()
    {
        //GameMan.SetRep(); // set reference of Player Rep of GameMan 
        base.OnStartLocalPlayer();
    }

    public override void OnStartClient()
    {
        //get my connectionid
        
    }

    #endregion
    
    /// <summary>
    /// Return Player Represent INSTANCE
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    public static PlayerRep RetRepIns (NetworkConnection conn = null)
    {
        if (NetworkServer.active && conn != null)
        {
            GameObject go = null;
            if (KnetMan.connbook.TryGetValue(conn, out go))
            {
                ins = go.GetComponent<PlayerRep>();
                return ins;
            }
            return null;
        }
        else
        {
            return ins;
        }
    }

    #region =========   Player Rep Only Method
    

    /*
/// <summary>
/// SPAWNING PLAYER BADGE OBJECT FOR ROOM UI
/// </summary>
    public void SpawnPlayerBadgeObject(  )
    {
        CmdSpawnPlayerBadgeObject();
    }

    [Command]
    private void CmdSpawnPlayerBadgeObject()
    {
        GameObject ago =(GameObject) Instantiate(KnetMan.knetIns.roomPlayerPrefab);
        //GameObject gso = Instantiate(KnetMan.singleton.spawnPrefabs[0]);
        //NetworkServer.Spawn(gso);
        NetworkServer.Spawn(ago , connectionToClient);
        TargetSpawnPlayerBadgeObject(ago);
    }

    [TargetRpc]
    private void TargetSpawnPlayerBadgeObject(GameObject pbadge)
    {
        if (pbadge != null)
        {
            H.klog($"Client - pbadge is {pbadge.name}");
            GameMan.ins.AddRoomPlayerUI_Cli(pbadge);
        }
    }
*/
    
    
    
    #endregion
    
    
    
    /// <summary>
    /// ======= loading additive scene
    /// </summary>
    public void RepLoadScene()
    {
        CmdLoadScene();
    }

    [Command]
    private void CmdLoadScene()
    {
        TargetLoadScene(connectionToClient);
    }

    [TargetRpc]
    private void TargetLoadScene(NetworkConnection conn)
    {
       sceneloadasync = SceneManager.LoadSceneAsync("Addictive", LoadSceneMode.Additive);
       
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (sceneloadasync != null && sceneloadasync.isDone)
        {
            H.klog1($"Scene loading is done", this.name);
            sceneloadasync.allowSceneActivation = true;
            sceneloadasync = null;
            //StartCoroutine(UndoSceneLoading());
        }
    }

    IEnumerator UndoSceneLoading()
    {
        yield return new WaitForSeconds(1);
        SceneManager.UnloadSceneAsync("Addictive");
        yield return null;
    }
}
