using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class whileFalling : PlayerController
{
    public float jumpForce = 8f;
    public bool hasJumped = false;
    public bool onFloor = true;
    public void FixedUpdate()
    {
        if (hasJumped)
        {
            if (onFloor)
            {
                rb.AddForce(jumpForce * player.up, ForceMode.Impulse);
            }
            hasJumped = false;
        }

        if (Physics.CheckSphere(feet.position, 0.1f, groundMask))
        {
            onFloor = true;
        }
        else
        {
            onFloor = false;
        }
    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasJumped = true;
        }
        Debug.Log("hasJumped: " + hasJumped);
        Debug.Log("onFloor: " + onFloor);
    }
}
