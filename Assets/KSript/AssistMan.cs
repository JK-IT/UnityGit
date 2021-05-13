using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AssistMan : MonoBehaviour
{
    public int cliConnId;
    //* connection to server, which will be assign in OnClientConnect()
    public NetworkConnection conntoserver;

    // this event will be invoke when successful connect to server
    public Action<bool> onCliConnect;
    
    #region ======  Unity Start & Functions

    public static AssistMan _ins;
    private KnetMan _knet;
    // Start is called before the first frame update
    void Start()
    {
        _ins = this;
        _knet = this.gameObject.GetComponent<KnetMan>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion
    
    #region ======== UI related =========

    public void Req_CreateRoom()
    {
        //H.klog2($"I {cliConnId} request server {conntoserver} to create room ", this.name);
        //H.klog($"Name that ask to CCREATE from UI : {PlayerMan._ins.playerdat.GetName()}");
        conntoserver.Send(new Msg_RoomInGeneral{urname =  PlayerMan._ins.playerdat.GetName(), comcode = ComCode.CreateRoom});
    }
    /// <summary>
    ///     CLIENT CALLS THIS
    /// </summary>
    /// <param name="idtojoin"></param>
    public void Req_JoinRoom(string idtojoin)
    {
        //H.klog($"Name When requesting to join from UI : {PlayerMan._ins.playerdat.GetName()}");
        conntoserver.Send(new Msg_JoinRoom{idtojoin = idtojoin, mename =  PlayerMan._ins.playerdat.GetName()});
    }
    /// <summary>
    ///     CLIENT CALLS THIS
    /// </summary>
    /// <param name="roomid"></param>
    public void Req_LeaveRoom(string roomid)
    {
        conntoserver.Send(new Msg_RoomInGeneral{comcode = ComCode.LeaveRoom, roomids =  roomid});
    }

    public void Req_ReadyFlag(string roomid, bool flg)
    {
        conntoserver.Send(new Msg_Lobby{comcode = ComCode.ReadyFlag, roomid = roomid, rdyFlag = flg});
    }

    /// <summary>
    ///     SERVER CALLS THIS
    ///     Function will be called in network manager, when a player disconnect
    ///     Remove disconnected player from the room
    /// </summary>
    /// <param name="conn"></param>
    public void Client_Disconnect_ser(NetworkConnection conn)
    {
        if (GameMan.ins.playerTab_ser.ContainsKey(conn.connectionId))
        {   //get room id from player table
            string roid = GameMan.ins.playerTab_ser[conn.connectionId];
            if (GameMan.ins.roomTab_ser.ContainsKey(roid))
            {
                LeaveAndPromote(roid, conn, new Msg_Lobby{comcode = ComCode.LeaveRoom, playerconnid = conn.connectionId});
            }
            H.klog($"Function Called to remove DISCONNECTED {conn.connectionId} from Game Manager");
            GameMan.ins.playerTab_ser.Remove(conn.connectionId);
        }
    }
    #endregion

    #region ======== MSG RELATED ============

    public void Msgreq_Welcome(NetworkConnection conn, Msg_Welcome msg)
    {
        H.klog($"Server got a msgSer from {conn.connectionId} => {msg.wlcomemsg}");
    }
    
    public void Msgres_Welcome(Msg_Welcome msg)
    {
        H.klog($"Client got a msgSer from Server => {msg.wlcomemsg}");
        AssistMan._ins.cliConnId = msg.cliconnid;
    }
    public void Msgreq_RoomInGeneral(NetworkConnection cliconn, Msg_RoomInGeneral inmsg)
    {
        if (inmsg.comcode == ComCode.CreateRoom)
        {
            //H.klog2($"Client - {cliconn} send me room req, what hsould id o", this.name);
            string id = H.GenIds();
            // set up room list on server, assign this cliconn as leader
            GameMan.ins.SetupRoom_Ser(id, cliconn, inmsg.urname);
            // assign the connection id with room id
            GameMan.ins.playerTab_ser.Add(cliconn.connectionId, id);
            // sending created room id to client
            Msg_RoomInGeneral sendtocli = new Msg_RoomInGeneral() {roomids = id, comcode = ComCode.CreateRoom};
            cliconn.Send(sendtocli);
        } 
        else if (inmsg.comcode == ComCode.LeaveRoom)
        {
            Msg_Lobby resmsg = new Msg_Lobby {comcode = ComCode.LeaveRoom, playerconnid = cliconn.connectionId};
            // send message to all member to remove the one that leave
            //cli who sends this still connect to server
            if (GameMan.ins.roomTab_ser.ContainsKey(inmsg.roomids))
            {
                LeaveAndPromote(inmsg.roomids, cliconn, resmsg);
            }
            // remove player from player table
            H.klog($"remove LEAVING {cliconn.connectionId} from Game Manager");
            if(GameMan.ins.playerTab_ser.ContainsKey(cliconn.connectionId))
                GameMan.ins.playerTab_ser.Remove(cliconn.connectionId);
        }
    }

    public void Msgres_RoomInGenral(Msg_RoomInGeneral sermsg)
    {
        if (sermsg.comcode == ComCode.CreateRoom)
        {
            GameMan.ins.SetupRoom_Cli(sermsg.roomids);
        }
    }
    

    public void Msgreq_JoinRoom(NetworkConnection cliconn, Msg_JoinRoom msg)
    {
        H.klog($"Got req to join from {cliconn.connectionId} , with name {msg.mename}");
        if (GameMan.ins.roomTab_ser.ContainsKey(msg.idtojoin))
        {   //prepare for message to send
            GameMan.Room ro = GameMan.ins.roomTab_ser[msg.idtojoin];
            List<int> memid = new List<int>();
            List<string> memname = new List<string>();
            //addin LEADER to the list
            memid.Add(ro.leader.connectionId);
            memname.Add(ro.nameslist[ro.leader.connectionId]);
            
            H.klog($"Memcount = {ro.members.Count}");
            if (ro.members.Count != 0)
            {   
                for (int i = 0; i < ro.members.Count; i++)
                {   
                    //add conn id to member id list to send to cliconn
                    memid.Add(ro.members[i].connectionId);
                    memname.Add(ro.nameslist[ro.members[i].connectionId]);
                    
                    //send a msg to each member of room, new one join
                    ro.members[i].Send(new Msg_Lobby{playerconnid = cliconn.connectionId, playername = msg.mename, comcode = ComCode.JoinRoom});
                }
            }
            //send message to leader to add new mem
            ro.leader.Send(new Msg_Lobby{playerconnid = cliconn.connectionId, playername = msg.mename, comcode = ComCode.JoinRoom});
            
            H.klog($"Memid count = {memid.Count}");
            Msg_JoinRoom mess = new Msg_JoinRoom {idtojoin = msg.idtojoin, members = memid, roommemnames = memname,memcount  = memid.Count ,error = false};
            cliconn.Send(mess);
            
            // adding cliconn as member on server
            GameMan.ins.AddRoomPlayer_Ser(msg.idtojoin, cliconn, msg.mename);
            GameMan.ins.playerTab_ser.Add(cliconn.connectionId, msg.idtojoin);
        }
    }

    public void Msgres_JoinRoom(Msg_JoinRoom sermsg)
    {
        // open game room and add itself
        GameMan.ins.SetupRoom_Cli(sermsg.idtojoin);
        // add extra lobby player
        if (sermsg.memcount != 0)
        {
            for (int i = 0; i < sermsg.memcount; i++)
            {
                GameMan.ins.AddRoomPlayerUI_Cli(sermsg.members[i], sermsg.roommemnames[i]);
            }
        }
    }
    
    public void Msgreq_Lobby(NetworkConnection cliconn, Msg_Lobby climsg)
    {
        if (climsg.comcode == ComCode.ReadyFlag)
        {
            if (GameMan.ins.roomTab_ser.ContainsKey(climsg.roomid))
            {
                GameMan.Room ro = GameMan.ins.roomTab_ser[climsg.roomid];
                if (ro.readyflags.ContainsKey(cliconn.connectionId))
                    ro.readyflags[cliconn.connectionId] = climsg.rdyFlag;
                else
                    ro.readyflags.Add(cliconn.connectionId, climsg.rdyFlag);

                if (ro.readyflags.Count == 4)
                {
                    //todo: check bool of all flag, then send msg to enable start button on leader machine
                }
            }
            
        }
    }

    public void Msgres_Lobby(Msg_Lobby sermsg)
    {
        if(sermsg.comcode == ComCode.JoinRoom)
            GameMan.ins.AddRoomPlayerUI_Cli(sermsg.playerconnid, sermsg.playername);
        else if(sermsg.comcode == ComCode.LeaveRoom)
        {
            GameMan.ins.RemovePlayer_Cli(sermsg.playerconnid);
        }
    }
    
    #endregion

    #region ============= HELPER FUNCTIONS
    // SERVER CALLS THIS TO REMOVE AND PROMOTE NEW LEADER OR REMOVE ROOM FROM RECORD
    private void LeaveAndPromote(string roomid,NetworkConnection cliconn  ,Msg_Lobby mess)
    {
        if (GameMan.ins.roomTab_ser.ContainsKey(roomid))
        {
            // send msg to all players in the room
            GameMan.Room ro = GameMan.ins.roomTab_ser[roomid];
            if (ro.members.Count == 0)
            {
                // remove the room from room list, cuz no one else left
                GameMan.ins.roomTab_ser.Remove(roomid);
            }
            else
            {
                if (cliconn != ro.leader)
                {
                    ro.leader.Send(mess);
                    ro.members.Remove(cliconn);
                    ro.nameslist.Remove(cliconn.connectionId);
                }
                else
                {
                    // promote the next one to be leader, need to be in different order
                    ro.nameslist.Remove(ro.leader.connectionId); // remove name of conn as leader
                    ro.leader = ro.members[0]; // promote
                    ro.leader.Send(mess);
                    ro.members.Remove(ro.leader);
                }

                // loop over members and send them msg to remove from their local lobby
                for (int i = 0; i < ro.members.Count; i++)
                {
                    ro.members[i].Send(mess);
                }

                // remove flag from flag list
                if (ro.readyflags.ContainsKey(cliconn.connectionId))
                {
                    ro.readyflags.Remove(cliconn.connectionId);
                }
            }
        }
    }
    

    #endregion
    
}
