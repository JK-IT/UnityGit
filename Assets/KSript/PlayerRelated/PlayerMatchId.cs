using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMatchId : MonoBehaviour
{
    public Guid matchGuid;
    [FormerlySerializedAs("matchGuid")] [SerializeField] public string string_match_guid;
    [SerializeField] public List<int> connidofobserver = new List<int>();

    private NetworkIdentity netidenThisobject;

    public void SetGuid(Guid inguid)
    {
        matchGuid = inguid;
        string_match_guid = inguid.ToString();
    }
    
    public static PlayerMatchId _ins;
    // Start is called before the first frame update
    void Start()
    {
        netidenThisobject = this.gameObject.GetComponent<NetworkIdentity>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    #region ============= EVENT HANDLER region
    

    #endregion
}
