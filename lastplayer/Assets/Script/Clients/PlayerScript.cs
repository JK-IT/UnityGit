using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerScript : NetworkBehaviour
{

    #region private variable
    
    [SerializeField]
    private TextMeshProUGUI _nameinpu;

    [SyncVar(hook =nameof(OnNameUpdated))]
    private string _syncName = "";

    #endregion

    private void OnNameUpdated(string prev, string next)
    {
        _nameinpu.text = next;
    }
    /// <summary>
    /// this is only update for local or client , not on server
    /// </summary>
    /// <param name="name"></param>
    public void PS_SetPlayerName(string name)
    {
        CmdSetName(name);
    }

    [Command]
    private void CmdSetName(string name)
    {
        _syncName = name;
    }

    private void Start()
    {

    }
}
