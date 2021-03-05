using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


/// <summary>
/// THIS SCRIPT IS ATTACHED TO THE CHARACTER GAME OBJECT
/// IT WILL THEN INVOKE THE METHOD IN CLIENT INSTANCE WHEN IT HAS AUTHORITY
/// </summary>
public class CharacterSpawnedAnnouncer : NetworkBehaviour
{

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        AnnouncedSpawn();
    }

    private void AnnouncedSpawn()
    {
        
        ClientInstance ci = ClientInstance.ReturnClientIns();
        ci.InvokeCharacterSpawned(gameObject);
    }
}
