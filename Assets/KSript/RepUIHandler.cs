using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RepUIHandler : NetworkBehaviour
{
    
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }

    #region ===== CREATE ROOM BLOck
    /// <summary>
    /// ======== creating room
    /// </summary>
    public void RepCreateRoom()
    {
        CmdCreateRoom();
    }

    [Command]
    private void CmdCreateRoom()
    {
        H.klog2($"Server: got room created req from {connectionToClient} of netid {netId}", this.name);
        string ids = H.GenIds();
        //add identity and room id to room list of game manager ins on server
        GameMan.ins.AssignGameRoom_Ser(ids, this.netIdentity);
        connectionToClient.identity.gameObject.GetComponent<PlayerRep>().syncpflag = 1;
        TargetCreateRoom(connectionToClient.identity, ids);
    }

    [TargetRpc]
    private void TargetCreateRoom(NetworkIdentity netids, string ids)
    {
       H.klog($"Target Client: This is the room ids {ids} of netiden {netId}");
       H.klog($"Target Client: This is the netiden {netids.netId} sent from server");
       // add identity and room list to game manager ins on client local machine
       GameMan.ins.AssignGameRoom_Cli(netids, ids);
       GameMan.ins.OpenRoomLobbyUI_Cli(ids, true);
    }
    #endregion

    #region ===== JOIN ROOM BLOCK
    
    /// <summary>
    /// =========== JOINING ROOm WITH ID
    /// </summary>
    /// <param name="roomid"></param>
    public void RepJoinRoom(string roomid)
    {
        CmdJoinRoom(roomid);
    }

    [Command]
    private void CmdJoinRoom(string roomid)
    {
        H.klog($"Server - Request join room with {roomid}");
        if (GameMan.ins.roomlist_ser.ContainsKey(roomid))
        {
            GameMan.Room ro = GameMan.ins.roomlist_ser[roomid];
            //TODO: THIS IS A TEST
            connectionToClient.identity.gameObject.GetComponent<PlayerRep>().syncpname = $"Im {connectionToClient.connectionId}";
            // 1 call for leadership
            TargetOtherJoinRoom(ro.leadernetiden.connectionToClient, connectionToClient.identity);
            //loop over other member and send them rpc that new one join
            for (int i = 0; i < ro.roommember.Count; i++)
            {
                TargetOtherJoinRoom(ro.roommember[i].connectionToClient, connectionToClient.identity);
            }
            
            // creating new list from room list as argument 
            List<NetworkIdentity> idenlist = new List<NetworkIdentity>();
            idenlist.Add(ro.leadernetiden);
            for (int i = 0; i < ro.roommember.Count; i++)
            {
                idenlist.Add(ro.roommember[i]);
            }
            ro.roommember.Add(connectionToClient.identity); // add this iden to server ins of Gameman
            TargetJoinRoom(connectionToClient.identity, roomid, idenlist);
        }
    }

    [TargetRpc]
    private void TargetJoinRoom(NetworkIdentity iden, string roomid , List<NetworkIdentity> idenlist)
    {
        GameMan.ins.AssignGameRoom_Cli(iden, roomid);
        GameMan.ins.OpenRoomLobbyUI_Cli(roomid, false);
        foreach (NetworkIdentity identi in idenlist)
        {
            GameMan.ins.AssignGameRoom_Cli(identi);
            GameMan.ins.AddRoomPlayerUI_Cli(identi);
        }
        
    }

    [TargetRpc]
    private void TargetOtherJoinRoom(NetworkConnection conn, NetworkIdentity iden)
    {
        GameMan.ins.AssignGameRoom_Cli(iden);
        GameMan.ins.AddRoomPlayerUI_Cli(iden);
    }
    #endregion

    #region ====== LEAVE ROOM BLOCK
    
    /// <summary>
    /// ========== leaving room
    /// </summary>
    public void RepLeaveRoom(string roomid)
    {
        CmdLeaveRoom(roomid);
    }

    [Command]
    private void CmdLeaveRoom(string roomid)
    {
        H.klog($"Server got requst to leave room {roomid} of this cli: {connectionToClient.connectionId}");
        if (GameMan.ins.roomlist_ser.ContainsKey(roomid))
        {
            GameMan.Room ro = GameMan.ins.roomlist_ser[roomid];
            // remove the client that request to leave
            if (ro.leadernetiden.netId == connectionToClient.identity.netId)
            {
                if (ro.roommember.Count == 0)
                {
                    GameMan.ins.roomlist_ser.Remove(roomid);
                }
                else
                {
                    foreach (NetworkIdentity identi in ro.roommember)
                    {
                        TargetOtherLeaveRoom(identi.connectionToClient, connectionToClient.identity);
                    }
                    ro.leadernetiden = ro.roommember[0]; // assign the first client in room member as room host
                    //TODO: FIX IT LATER, FOR NOW JUST SET READY AS U BECOME LEADER
                    ro.leadernetiden.gameObject.GetComponent<PlayerRep>().syncpflag = 1; 
                    TargetBecomeLeader(ro.leadernetiden.connectionToClient);
                    ro.roommember.RemoveAt(0);
                }
            }
            else
            {
                ro.roommember.Remove(connectionToClient.identity);
                // sent rpc to room leader
                TargetOtherLeaveRoom(ro.leadernetiden.connectionToClient, connectionToClient.identity);
                for (int i = 0; i < ro.roommember.Count; i++)
                {
                    TargetOtherLeaveRoom(ro.roommember[i].connectionToClient, connectionToClient.identity);
                }
            }
        }
        // THIS will reset player state of gameobject that has this connection and netiden
        connectionToClient.identity.gameObject.GetComponent<PlayerRep>().ResetPlayerState();
    }

    [TargetRpc]
    private void TargetOtherLeaveRoom(NetworkConnection target, NetworkIdentity iden)
    {
        GameMan.ins.RemovePlayer_Cli(iden);
    }

    [TargetRpc]
    private void TargetBecomeLeader(NetworkConnection target)
    {
        GameMan.ins.SetLeaderUI_Cli();
    }
    #endregion

    #region ======== Ready

    public void RepReady()
    {
        CmdReady();
    }

    [Command]
    private void CmdReady()
    {
        PlayerRep go = PlayerRep.RetRepIns(connectionToClient);

        if (go.syncpflag == 0)
        {
            go.syncpflag = 1;
        }
        else
        {
            go.syncpflag = 0;
        }
        TargetReady(go.syncpflag);
        
    }

    [TargetRpc]
    private void TargetReady(int state)
    {
        if (state == 0)
        {
            GameMan.ins.readyButt.SetActive(true);
            GameMan.ins.cancelButt.SetActive(false);
            GameMan.ins.startButt.SetActive(false);
        }
        else
        {
            GameMan.ins.readyButt.SetActive(false);
            GameMan.ins.cancelButt.SetActive(true);
            GameMan.ins.startButt.SetActive(false);
        }
    }

    #endregion

    #region ============= Start Game Block

    public void RepStartGame(string roomids)
    {
        CmdStartGame(roomids);
    }

    [Command]
    private void CmdStartGame(string roomid)
    {

        if (GameMan.ins.roomlist_ser.ContainsKey(roomid))
        {
            GameMan.Room ro = GameMan.ins.roomlist_ser[roomid];
            bool allready = true;
            foreach (NetworkIdentity identi in ro.roommember)
            {
                allready = (allready && Convert.ToBoolean (identi.gameObject.GetComponent<PlayerRep>().syncpflag) );
            }
            if (allready)
            {
                H.klog($"ALL MEMBER READY , LET'S PLAY");
            }
            else
            {
                H.klog($"DUCK, SOME OF THEM ARE COWARD");
            }
        }
    }

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
