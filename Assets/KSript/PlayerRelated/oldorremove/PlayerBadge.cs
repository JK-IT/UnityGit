using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerBadge : MonoBehaviour
{
    public static PlayerBadge ins;
    private PlayerRep prep;
    public int connectedConnid;
    public string pname = "defname";
    public string pflag = "Not Ready";
    public TMP_Text namefield = null;
    public TMP_Text flagfield = null;

    /// <summary>
    /// ----- Get name and flag value of instance of player represent of this connection
    /// ----- if null , assign default value
    /// </summary>
    private void Init()
    {
        //namefield = this.gameObject.transform.Find("Name").GetComponent<TMP_Text>();
        //flagfield = this.gameObject.transform.Find("Flag").GetComponent<TMP_Text>();
        if (prep == null)
        {
            namefield.text = pname;
            namefield.text = pflag;
        }
        else
        {
            namefield.text = prep.syncpname;
            if (prep.syncpflag == 0)
            {
                flagfield.text = "Not Ready";
            }
            else
            {
                flagfield.text = "Ready";
            }
        }
    }

    public void SetPlayerRep(PlayerRep inrep)
    {
        prep = inrep;
        connectedConnid = prep.myConnid;
        Init();
        prep.flagChangedEvn += GotFlagChangedEvn;
        prep.nameChangedEvn += GotNameChangedEvn;
    }

    private void GotFlagChangedEvn(int nint)
    {
        if (nint == 0)
        {
            flagfield.text = "Not Ready";
        }
        else
        {
            flagfield.text = "Ready";
        }
    }

    private void GotNameChangedEvn(string ins)
    {
        namefield.text = ins;
    }

    // Start is called before the first frame update
    void Start()
    {
        // NOTE: it is safe to do this cuz this is local object
        ins = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
