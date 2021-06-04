using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : NetworkBehaviour
{
    // Start is called before the first frame update
    private Control001 _inputControl;
    private Vector2 movRead;
    void Start()
    {
        _inputControl = new Control001();
        _inputControl.Player.Enable();
        _inputControl.Player.Move.performed += OnMoveInput;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        _inputControl.Player.Disable();
    }

    private void FixedUpdate()
    {
        if(base.hasAuthority)
            Move(movRead);
    }

    private void OnMoveInput(InputAction.CallbackContext ctx)
    {
        //H.klog($"{ctx.ReadValue<Vector2>()}");
        movRead = ctx.ReadValue<Vector2>();
    }

    private void Move(Vector2 dir)
    {
        this.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(dir.x * 40 * Time.fixedDeltaTime
            , dir.y * 40 * Time.fixedDeltaTime);
    }
}
