using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using PlayFab;
using PlayFab.MultiplayerModels;
using PlayFab.MultiplayerAgent.Model;
using ConnectedPlayer = PlayFab.MultiplayerAgent.Model.ConnectedPlayer;


public class FabServer : MonoBehaviour
{
    public static FabServer _ins;
    [SerializeField] public KnetMan knet;
    
    // list of connected player on dedicated host
    // this will be the book that decides the life of game server
    private List<ConnectedPlayer> _fabPlayerBook;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isBatchMode)
        {
            H.klog($"Fab Game Server is starting....");
            StartRemoteServer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartRemoteServer()
    {
        H.klog($"Starting Remote Server on Dead Server");
        PlayFabMultiplayerAgentAPI.Start();
        PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
        PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
        PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnServerShutdown;
        PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;
        PlayFabMultiplayerAgentAPI.IsDebugging = true;
        
        // subcribe to event on server when adding and removing player
        knet.evnPlayerAdded += OnServerAddedPlayer;
        knet.addenv.AddListener(OnServerAddedPlayer); 
        knet.evnPlayerRemoved += OnServerRemovedPlayer;
        knet.removeevn.AddListener(OnServerRemovedPlayer);
        
        StartCoroutine(ReadyForPlayers());
    }

    IEnumerator ReadyForPlayers()
    {
        yield return new WaitForSeconds(.3f);
        PlayFabMultiplayerAgentAPI.ReadyForPlayers();
    }

    private void OnServerActive()
    {
        H.klog($"Far Server is ready to run & now we start game server");
        // start game server
        knet.StartServer();
    }

    private void OnMaintenance(DateTime? NextSchedule)
    {
        H.klog($"Fab Server is about to maintenance {NextSchedule.Value.ToString()}");
        if (KnetMan.cliBook.Count != 0)
        {
            foreach (KeyValuePair<NetworkConnection, GameObject> kv in KnetMan.cliBook)
            {
                kv.Key.Send(new Msg_Maintenance
                {
                    maintenTime = (DateTime)NextSchedule
                });
            }
        }
    }

    private void OnServerShutdown()
    {
        H.klog($"Fab Sever will be shutting down");
        StartShutdown();
    }

    private void OnAgentError(string err)
    {
        H.klog($"Multiplayer Agent is on error {err}");
    }

    private void OnServerAddedPlayer(string infabid)
    {
        H.klog1($"Fab server Got 1 client Connecting -> update connected player", this.name);
        _fabPlayerBook.Add(new ConnectedPlayer(infabid));
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_fabPlayerBook);
    }
    private void OnServerRemovedPlayer(string infabid)
    {
        H.klog1($"A Player just left the server, fab will update and shutdown if neccessary", this.name);
        // look in fab book, if existing then remove
        //foreach (ConnectedPlayer pl in _fabPlayerBook)
        for(int i = 0; i < _fabPlayerBook.Count ; i++)
        {
            ConnectedPlayer pl = _fabPlayerBook[i];
            if (pl.PlayerId.Equals(infabid, StringComparison.OrdinalIgnoreCase))
            {
                _fabPlayerBook.Remove(pl);
                break;
            }
        }
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_fabPlayerBook);
        CheckConnToShutDown();
    }

    private void CheckConnToShutDown()
    {
        if (_fabPlayerBook.Count == 0)
        {
            H.klog1($"No more client ->>> shutdown game server on FAB", this.name);
            StartShutdown();
        }
    }

    private void StartShutdown()
    {
        H.klog1($"Fab Game Server is shutting down", this.name);
        StartCoroutine(ServerShutdown());
    }

    IEnumerator ServerShutdown()
    {
        yield return new WaitForSeconds(3f);
        Application.Quit();
    }
}
