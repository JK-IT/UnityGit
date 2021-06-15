using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class FabSubNet : NetworkBehaviour
{
    public static FabSubNet _ins;
    
    public Action<string> eventPlayerAdded;
    public  Action<string> eventPlayerRemoved;

    public string clinetFabId;
    
    //* Configuration for internal/custom game server ==> mirror
    //public int gamePort = 7777;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isBatchMode) // this is client
        {
            _ins = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
