using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Transform feet;

    private Vector3 inputDir;

    public LayerMask groundMask;

    public float Acceleration = 30f;
    public float deceleration = 3f;
    public float maxSpeed = 20f;
    


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {

        if (inputDir != Vector3.zero)
        {
            rb.AddForce(inputDir * Acceleration);

            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }

        }
        else if(inputDir == Vector3.zero)
        {
            rb.AddForce(new Vector3(rb.velocity.x, 0, rb.velocity.z) * -deceleration);
        }

    }
    
    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
    }

}
