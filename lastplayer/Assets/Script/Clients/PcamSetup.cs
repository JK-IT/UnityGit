using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PcamSetup : NetworkBehaviour
{
    [SerializeField]
    Transform _pcam = null;
    [SerializeField]
    GameObject _wcam = null;


    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        _pcam.gameObject.SetActive(true);
        //_wcam.gameObject.SetActive(false);
    }


    // Start is called before the first frame update
    void Start()
    {
        _wcam = GameObject.Find("Main Camera");
        //_wcam.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
