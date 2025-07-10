using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    
    public Transform orientation;
    public Transform player;
    public Transform playerObject;
    public Rigidbody rb;
    public PlayerSystem playerSystem;
    [SerializeField] PlayerStats stats;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void Update()
    {
        Vector3 viewDir = playerObject.position - new Vector3(transform.position.x, playerObject.position.y, transform.position.z);

        orientation.forward = viewDir.normalized;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(inputDir != Vector3.zero && !(playerSystem.GetState() is PlayerSystem.State.Diving) && !(playerSystem.GetState() is PlayerSystem.State.WallSliding) && !playerSystem.isCurrentlyWallJumping && !playerSystem.isCurrentlyBellySliding && !(playerSystem.GetState() is PlayerSystem.State.Bonked))
        {
            playerObject.forward = Vector3.Slerp(playerObject.forward, inputDir.normalized, Time.deltaTime * stats.rotationSpeed);
        }

    }
}
