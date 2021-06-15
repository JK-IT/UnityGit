using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class AssistMan : MonoBehaviour
{
    [FormerlySerializedAs("cliConnId")] public int myCliConnId;
    //* connection to server, which will be assign in OnClientConnect()
    public NetworkConnection conntoserver;

    // this event will be invoke when successful connect to server
    public Action<bool> onCliConnect;
    
    #region ======  Unity Start & Functions

    public static AssistMan _ins;
    private KnetMan _knet;
    
    public AsyncOperation sceneloading;
    private static int spawn_loc_used = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        H.klog($"Assist Man Starting -- Init assistman instance and register _knet _ins" );
        _ins = this;
        _knet = this.gameObject.GetComponent<KnetMan>();
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (sceneloading != null &&  sceneloading.isDone)
        {
            conntoserver.Send(new Msg_StartGame{doneloading = true, comcode = ComCode.SceneLoading, roomid = GameMan._ins.currentRoomid_cli});
            sceneloading = null;
        }
    }

    #endregion
    
    #region ======== UI related =========

    public void Req_CreateRoom()
    {
        //H.klog2($"I {myCliConnId} request server {conntoserver} to create room ", this.name);
        //H.klog($"Name that ask to CCREATE from UI : {DataMan._ins.playerdat.GetName()}");
        conntoserver.Send(new Msg_RoomInGeneral{urname =  DataMan._ins.playerdat.GetName(), comcode = ComCode.CreateRoom});
    }
    /// <summary>
    ///     CLIENT CALLS THIS
    /// </summary>
    /// <param name="idtojoin"></param>
    public void Req_JoinRoom(string idtojoin)
    {
        //H.klog($"Name When requesting to join from UI : {DataMan._ins.playerdat.GetName()}");
        conntoserver.Send(new Msg_JoinRoom{idtojoin = idtojoin, mename =  DataMan._ins.playerdat.GetName()});
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
    
    public void Req_StartGame(string inid)
    {
        // ask client to load the scene,
        //client will report if the scene done loading
        conntoserver.Send(new Msg_StartGame{roomid = inid, comcode = ComCode.StartGame});
    }

    private void ReadySpawn()
    {
        // set client ready
        if (!NetworkClient.ready)
            NetworkClient.Ready();
        // send to server to spawn player obj
        conntoserver.Send(new Msg_SpawnMe{comCode = ComCode.SpawnMe, roomid = GameMan._ins.currentRoomid_cli});
    }
    /// <summary>
    ///     SERVER CALLS THIS
    ///     Function will be called in network manager, when a player disconnect
    ///     Remove disconnected player from the room
    /// </summary>
    /// <param name="conn"></param>
    public void Client_Disconnect_ser(NetworkConnection conn)
    {
        if (GameMan._ins.playerTab_ser.ContainsKey(conn.connectionId))
        {   //get room id from player table
            string roid = GameMan._ins.playerTab_ser[conn.connectionId];
            if (GameMan._ins.roomTab_ser.ContainsKey(roid))
            {
                LeaveAndPromote(roid, conn, new Msg_Lobby{comcode = ComCode.LeaveRoom, playerconnid = conn.connectionId});
            }
            H.klog($"Function Called to remove DISCONNECTED {conn.connectionId} from Game Manager");
            GameMan._ins.playerTab_ser.Remove(conn.connectionId);
        }
    }
    #endregion

    #region ======== MSG RELATED ============

    public void MsgtoSer_Welcome(NetworkConnection conn, Msg_Welcome msg)
    {
        H.klog1($"Server got a msgSer from {conn.connectionId} => {msg.wlcomemsg}", this.name);
        if(msg.fabid != "")
            _knet.UpdateFabidForConn(conn.connectionId, msg.fabid);
        
    }
    
    public void MsgtoCli_Welcome(Msg_Welcome msg)
    {
        H.klog1($"Client got a msgSer from Server => {msg.wlcomemsg}", this.name);
        AssistMan._ins.myCliConnId = msg.cliconnid;
        // setup conn id on _myIden if this client is login and authenticated
        if(FabAuthenCodeNLogin.authenNverified)
            FabAuthenCodeNLogin._ins.SetCliConnidToIden(msg.cliconnid);
    }
    public void MsgtoSer_RoomInGeneral(NetworkConnection cliconn, Msg_RoomInGeneral inmsg)
    {
        if (inmsg.comcode == ComCode.CreateRoom)
        {
            //H.klog2($"Client - {cliconn} send me room req, what hsould id o", this.name);
            string id = H.GenIds();
            // set up room list on server, assign this cliconn as leader
            GameMan._ins.SetupRoom_Ser(id, cliconn, inmsg.urname);
            // assign the connection id with room id
            GameMan._ins.playerTab_ser.Add(cliconn.connectionId, id);
            // sending created room id to client
            Msg_RoomInGeneral sendtocli = new Msg_RoomInGeneral() {roomids = id, comcode = ComCode.CreateRoom};
            cliconn.Send(sendtocli);
            cliconn.Send(new Msg_Lobby{comcode = ComCode.SetLeader, ureleader = true});
        } 
        else if (inmsg.comcode == ComCode.LeaveRoom)
        {
            Msg_Lobby resmsg = new Msg_Lobby {comcode = ComCode.LeaveRoom, playerconnid = cliconn.connectionId};
            // send message to all member to remove the one that leave
            //cli who sends this still connect to server
            if (GameMan._ins.roomTab_ser.ContainsKey(inmsg.roomids))
            {
                LeaveAndPromote(inmsg.roomids, cliconn, resmsg);
            }
            // remove player from player table
            H.klog($"remove LEAVING {cliconn.connectionId} from Game Manager");
            if(GameMan._ins.playerTab_ser.ContainsKey(cliconn.connectionId))
                GameMan._ins.playerTab_ser.Remove(cliconn.connectionId);
        }
    }

    public void MsgtoCli_RoomInGenral(Msg_RoomInGeneral sermsg)
    {
        if (sermsg.comcode == ComCode.CreateRoom)
        {
            GameMan._ins.SetupRoom_Cli(sermsg.roomids, true);
        }
    }
    

    public void MsgtoSer_JoinRoom(NetworkConnection cliconn, Msg_JoinRoom msg)
    {
        H.klog($"Got req to join from {cliconn.connectionId} , with name {msg.mename}");
        if (GameMan._ins.roomTab_ser.ContainsKey(msg.idtojoin))
        {   //prepare for message to send
            GameMan.Room ro = GameMan._ins.roomTab_ser[msg.idtojoin];
            List<int> memid = new List<int>();
            List<string> memname = new List<string>();
            List<bool> memstate = new List<bool>();
            //addin LEADER to the list
            //memid.Add(ro.leader.connectionId);
            //memname.Add(ro.nameslist[ro.leader.connectionId]);
            
            //H.klog($"Memcount = {ro.members.Count}");
            if (ro.members.Count != 0)
            {   
                for (int i = 0; i < ro.members.Count; i++)
                {   
                    //add conn id to member id list to send to cliconn
                    memid.Add(ro.members[i].connectionId);
                    memname.Add(ro.nameslist[ro.members[i].connectionId]);
                
                    //send a msg to each member of room, new one join
                    ro.members[i].Send(new Msg_Lobby{playerconnid = cliconn.connectionId, playername = msg.mename, comcode = ComCode.JoinRoom});
                    
                // adding ready state to right client conn id
                    if(ro.readyflags.ContainsKey(ro.members[i].connectionId))
                        memstate.Add(ro.readyflags[ro.members[i].connectionId]);
                    else
                        memstate.Add(false);
                }
            }
            //send message to leader to add new mem
            ro.leader.Send(new Msg_Lobby{playerconnid = cliconn.connectionId, playername = msg.mename, comcode = ComCode.JoinRoom});
            //reset start ui button
            ro.leader.Send(new Msg_Lobby{comcode = ComCode.AllReady, enaRoomStart = false});
            //H.klog($"Memid count = {memid.Count}");
            Msg_JoinRoom mess = new Msg_JoinRoom {idtojoin = msg.idtojoin, members = memid, roommemnames = memname,memcount  = memid.Count, memstate = memstate,leaderid = ro.leader.connectionId, leadername = ro.nameslist[ro.leader.connectionId],error = false};
            cliconn.Send(mess);
            
            // adding cliconn as room member on server
            GameMan._ins.AddRoomPlayer_Ser(msg.idtojoin, cliconn, msg.mename);
            //adding player to player table
            GameMan._ins.playerTab_ser.Add(cliconn.connectionId, msg.idtojoin);
            
        }
    }

    public void MsgtoCli_JoinRoom(Msg_JoinRoom sermsg)
    {
        // open game room and add itself
        GameMan._ins.SetupRoom_Cli(sermsg.idtojoin);
        // adding leader badge to the room
        GameMan._ins.AddRoomPlayerUI_Cli(sermsg.leaderid, sermsg.leadername, true);
        // add extra lobby player
        if (sermsg.memcount != 0)
        {
            for (int i = 0; i < sermsg.memcount; i++)
            {
                GameMan._ins.AddRoomPlayerUI_Cli(sermsg.members[i], sermsg.roommemnames[i], false ,sermsg.memstate[i]);
            }
        }
    }
    
    public void MsgtoSer_Lobby(NetworkConnection cliconn, Msg_Lobby climsg)
    {
        if (climsg.comcode == ComCode.ReadyFlag)
        {
            if (GameMan._ins.roomTab_ser.ContainsKey(climsg.roomid))
            {
                GameMan.Room ro = GameMan._ins.roomTab_ser[climsg.roomid];
                if (ro.readyflags.ContainsKey(cliconn.connectionId))
                    ro.readyflags[cliconn.connectionId] = climsg.rdyFlag;
                else
                    ro.readyflags.Add(cliconn.connectionId, climsg.rdyFlag);
                // loop over member in room and update that player's state
                Msg_Lobby messtocli = new Msg_Lobby {playerconnid = cliconn.connectionId, comcode = ComCode.ReadyFlag};
                messtocli.rdyornot = climsg.rdyFlag ? "Ready" : "Not Ready";

                if (ro.leader != cliconn)
                    ro.leader.Send(messtocli);

                for (int i = 0; i < ro.members.Count; i++)
                {
                    ro.members[i].Send(messtocli);
                }
                // in case if leader id exist in ready list, remove it
                if (ro.readyflags.ContainsKey(ro.leader.connectionId))
                    ro.readyflags.Remove(ro.leader.connectionId);
                // enable start button if all ready
                CheckRoomReadyFlags(ro);
            }
        }
    }

    public void MsgtoCli_Lobby(Msg_Lobby sermsg)
    {
        if (sermsg.comcode == ComCode.JoinRoom)
            GameMan._ins.AddRoomPlayerUI_Cli(sermsg.playerconnid, sermsg.playername);
        else if (sermsg.comcode == ComCode.LeaveRoom)
        {
            GameMan._ins.RemovePlayer_Cli(sermsg.playerconnid);
        } 
        else if (sermsg.comcode == ComCode.ReadyFlag)
        {
            GameMan._ins.ChangeReadyUI_Cli(sermsg.playerconnid, sermsg.rdyornot);
        }
        else if (sermsg.comcode == ComCode.AllReady)
        {
            GameMan._ins.EnableStartButton(sermsg.enaRoomStart);
        }
        else if (sermsg.comcode == ComCode.SetLeader)
        {
            GameMan._ins.SetLeaderUI(sermsg.ureleader);
        } 
        else if (sermsg.comcode == ComCode.ChangeLeader)
        {
            GameMan._ins.ChangLeader_Cli(sermsg.playerconnid);
        }
    }

    public void MsgtoSer_StartGame(NetworkConnection cliconn, Msg_StartGame climsg)
    {
        
        if (climsg.comcode == ComCode.StartGame)
        { 
            H.klog($"Server got msg to start game at room {climsg.roomid}");
            if (GameMan._ins.roomTab_ser.ContainsKey(climsg.roomid))
            {
                GameMan.Room ro = GameMan._ins.roomTab_ser[climsg.roomid];
                // sending loading message to leader and all other member
                Msg_StartGame mess = new Msg_StartGame {sceneName = "Scene_001", comcode = ComCode.StartGame, load_scene_command = true};
                ro.leader.Send(mess);
                for (int i = 0; i < ro.members.Count; i++)
                {
                    ro.members[i].Send(mess);
                }
                GameMan._ins.SetupMatchIns(climsg.roomid);
            }
        } 
        else if (climsg.comcode == ComCode.SceneLoading)
        {
            H.klog($"This client {cliconn.connectionId} finishes loading scene");
            if (GameMan._ins.roomTab_ser.ContainsKey(climsg.roomid))
            {
                GameMan.Room ro = GameMan._ins.roomTab_ser[climsg.roomid];
                if (ro.members.Count == 0)
                {
                    ro.leader.Send(new Msg_StartGame{comcode = ComCode.StartGame, rdyNspawn = true});
                    
                }
                else
                {
                    ro.allDoneLoading += 1;
                    if (ro.allDoneLoading == (ro.members.Count + 1) )
                    {
                        ro.allDoneLoading = 0;
                        Msg_StartGame msg = new Msg_StartGame {comcode = ComCode.StartGame, rdyNspawn = true, load_scene_command = false};
                        ro.leader.Send(msg);
                        for (int i= 0; i < ro.members.Count; i++)
                        {
                            ro.members[i].Send(msg);
                        }
                    }
                }
            }       
        }
        
    }

    public void MsgtoCli_StartGame(Msg_StartGame sermsg)
    {
        if (sermsg.comcode == ComCode.StartGame)
        {
            if(sermsg.load_scene_command)
                sceneloading = SceneManager.LoadSceneAsync(sermsg.sceneName);
            else
            {
                if(sermsg.rdyNspawn)
                    ReadySpawn();
            }
        }
    }

    public void MsgtoSer_SpawnMe(NetworkConnection cliconn, Msg_SpawnMe climsg)
    {
        if (climsg.comCode == ComCode.SpawnMe)
        {
            H.klog2($"Server got Msg to Spawn NetClient prefab for {cliconn.connectionId}", this.name, "#2e8b57");
            
            // spawning the client player object
            if (_knet.playerPrefab != null && _knet.playerPrefab.GetComponent<NetworkIdentity>() != null)
            {
                GameObject go;
                //spawn this as child of MATCH INSTANCE GAME OBJECT
                if (GameMan._ins.MatchIns_ser.ContainsKey(climsg.roomid))
                    go = Instantiate(_knet.playerPrefab, GameMan._ins.MatchIns_ser[climsg.roomid].transform);
                else
                {
                    H.klog2($"error spawning this game object with parent", this.name, "#ff0000");
                    go = Instantiate(_knet.playerPrefab);
                }
                
                // assigning guid to game object
                Guid gudi = H.ToGuid(climsg.roomid);
                go.GetComponent<PlayerMatchId>().SetGuid(gudi);
                H.klog($"This is guid for this room id {climsg.roomid} - {gudi.ToString()}");
                go.name = $"{_knet.playerPrefab.name} _ {cliconn.connectionId}";
                
                // changing layer the same as parent
                if (go.transform.parent != null)
                {
                    go.layer = go.transform.parent.gameObject.layer;
                }
                // add to KnetMan ConnBook 
                if (KnetMan.cliBook.ContainsKey(cliconn))
                {
                    KnetMan.cliBook[cliconn] = go;
                }
                else
                {
                    H.klog($"cannot find connection in ConnBook of Knetman => adding new entry");
                    KnetMan.cliBook.Add(cliconn, go);
                }
                NetworkServer.AddPlayerForConnection(cliconn, go);
            }
        }
        else if (climsg.comCode == ComCode.SpawnNCustom)
        {
            H.klog2($"Server got Msg to Spawn Playable obj for {cliconn.connectionId}", this.name, "#2e8b57");
            // using knetMan cliBook to look up for game object
            if (KnetMan.cliBook.ContainsKey(cliconn))
            {
                string guid = KnetMan.cliBook[cliconn].GetComponent<PlayerMatchId>().GetGuid();
                SpawnPlayableObj(cliconn, guid, climsg);
            }
            else
            {
                H.klog2($"Error: Client Connection not Register", this.name, "#ff1f1f");
            }
        }
    }

    public static void MsgtoCli_Maintenance(Msg_Maintenance msg)
    {
        DateTime dt = msg.maintenTime;
        H.klog($"Cli Got Server Time to maintain {dt}");
        // TODO: DISPLAY POPUP TO SHOW THEM TIME SERVER WILL BE MAINTAINED
    }
    
    
    #endregion

    #region ============= HELPER FUNCTIONS
    // SERVER CALLS THIS TO REMOVE AND PROMOTE NEW LEADER OR REMOVE ROOM FROM RECORD
    /// <summary>
    /// 
    /// </summary>
    /// <param name="roomid"></param>
    /// <param name="cliconn"></param>
    /// <param name="mess"> Contains the id of conn that leave</param>
    private void LeaveAndPromote(string roomid,NetworkConnection cliconn  ,Msg_Lobby mess)
    {
        if (GameMan._ins.roomTab_ser.ContainsKey(roomid))
        {
            // send msg to all players in the room
            GameMan.Room ro = GameMan._ins.roomTab_ser[roomid];
            if (ro.members.Count == 0)
            {
                // remove the room from room list, cuz no one else left
                GameMan._ins.roomTab_ser.Remove(roomid);
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
                    // send msg to enable leader ui
                    ro.leader.Send(new Msg_Lobby{comcode = ComCode.SetLeader, ureleader = true});
                    ro.members.Remove(ro.leader);
                    // remove id of new leader from ready flags list
                    if (ro.readyflags.ContainsKey(ro.leader.connectionId))
                    {
                        ro.readyflags.Remove(ro.leader.connectionId);
                    }
                    
                }

                // loop over members and send them msg to remove from their local lobby
                for (int i = 0; i < ro.members.Count; i++)
                {
                    ro.members[i].Send(mess);
                    // send another message to update all with the new leader
                    ro.members[i].Send(new Msg_Lobby{comcode = ComCode.ChangeLeader, playerconnid = ro.leader.connectionId});
                }
                // remove flag from flag list
                if (ro.readyflags.ContainsKey(cliconn.connectionId))
                {
                    ro.readyflags.Remove(cliconn.connectionId);
                    H.klog($"Remove Flag of players that left the room, remaining {ro.readyflags.Count}");
                }
                // check ro ready flags, to enable start button for new leader
                CheckRoomReadyFlags(ro);
            }
        }
    }

    private void CheckRoomReadyFlags(GameMan.Room ro)
    {
        if ((ro.members.Count) == ro.readyflags.Count)
        {
            // check bool of all flags
            // set this to true, cuz if there no room member then leader only should have this button on to start by itself
            bool roomrdy = true;
            for (int i = 0; i < ro.members.Count; i++)
            {
                //if no members click on ready butt => readyflags list is empty, this will complain, should have a guard here
                if (ro.readyflags.Count != 0)
                {
                    if (ro.readyflags.ContainsKey(ro.members[i].connectionId))
                        roomrdy &= ro.readyflags[ro.members[i].connectionId];
                }
                else
                    roomrdy = false; //there are members but no one click ready so default should make it false

            }
            H.klog($"Value of room ready Flag {roomrdy.ToString()}");
            if (roomrdy)
                ro.leader.Send(new Msg_Lobby{comcode = ComCode.AllReady, enaRoomStart = true});
            else
                ro.leader.Send(new Msg_Lobby{comcode = ComCode.AllReady, enaRoomStart = false});
        }
    }

    #endregion

    #region ================   SERVER CALLS FOR NETCLIENT on Server
    
    //---------- > Spawning the playable game object for player
    public void SpawnPlayableObj(NetworkConnection cli, string guid, Msg_SpawnMe inmsg)
    {
        H.klog($" Spawning PLAYABLE OBJECT  for conn {cli.connectionId}");
        GameObject go = Instantiate( _knet.spawnPrefabs.Find(x => x.name.Contains("Ppo")), KnetMan.cliBook[cli].transform.parent);
        
        go.name = $"pp0 {cli.connectionId}";
        go.GetComponent<PlayerMatchId>().SetGuid(Guid.Parse(guid));
        go.GetComponent<PlayerRegulator>().colorhex = inmsg.colorhex;
        go.transform.position = _knet.SpawnLocation[spawn_loc_used++].transform.position;
        
        // changing layer the same as parent
        if (go.transform.parent != null)
        {
            go.layer = go.transform.parent.gameObject.layer;
        }
        
        //spawn_loc_used++;
        spawn_loc_used = (spawn_loc_used == _knet.SpawnLocation.Count) ? 0 : spawn_loc_used;
        NetworkServer.Spawn(go, cli);
    }

    #endregion

    #region =========================== RCP CALLLS
    
    

    #endregion
    
}
