using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameMan : MonoBehaviour
{
    public static GameMan ins = null;
    private static PlayerRep prep = null;

    private string defRoomTitle = "Room ID: ";
    //[SerializeField] public List<GameObject> gameIns;
    
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

    // ---- CLIENT SIDE
    [Header("Room Members")]
    [SerializeField] private string currentRoomid_cli ;
    //[SerializeField] private List<NetworkIdentity> roomMember_cli = new List<NetworkIdentity>();
    public Dictionary<int, GameObject> memberTab_cli = new Dictionary<int, GameObject>();
    
    // --- SERVER SIDE
            // key: roomid, obj: room obj
    public Dictionary<string, Room> roomTab_ser = new Dictionary<string, Room>();
            //key: conn id, obj: room id
    public Dictionary<int, string> playerTab_ser = new Dictionary<int, string>();

    #region ============================ UI METHOD

    public void UIServer()
    {
        KnetMan.singleton.StartServer();
    }

    public void UIClient()
    {
        KnetMan.singleton.StartClient();
    }

    public void UIHost()
    {
        KnetMan.singleton.StartHost();
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
        //this.roomMember_cli.Clear();
        this.CloseLobbyUI_Cli();
        this.memberTab_cli.Clear();
        currentRoomid_cli = "";
    }
    public void UIQuit()
    {
        Application.Quit();
    }

    public void UIReady()
    {
        AssistMan._ins.Req_ReadyFlag(currentRoomid_cli, true);
    }

    public void UIStartGame()
    {
        //prep.repui.RepStartGame(currentRoomid_cli);
    }

    public void UICancelReady()
    {
        AssistMan._ins.Req_ReadyFlag(currentRoomid_cli, false);
    }

    public void UILoadScene()
    {
        prep.RepLoadScene();
    }
    
    #endregion

    #region ============= INTERNAL METHOD

    /// <summary>
    ///     Enable the game control panel
    /// </summary>
    /// <param name="inb"></param>
    public void SetUIonConnect(bool inb)
    {
        uigroups[0].SetActive(false);
        uigroups[1].SetActive(true);
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
    public void SetupRoom_Cli(string ids)
    {
        currentRoomid_cli = ids;
        OpenRoomLobbyUI_Cli(ids);
        AddRoomPlayerUI_Cli();
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
    public void AddRoomPlayerUI_Cli(int connid = -1, string disname = "")
    {
        GameObject badgeobj = (GameObject) Instantiate(playerBadge);
        if (connid != -1)
        {
            memberTab_cli.Add(connid, badgeobj);
            badgeobj.transform.GetChild(1).GetComponent<TMP_Text>().text = disname;
        }
        else
        {
            // get name from local player data scriptable object
            badgeobj.transform.GetChild(1).GetComponent<TMP_Text>().text = PlayerMan._ins.playerdat.GetName();
        }
        badgeobj.transform.SetParent(roomLobbyUI.transform);
    }
    
    /// <summary>
    ///         ENABLE LEADER INTERFACE FOR THIS CONNECTION
    /// </summary>
    public void SetLeaderUI_Cli()
    {
        readyButt.SetActive(false);
        startButt.SetActive(true);
        //startButt.GetComponent<Button>().interactable = false;
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
        //GameObject.Find("RoomTitle").GetComponent<TMP_Text>().text = "";
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
/// <summary>
/// SETTING REFERENCE OF INSTANCE OF PLAYER REPRESENTATIVE LOCALLY
/// </summary>
    public static void SetRep()
    {
        prep = PlayerRep.RetRepIns();
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
        public Dictionary<int, bool> readyflags = new Dictionary<int, bool>();
        public Room() { }
    }
    

    #endregion

    #region ========== Start & Init 
    // Start is called before the first frame update
    void Start()
    {
        ins = this;
        InitUI();
        SetupEventSub();
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

    private void SetupEventSub()
    {
        AssistMan._ins.onCliConnect += SetUIonConnect;
    }
    #endregion

}
