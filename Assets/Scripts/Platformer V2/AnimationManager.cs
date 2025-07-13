using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class AnimationManager : MonoBehaviour
{
    public Renderer playerRender;
    public PlayerSystem playerSystem;
    [SerializeField] Animator animator;
    public Transform playerRot;
    public Vector3 targetRot;
    private Quaternion targetRotation;
    float x;
    float y;
    float z;
    public float smoothTime = 0.1f;
    public float rotSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rotSpeed = Time.deltaTime / smoothTime;

        Quaternion localOffset = Quaternion.Euler(targetRot);

        targetRotation = playerSystem.transform.rotation * localOffset;

        // Step 3: Smoothly rotate towards it
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed);

        SetAnimatorParams();
        
    }

    

    void SetAnimatorParams()
    {
        if (playerSystem.GetState() is PlayerSystem.State.Grounded)
        {
            playerRender.material.SetColor("_Color", Color.red);
            bool grounded = playerSystem.GetState() is PlayerSystem.State.Grounded;
            animator.SetBool("Grounded", grounded);
            targetRot = Vector3.zero;
            animator.SetBool("Diving", false);
            animator.SetBool("Falling", false);
            animator.SetBool("Jumping", false);
            animator.SetBool("Bonked", false);
            animator.SetBool("PerfectSlideCancel", false);
            animator.SetBool("BellySliding", false);


        }
        else if (playerSystem.GetState() is PlayerSystem.State.Aerial)
        {
            playerRender.material.SetColor("_Color", Color.red);
            bool grounded = playerSystem.GetState() is PlayerSystem.State.Grounded;
            animator.SetBool("Grounded", grounded);
            targetRot = Vector3.zero;
            animator.SetBool("Diving", false);
            if (playerSystem.rb.velocity.y < 0) {
                animator.SetBool("Falling", true);
                animator.SetBool("Jumping", false);
            }
            else
            {
                animator.SetBool("Falling", false);
            }
            animator.SetBool("BellySliding", false);
        }
        else if (playerSystem.GetState() is PlayerSystem.State.Diving)
        {
            
            playerRender.material.SetColor("_Color", Color.yellow);
            animator.SetBool("Grounded", false);
            animator.SetBool("Diving", true);
            animator.SetBool("Jumping", false);
            animator.SetBool("PerfectSlideCancel", false);
            animator.SetBool("BellySliding", false);

            float xRotation = 75 - playerSystem.rb.velocity.y * 2;
            xRotation = Mathf.Clamp(xRotation, 70f, 120f);

            targetRot = new Vector3(xRotation, targetRot.y, targetRot.z);

        }
        else if (playerSystem.GetState() is PlayerSystem.State.BellySlide)
        {
            animator.SetBool("BellySliding", true);
            animator.SetBool("PerfectSlideCancel", false);
            playerRender.material.SetColor("_Color", Color.blue);

            Vector3 right = playerRot.right;
            Vector3 forward = playerRot.forward;

            Vector3 slopeDirX = Vector3.ProjectOnPlane(playerSystem.floorNormal, right);
            float xAngle = Vector3.SignedAngle(Vector3.up, slopeDirX, right);

            Vector3 slopeDirZ = Vector3.ProjectOnPlane(playerSystem.floorNormal, forward);
            float zAngle = Vector3.SignedAngle(Vector3.up, slopeDirZ, forward);

            targetRot = new Vector3(xAngle + 90, 0f, zAngle);



            animator.SetBool("Grounded", true);
            animator.SetBool("Diving", false);
        }
        else if (playerSystem.GetState() is PlayerSystem.State.WallSliding)
        {
            playerRender.material.SetColor("_Color", Color.green);
        }
        else if (playerSystem.GetState() is PlayerSystem.State.WallJump)
        {
            playerRender.material.SetColor("_Color", Color.magenta);
        }
        else if (playerSystem.GetState() is PlayerSystem.State.Bonked)
        {

            playerRender.material.SetColor("_Color", Color.gray);
            targetRot = new Vector3(-80, targetRot.y, targetRot.z);
            animator.SetBool("Diving", false);
            animator.SetBool("Bonked", true);
            Debug.Log("Bonked");
        }
    }
}
