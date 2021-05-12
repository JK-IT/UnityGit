using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct Msg_Welcome : NetworkMessage
{
    public string wlcomemsg;
    public int cliconnid;
}

public enum ComCode
{
    CreateRoom,
    LeaveRoom,
    JoinRoom
}


public struct Msg_RoomInGeneral : NetworkMessage
{
    public ComCode comcode;
    public string urname;
    public string roomids;
    public bool error;
}

public struct Msg_JoinRoom : NetworkMessage
{
    //info send from cli
    public string idtojoin;
    public string mename;
    // info send from server
    public List<int> members;
    public int memcount;
    public List<string> roommemnames;
    public bool error;    
}

public struct Msg_Lobby : NetworkMessage
{
    public ComCode comcode;
    
    //info send from server
    public int playerconnid;
    public string playername;
}
