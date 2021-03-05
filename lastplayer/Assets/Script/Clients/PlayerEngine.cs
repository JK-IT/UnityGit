using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

/// <summary>
/// this method is used to get gameobject of InputControler
/// public InputActionAsset inInputAction;
/// InputActionMap map = inInputAction..FindActionMap("nameofthemap");
/// </summary>



public class PlayerEngine : NetworkBehaviour , PlayerControl.IPlayerControlMapsActions
{
    #region IMPLEMENT INTERFACE

    public void OnMoveActions(InputAction.CallbackContext ctx)
    {
        if(base.hasAuthority)
        {
            Debug.Log(ctx.ReadValue<Vector2>());
            Vector2 movedir = ctx.ReadValue<Vector2>();
            if(movedir.magnitude == 0)
            {
                Debug.Log("stop player from moving");
            }
            Move(movedir);

        }
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        Debug.Log("Shoot is triggered " + playerControlMaps.PlayerControlMaps.Shoot.triggered);
    }

    #endregion

    //======================

    #region private variable
    PlayerControl playerControlMaps;
    InputAction movaction;
    InputAction shootaction;
    Rigidbody2D body;
    #endregion

    #region MY METHOD
    private void Move(Vector2 dir)
    {
        body.velocity = (dir * 2f);
    }
    #endregion

    //===========================

    #region behavior call
    private void Awake()
    {
        playerControlMaps = new PlayerControl();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.hasAuthority)
        {
            playerControlMaps.Disable();
            playerControlMaps.PlayerControlMaps.SetCallbacks(null);
        } else
        {
            playerControlMaps.Enable();
            playerControlMaps.PlayerControlMaps.SetCallbacks(this);
        }
    }

    private void OnEnable()
    {
        //playerControlMaps.Enable();
        
        /**
         * you can use this to get action from the action control script
        movaction = playerControlMaps.asset.FindActionMap("PlayerControlMaps").FindAction("MoveActions");
        shootaction = playerControlMaps.asset.FindActionMap("PlayerControlMaps").FindAction("Shoot");
        */

    }

    private void OnDisable()
    {
        playerControlMaps.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        body = this.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#endregion
}
