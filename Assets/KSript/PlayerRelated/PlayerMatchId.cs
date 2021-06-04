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

    public static PlayerMatchId _ins;
    
    #region =========== UNITY CALL BACK REGIONS

    // Start is called before the first frame update
        void Start()
        {
            
        }
    
        // Update is called once per frame
        void Update()
        {
            
        }

    #endregion
    


    #region ============= Function region
    
    public void SetGuid(Guid inguid)
    {
        matchGuid = inguid;
        string_match_guid = inguid.ToString();
    }

    public string GetGuid()
    {
        return string_match_guid;
    }
    

    #endregion
}
