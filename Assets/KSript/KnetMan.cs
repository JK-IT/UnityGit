using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Collections.Generic;

/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class KnetMan : NetworkManager
{
    public static Dictionary<NetworkConnection, GameObject> connbook = new Dictionary<NetworkConnection, GameObject>();
    public static KnetMan knetIns;
    
    [Tooltip("Room Player Prefab")] [Header("Room Player Prefab")] [SerializeField]
    public GameObject roomPlayerPrefab;
    //------------------------
    #region Unity Callbacks

    public override void OnValidate()
    {
        base.OnValidate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Awake()
    {
        this.networkAddress = "127.0.0.1";
        base.Awake();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion

    #region Start & Stop

    /// <summary>
    /// Set the frame rate for a headless server.
    /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
    /// </summary>
    public override void ConfigureServerFrameRate()
    {
        base.ConfigureServerFrameRate();
    }

    /// <summary>
    /// called when quitting the application by closing the window / pressing stop in the editor
    /// </summary>
    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// This causes the server to switch scenes and sets the networkSceneName.
    /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
    }

    /// <summary>
    /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    public override void OnServerChangeScene(string newSceneName) { }

    /// <summary>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    public override void OnServerSceneChanged(string sceneName)
    {
        H.klog("SERVER DONE LOADING SCENE");
    }

    /// <summary>
    /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    /// <param name="sceneOperation">Scene operation that's about to happen</param>
    /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    /// <param name="conn">The network connection that the scene change message arrived on.</param>
    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        H.klog("CLIENT DONE LOADING SCENE");
        base.OnClientSceneChanged(conn);
    }

    #endregion

    #region Server System Callbacks

    /// <summary>
    /// Called on the server when a new client connects.
    /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerConnect(NetworkConnection conn)
    {
        connbook.Add(conn, null);
        // sending not ready msg
        NetworkServer.SetClientNotReady(conn);
        Msg_Welcome msgtocli = new Msg_Welcome {wlcomemsg = $"Hey , my friends {conn.connectionId}", cliconnid = conn.connectionId};
        conn.Send(msgtocli);
    }

    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnection conn)
    {
        H.klog($"Client {conn} says IT IS READY!!!! AFTER SCENE FINISH LOADING");
        base.OnServerReady(conn);
    }

    /// <summary>
    /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //base.OnServerAddPlayer(conn);
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";

        connbook[conn] = player;
        
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        H.klog($"Client {conn.connectionId} just disconnect from server");
        AssistMan._ins.Client_Disconnect_ser(conn); // send the disconnected conn to assistman
        connbook.Remove(conn);
        base.OnServerDisconnect(conn);
    }

    #endregion

    #region Client System Callbacks

    /// <summary>
    /// Called on the client when connected to a server.
    /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
    /// </summary>
    /// <param name="conn">Connection to the server.</param>
    public override void OnClientConnect(NetworkConnection serconn)
    {
        H.klog1($"client successful connected to server", this.name);
        //Caching ConntoServer, so we can send msg to server
        AssistMan._ins.conntoserver = serconn;
        AssistMan._ins.onCliConnect?.Invoke(true);
        // We will disable auto ready here --------
    //---       BASE ON CLIENT CONNECT    
        // OnClientConnect by default calls AddPlayer but it should not do
        // that when we have online/offline scenes. so we need the
        // clientLoadedScene flag to prevent it.
        if (!clientLoadedScene)
        {
            // Ready/AddPlayer is usually triggered by a scene load
            // completing. if no scene was loaded, then Ready/AddPlayer it
            // here instead.
    //===>    //***    if (!NetworkClient.ready) NetworkClient.Ready();
            if (autoCreatePlayer)
            {
                NetworkClient.AddPlayer();
            }
        }
        //base.OnClientConnect(conn);
    // ----     END BASE ON CLIENT CONNECT    
    serconn.Send(new Msg_Welcome{wlcomemsg = $"Anal, Im connecting to server"});
    }

    /// <summary>
    /// Called on clients when disconnected from a server.
    /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
    /// </summary>
    /// <param name="conn">Connection to the server.</param>
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        H.klog($"As a client, im disconnecting, try to send msg to server");
        //sending message to server if in a room, with room id
        conn.Send(new Msg_Welcome{wlcomemsg = $"Bye, i am leaving"});
        base.OnClientDisconnect(conn);
    }

    /// <summary>
    /// Called on clients when a servers tells the client it is no longer ready.
    /// <para>This is commonly used when switching scenes.</para>
    /// </summary>
    /// <param name="conn">Connection to the server.</param>
    public override void OnClientNotReady(NetworkConnection conn)
    {
        H.klog2($"conn {conn.connectionId} get not ready msg", this.name, "yellow");
    }

    #endregion

    #region Start & Stop Callbacks

    // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
    // their functionality, users would need override all the versions. Instead these callbacks are invoked
    // from all versions, so users only need to implement this one case.

    /// <summary>
    /// This is invoked when a host is started.
    /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartHost() { }

    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// This is involed inside STARTSERVER(), STARTHOST() => Register Msg here
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer()
    {
        knetIns = this; //set ins of knet to this on server
        NetworkServer.RegisterHandler<Msg_Welcome>(AssistMan._ins.Msgreq_Welcome);
        NetworkServer.RegisterHandler<Msg_RoomInGeneral>(AssistMan._ins.Msgreq_RoomInGeneral);
        NetworkServer.RegisterHandler<Msg_JoinRoom>(AssistMan._ins.Msgreq_JoinRoom);
        NetworkServer.RegisterHandler<Msg_Lobby>(AssistMan._ins.Msgreq_Lobby);
    }
    
    /// <summary>
    /// This is invoked when the client is started.
    /// This is involed inside STARTCLIENT() function => Register Msg here
    /// </summary>
    public override void OnStartClient()
    {
        if (roomPlayerPrefab != null)
        {
            NetworkClient.RegisterPrefab(roomPlayerPrefab);
        }
        NetworkClient.RegisterHandler<Msg_Welcome>(AssistMan._ins.Msgres_Welcome);
        NetworkClient.RegisterHandler<Msg_RoomInGeneral>(AssistMan._ins.Msgres_RoomInGenral);
        NetworkClient.RegisterHandler<Msg_JoinRoom>(AssistMan._ins.Msgres_JoinRoom);
        NetworkClient.RegisterHandler<Msg_Lobby>(AssistMan._ins.Msgres_Lobby);
    }
    
    /// <summary>
    /// This is called when a host is stopped.
    /// </summary>
    public override void OnStopHost() { }

    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// This is called when a client is stopped.
    /// </summary>
    public override void OnStopClient() { }

    #endregion
}
