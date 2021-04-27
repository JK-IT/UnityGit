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
    [Header("Canvases")]
    [SerializeField] public List<GameObject> canvases;
    [SerializeField] public GameObject playerBadge;
    [SerializeField] public TMP_InputField roomidsinput;
    [Header("Interactive Button")]
    [SerializeField] public GameObject readyButt;
    [SerializeField] public GameObject cancelButt;
    [SerializeField] public GameObject startButt;
    [Header("Room Lobby")]
    [SerializeField] private GameObject roomUIContent;
    [Header("Room Members")]
    [SerializeField] private string currentRoomid_cli ;
    [SerializeField] private List<NetworkIdentity> roomMember_cli = new List<NetworkIdentity>();
    public Dictionary<NetworkIdentity, GameObject> badgeGolist_cli = new Dictionary<NetworkIdentity, GameObject>();
    public Dictionary<string, Room> roomlist_ser = new Dictionary<string, Room>();

    #region ============================ UI METHOD

    public void UICreateRoom()
    {
        //H.klog($" create room {prep.connectionToClient}");
        prep.repui.RepCreateRoom();
    }

    public void UIJoinRoom()
    {
        string id = roomidsinput.text;
        roomidsinput.text = "";
        prep.repui.RepJoinRoom(id);
    }

    public void UILeaveRoom()
    {
        prep.repui.RepLeaveRoom(currentRoomid_cli);
        this.roomMember_cli.Clear();
        this.badgeGolist_cli.Clear();
        this.CloseLobbyUI_Cli();
        currentRoomid_cli = "";
    }
    public void UIQuit()
    {
        Application.Quit();
    }

    public void UIReady()
    {
        prep.repui.RepReady();
    }

    public void UIStartGame()
    {
        prep.repui.RepStartGame(currentRoomid_cli);
    }
    
    public void UILoadScene()
    {
        prep.RepLoadScene();
    }
    
    #endregion

    #region ============= INTERNAL METHOD
    
    // this will be called on instance of gameman on server
    /// <summary>
    /// ADDING TO ROOM LIST NEW ROOM WITH ITS ID ON INSTANCE OF GAME MAN ON SERVER
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="netiden"></param>
    public void AssignGameRoom_Ser(string ids, NetworkIdentity netiden)
    {
        Room r = new Room {roomids = ids, leadernetiden = netiden};
        roomlist_ser.Add(ids, r);
    }
    /// <summary>
    /// ASSIGN THE ROOM ID THIS CLIENT IS IN, ADDING MEMBER TO MEMBER LIST
    /// </summary>
    /// <param name="netiden"></param>
    /// <param name="ids"></param>
    public void AssignGameRoom_Cli(NetworkIdentity netiden, string ids = null)
    {
        if(ids != null)
            currentRoomid_cli = (ids); // set room id, this client is in
        roomMember_cli.Add(netiden);
    }
    /// <summary>
    /// SHOW THE ROOM UI DISPLAY
    /// </summary>
    /// <param name="ids"></param>
    public void OpenRoomLobbyUI_Cli(string ids, bool leader)
    {
        canvases[0].SetActive(false); // control panel
        canvases[1].SetActive(true); //game control panel
        canvases[2].SetActive(true); // room panel
        if (leader)
        {
            SetLeaderUI_Cli();
        }
        GameObject.Find("RoomTitle").GetComponent<TMP_Text>().text += ids;
        // NOTE: spawning local game object instance instead
        //prep.SpawnPlayerBadgeObject();
        AddRoomPlayerUI_Cli();
    }
/// <summary>
/// ADDING PLAYER BADGE TO ROOM UI
/// </summary>
/// <param name="playerbadage"></param>
    public void AddRoomPlayerUI_Cli(NetworkIdentity iden = null)
    {
        GameObject badgeobj = (GameObject) Instantiate(playerBadge);
        if(iden == null)
            badgeobj.GetComponent<PlayerBadge>().SetPlayerRep(prep); // set connected prep to this GameMan
        else
        {
            badgeobj.GetComponent<PlayerBadge>().SetPlayerRep(iden.gameObject.GetComponent<PlayerRep>());
            badgeGolist_cli.Add(iden, badgeobj);
        }
        badgeobj.transform.SetParent(roomUIContent.transform);
    }
    
    /// <summary>
    /// ENABLE LEADER INTERFACE FOR THIS CONNECTION
    /// </summary>
    public void SetLeaderUI_Cli()
    {
        readyButt.SetActive(false);
        startButt.SetActive(true);
        //startButt.GetComponent<Button>().interactable = false;
    }
    
    public void CloseLobbyUI_Cli()
    {
        cancelButt.SetActive(false);
        readyButt.SetActive(true);
        startButt.SetActive(false);
        int roomuichildcount = roomUIContent.transform.childCount;
        for (int i = 0; i < roomuichildcount; i++)
        {
            GameObject tgo = roomUIContent.transform.GetChild(i).gameObject;
            Destroy(tgo);
        }
        GameObject.Find("RoomTitle").GetComponent<TMP_Text>().text = "";

        canvases[2].SetActive(false);
        canvases[1].SetActive(false);
        canvases[0].SetActive(true);
    }
    /// <summary>
    /// REMOVE THE IDEN FROM THE LOCAL LIST
    /// REMOVE THE BADGE UI OF THAT IDEN
    /// </summary>
    /// <param name="iden"></param>
    public void RemovePlayer_Cli(NetworkIdentity iden)
    {
        roomMember_cli.Remove(iden);
        H.klog($"Remove player rquest from server - iden {iden.netId}");
        if (badgeGolist_cli.ContainsKey(iden))
        {
            H.klog($"Found iden to remove");
            GameObject go = badgeGolist_cli[iden];
            H.klog($"found Badge go {go}");
            badgeGolist_cli.Remove(iden);
            Destroy(go);
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
        public NetworkIdentity leadernetiden;
        public List<NetworkIdentity> roommember = new List<NetworkIdentity>();
        public bool allReady = false;
        public Room() { }
    }
    

    #endregion
    
    
    // Start is called before the first frame update
    void Start()
    {
        ins = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
