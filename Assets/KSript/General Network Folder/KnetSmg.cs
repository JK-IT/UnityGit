using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct Msg_Welcome : NetworkMessage
{
    public string wlcomemsg;
    public int cliconnid;
    public string fabid;
}

public enum ComCode
{
    CreateRoom,
    LeaveRoom,
    JoinRoom,
    ReadyFlag,
    AllReady,
    SetLeader,
    ChangeLeader,
    StartGame,
    SceneLoading,
    SpawnMe,
    SpawnNCustom
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
    public int leaderid;
    public string leadername;
    public List<bool> memstate;
    public bool error;    
}

public struct Msg_Lobby : NetworkMessage
{
    public ComCode comcode;
    public string roomid;
    
    //info send from server
    public int playerconnid;
    public string playername;
    public string rdyornot;
    public bool ureleader;
    public bool enaRoomStart;
    
    //info send from client
    public bool rdyFlag;
}

public struct Msg_StartGame : NetworkMessage
{
    //general info
    public string roomid;
    public ComCode comcode;

    //info to cli
    public string sceneName;
    public bool rdyNspawn;
    public bool doneloading;
    public bool load_scene_command;
}


public struct Msg_SpawnMe : NetworkMessage
{
    //general
    public ComCode comCode;
    public string roomid;
    
    public Vector3 position;
    public string colorhex;
    
    // info to server : character customization
    // info to client:...

}

public struct Msg_Maintenance : NetworkMessage
{
    public DateTime maintenTime;
}

public static class DateTimeReadWrite
{
    public static void WriteDateTime(this NetworkWriter writer, DateTime val)
    {
        string str = JsonUtility.ToJson(val);
        writer.Write(str);
    }

    public static DateTime ReadDateTime(this NetworkReader reader)
    {
        string str = reader.ReadString();
        return (DateTime)JsonUtility.FromJson<DateTime>(str);
    }
}