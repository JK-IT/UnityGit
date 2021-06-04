using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameMan : MonoBehaviour
{

    #region ================== VARIABLE

    public static GameMan ins = null;
    private static PlayerRep prep = null;

    private string defRoomTitle = "Room ID: ";
    private GameObject ownRoomLobbyObj;
    
    [Header("UI Group")]
    [SerializeField] public List<GameObject> uigroups;
    [SerializeField] public TMP_InputField roomidsinput;
    
    [Header("Interactive Button")]
    [SerializeField] public GameObject readyButt;
    [SerializeField] public GameObject cancelButt;
    [SerializeField] public GameObject startButt;
    
    [Header("Room Lobby")]
    [SerializeField] private GameObject roomLobbyUI;
    [SerializeField] private GameObject playerBadge;
    
    [Header("Game Room Prefab")][SerializeField] private GameObject GameRoomPrefab;

    // ---- CLIENT SIDE
    [Header("Room Members")]
    [SerializeField] public string currentRoomid_cli ;
    //[SerializeField] private List<NetworkIdentity> roomMember_cli = new List<NetworkIdentity>();
    public Dictionary<int, GameObject> memberTab_cli = new Dictionary<int, GameObject>();
    
    // --- SERVER SIDE
            // key: roomid, obj: room obj
    public Dictionary<string, Room> roomTab_ser = new Dictionary<string, Room>();
            //key: conn id, obj: room id
    public Dictionary<int, string> playerTab_ser = new Dictionary<int, string>();

    private const int MAXROOMCAPACITY = 5;
    public SortedList<string, GameObject> MatchIns_ser = new SortedList<string, GameObject>();
    

    #endregion

    #region ============================ UI METHOD

    public void UIServer()
    {
        KnetMan.singleton.StartServer();
        //SetUIonConnect();
    }

    public void UIClient()
    {
        KnetMan.singleton.StartClient();
        SetUIonConnect();
    }

    public void UIHost()
    {
        KnetMan.singleton.StartHost();
        SetUIonConnect();
    }
    public void UICreateRoom()
    {
        AssistMan._ins.Req_CreateRoom();
    }

    public void UIJoinRoom()
    {
        string id = roomidsinput.text;
        roomidsinput.text = "";
        AssistMan._ins.Req_JoinRoom(id);
    }

    public void UILeaveRoom()
    {
        AssistMan._ins.Req_LeaveRoom(currentRoomid_cli);
        this.CloseLobbyUI_Cli(); // clear all room lobby member
        this.memberTab_cli.Clear();
        currentRoomid_cli = "";
        // todo: fix this later, cuz u don't need it in released version
        //clear out the start button , if this client is a leader earlier
        ResetGameRoomUI();
    }
    public void UIQuit()
    {
        Application.Quit();
    }

    public void UIReady()
    {
        AssistMan._ins.Req_ReadyFlag(currentRoomid_cli, true);
        // -1 as local badge
        memberTab_cli[-1].transform.GetChild(2).GetComponent<TMP_Text>().text = "Ready";
    }

    public void UIStartGame()
    {
        H.klog($"Server will start the game");
        AssistMan._ins.Req_StartGame(currentRoomid_cli);
    }

    public void UICancelReady()
    {
        AssistMan._ins.Req_ReadyFlag(currentRoomid_cli, false);
        // -1 as local badge
        memberTab_cli[-1].transform.GetChild(2).GetComponent<TMP_Text>().text = "Not Ready";
    }
    
    #endregion

    #region ============= INTERNAL METHOD

    /// <summary>
    ///         ENABLE LEADER UI
    /// </summary>
    /// <param name="enordis"></param>
    public void SetLeaderUI(bool enordis)
    {
        if (enordis)
        {
            startButt.SetActive(true);
            if (memberTab_cli.Count == 1)
                startButt.GetComponent<Button>().interactable = true;
            else
                startButt.GetComponent<Button>().interactable = false;
            readyButt.SetActive(false);
            // set up difference text for player badge
            memberTab_cli[-1].transform.GetChild(2).GetComponent<TMP_Text>().text = "Leader";
        }
        else
        {
            ResetGameRoomUI();
        }
    }

    public void EnableStartButton(bool enaornot)
    {
        if (enaornot)
            startButt.GetComponent<Button>().interactable = true;
        else
            startButt.GetComponent<Button>().interactable = false;
    }

    public void ResetGameRoomUI()
    {
        startButt.SetActive(false);
        readyButt.SetActive(true);
    }
    
    public void ChangLeader_Cli(int leaderid)
    {
        if (memberTab_cli.ContainsKey(leaderid))
        {
            memberTab_cli[leaderid].transform.GetChild(2).GetComponent<TMP_Text>().text = "Leader";
        }
    }
    
    /// <summary>
    ///     Enable the game control panel
    /// </summary>
    /// <param name="inb"></param>
    public void SetUIonConnect(bool inb = false)
    {
        if (KnetMan.singleton.isNetworkActive)
        {
            uigroups[0].SetActive(false);
            uigroups[1].SetActive(true);
        }
    }
    
    // this will be called on instance of gameman on server
    /// <summary>
    /// ADDING TO ROOM LIST NEW ROOM WITH ITS ID ON INSTANCE OF GAME MAN ON SERVER
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="conn"></param>
    public void SetupRoom_Ser(string ids,NetworkConnection conn, string leadname)
    {
        Room r = new Room {roomids = ids, leader = conn};
        r.nameslist.Add(conn.connectionId, leadname);
        roomTab_ser.Add(ids, r);
    }

    /// <summary>
    ///             ADDING PLAYERS CONN TO ROOM LIST ON SERVER
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="conn"></param>
    public void AddRoomPlayer_Ser(string ids, NetworkConnection conn, string pname)
    {
        if (roomTab_ser.ContainsKey(ids))
        {
            roomTab_ser[ids].members.Add(conn);
            roomTab_ser[ids].nameslist.Add(conn.connectionId, pname);
        }
    }
    
    /// <summary>
    ///             ASSIGN THE ROOM ID to THIS CLIENT
    ///             OPEN THE LOBBY AND ADD CLI ITSELF
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="setleader"> enable leader features if the client request creating a room</param>
    public void SetupRoom_Cli(string ids, bool setleader = false)
    {
        currentRoomid_cli = ids;
        OpenRoomLobbyUI_Cli(ids);
        if(setleader)
            AddRoomPlayerUI_Cli(-1, "", true); // add self as leader
        else
            AddRoomPlayerUI_Cli(); // add self as normal player
    }
    
    /// <summary>
    ///         SHOW THE ROOM UI DISPLAY
    /// </summary>
    /// <param name="ids"></param>
    public void OpenRoomLobbyUI_Cli(string ids)
    {
        //TODO: REDO THE OPEN ROOM LOBBY UI
        //uigroups[1].SetActive(false); // connecting panel
        roomLobbyUI.SetActive(true); //room panel
        // set room id text
        roomLobbyUI.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = $"Room: {ids}";
        //GameObject.Find("RoomTitle").GetComponent<TMP_Text>().text += ids;
        // NOTE: spawning local game object instance instead
        //prep.SpawnPlayerBadgeObject();
        //AddRoomPlayerUI_Cli();
    }
    
/// <summary>
///             ADDING PLAYER BADGE TO ROOM UI
/// </summary>
/// <param name="playerbadage"></param>
    public void AddRoomPlayerUI_Cli(int connid = -1, string disname = "", bool isleader = false, bool rdystate = false)
    {
        GameObject badgeobj = (GameObject) Instantiate(playerBadge);
        if (connid != -1)
        {
            badgeobj.transform.GetChild(1).GetComponent<TMP_Text>().text = disname;
        }
        else
        {
            badgeobj.transform.GetChild(1).GetComponent<TMP_Text>().text = DataMan._ins.playerdat.GetName();
        }

        if (isleader)
            badgeobj.transform.GetChild(2).GetComponent<TMP_Text>().text = "Leader";

        if (rdystate)
            badgeobj.transform.GetChild(2).GetComponent<TMP_Text>().text = "Ready";
        memberTab_cli.Add(connid, badgeobj);
        badgeobj.transform.SetParent(roomLobbyUI.transform);
    }
    
    /// <summary>
    ///         SET READY FOR PLAYERS IN THE ROOM
    /// </summary>
    public void ChangeReadyUI_Cli(int playerconnid, string rdyornot)
    {
        if (memberTab_cli.ContainsKey(playerconnid))
        {
            memberTab_cli[playerconnid].transform.GetChild(2).GetComponent<TMP_Text>().text = rdyornot;
        }
    }
    
    /// <summary>
    ///     Remove Room Lobby UI on leaving or disconnect
    /// </summary>
    public void CloseLobbyUI_Cli()
    {
        int roomuichildcount = roomLobbyUI.transform.childCount;
        // skip the title child element
        for (int i = 1; i < roomuichildcount; i++)
        {
            GameObject tgo = roomLobbyUI.transform.GetChild(i).gameObject;
            Destroy(tgo);
        }
        roomLobbyUI.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = $"";

    }
    
    /// <summary>
    /// REMOVE THE IDEN FROM THE LOCAL LIST
    /// REMOVE THE BADGE UI OF THAT IDEN
    /// </summary>
    /// <param name="iden"></param>
    public void RemovePlayer_Cli(int connid)
    {
        if (memberTab_cli.ContainsKey(connid))
        {
            GameObject playerbadge = memberTab_cli[connid];
            memberTab_cli[connid] = null;
            Destroy(playerbadge);
            memberTab_cli.Remove(connid);
        }
    }
    
    // -------------------- > > > > Where game starts

    /// <summary>
    /// create a game object with a layer, so that they will not collide on server,
    /// eventho they do not see each other, but the gameobject still there
    /// </summary>
    /// <param name="matchid"></param>
    public void SetupMatchIns(string matchid)
    {
        //H.klog2($"is gameroom prefab null {GameRoomPrefab == null}", this.name, "#ff0000");
        if (GameRoomPrefab != null)
        {
            //H.klog2($"Spawing Match instance with tag", this.name, "#ff0000");
            GameObject go = Instantiate(GameRoomPrefab);
            int avaiPos= 0;

            if (MatchIns_ser.Count < MAXROOMCAPACITY)
            {
                avaiPos = MatchIns_ser.Count;
                go.layer = avaiPos + 6;
                go.name = matchid;
                MatchIns_ser.Add(matchid, go);
                
                // set ignore to other layer
                for (int i = 0; i < 5; i++)
                {
                    if(go.layer == (i + 6))
                        continue;
                    Physics2D.IgnoreLayerCollision(go.layer, i + 6, true); 
                }
                
                // loop over and spawn scene object, cannot preset with the above game object
                // cuz it will generate different asset id, which is != registered asset id on client
                for (int v = 1; v < KnetMan.singleton.spawnPrefabs.Count; v++)
                {
                     GameObject sob = Instantiate(KnetMan.singleton.spawnPrefabs[v], go.transform);
                    sob.layer = go.layer;
                    sob.GetComponent<PlayerMatchId>().SetGuid(H.ToGuid(matchid));
                    NetworkServer.Spawn(sob);
                }
            }
        }
    }
    
    #endregion

    #region =============== Room Class
    public class Room
    {
        public string roomids;
        //TODO : REMOVE OR MODIFY TO FIT NEW DESIGN
        /*public NetworkIdentity leadernetiden;
        public List<NetworkIdentity> roommember = new List<NetworkIdentity>();
        public bool allReady = false;*/
        //----

        public NetworkConnection leader;
        public List<NetworkConnection> members = new List<NetworkConnection>();
        public Dictionary<int, string> nameslist = new Dictionary<int, string>();
        //only != 0 , if members click ready button, else it will always = 0
        public Dictionary<int, bool> readyflags = new Dictionary<int, bool>();
        public int allDoneLoading = 0;
        public Room() { }
    }
    

    #endregion

    #region ========== Start & Init Unity Callback
    // Start is called before the first frame update
    void Start()
    {
        H.klog($"Game Manager Starting");
        ins = this;
        DontDestroyOnLoad(ins);
        InitUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitUI()
    {
        uigroups[0].SetActive(true);
        uigroups[1].SetActive(false);
    }
    #endregion

}
