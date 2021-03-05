using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/// <summary>
/// 
/// </summary>
public class ClientInstance :  NetworkBehaviour
{
    private static ClientInstance k_ins;

    [SerializeField]
    public GameObject playerPreb = null;

    /// <summary>
    ///     Events Dispatched when character of the owner is spawned
    /// </summary>
    public static Action<GameObject> OnOwnersCharacterSpawned;
    public void InvokeCharacterSpawned(GameObject go)
    {
        Debug.Log("Character spawned');");
        OnOwnersCharacterSpawned?.Invoke(go);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        k_ins = this;
        CmdRequestSpawn();
    }

    /// <summary>
    /// Request spawn for character
    /// </summary>

    [Command]
    private void CmdRequestSpawn()
    {
        if(base.isServer)
        {
            NetSpawnPlayer();

        }
    }

   // [Server]
    private void NetSpawnPlayer()
    {
       
        GameObject go = Instantiate(playerPreb, transform.position, Quaternion.identity);
        NetworkServer.Spawn(go, base.connectionToClient);
        
    }

    ///<summary>
    /// return client instance from server
    /// 
    /// </summary>
    public static ClientInstance ReturnClientIns(NetworkConnection conn = null)
    {
        if(NetworkServer.active && conn != null)
        {
            NetworkIdentity localplayer;
            if (K_NetworkMangagerScript.localplayer.TryGetValue(conn, out localplayer))
                return localplayer.GetComponent<ClientInstance>();
            else
                return null;
        } else //if no conn, then it is client
        {
            return k_ins;
        }
        
    }

    
    
}
