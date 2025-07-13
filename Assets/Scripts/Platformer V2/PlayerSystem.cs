using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;



public class PlayerSystem : MonoBehaviour
{
    #region Declarations
    [SerializeField] PlayerStats stats;
    [SerializeField] Animator animator;
    public Vector2 frameInput;
    bool jumpThisFrame;
    public Rigidbody rb;
    Collider col;
    public Collider wallCollider;
    [SerializeField] Transform player;
    [SerializeField]Transform orientation;
    Vector3 frameDirection;
    Vector3 forceToApplyThisFrame;
    Vector3 horizontalVelocities;
    ConstantForce constantForce;
    public Vector3 groundSlope;
    public CapsuleCollider standingCollider;
    public CapsuleCollider diveCollider;

    #endregion

    #region Updates
    private void Awake()
    {
        
        Application.targetFrameRate = 120;
        if (!player.TryGetComponent<Rigidbody>(out rb)) rb = gameObject.AddComponent<Rigidbody>();
        if (!player.TryGetComponent<Collider>(out col)) col = gameObject.AddComponent<CapsuleCollider>();
        standingCollider.enabled = true;
        diveCollider.enabled = false;
        if (!player.TryGetComponent<ConstantForce>(out constantForce)) constantForce = gameObject.AddComponent<ConstantForce>();
    }

    private void Update()
    {
        if (hasJumped && hasDived)
        {
            Debug.Log("Dash and Jump pressed!");
        }

        GatherInput();
    }
    private void FixedUpdate()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        frameInput = new Vector2(horizontalInput, verticalInput);
        SetFrameData();
        SpeedControl(GetState());
        CalculateDirection();
        WallCheck();
        BonkCheck();
        WallSlideCheck();
        WallJumpCheck();
        JumpCheck();
        BellySlideCheck();
        DiveCheck(GetState());



        //Second Lowest :thumbsup:
        Move(GetState());
        //CleanFrameData always Last
        CleanFrameData();
    }
    #endregion

    #region Move
    public void Move(State state)
    {
        Vector3 newVelocity = Vector3.zero;
        Vector3 movement = orientation.forward * frameInput.y + orientation.right * frameInput.x;
        //var step = frameInput == Vector2.zero ? stats.friction : stats.acceleration;
        float step;
        if(frameInput == Vector2.zero)
        {
            if (GetState() is State.Aerial)
            {
                step = stats.airFriction;
            }
            else if (!isCurrentlyWallJumping)
            {
                step = stats.friction;
            }
            else
            {
                step = stats.wallJumpFriction;
            }
            
        }
        else{
            if (GetState() is State.Aerial)
            {
                step = stats.airAcceleration;
            }
            else
            {
                
                step = stats.acceleration;
            }
            
        }

        step *= Time.fixedDeltaTime;
        if (frameInput != Vector2.zero)
        {
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }
        var xDir = frameInput == Vector2.zero ? frameDirection.x : rb.velocity.normalized.x;
        var zDir = frameInput == Vector2.zero ? frameDirection.z : rb.velocity.normalized.z;
        var extraFallSpeed = endedJumpEarly && rb.velocity.y > 0 ? stats.EarlyJumpEndMult : 1;
        var extraForce = new Vector3(0, -stats.gravity * extraFallSpeed, 0);
        constantForce.force = Vector3.MoveTowards(constantForce.force, extraForce * rb.mass, step * 3f);


        if (forceToApplyThisFrame != Vector3.zero)
        {
            
            rb.AddForce(forceToApplyThisFrame * rb.mass, ForceMode.Impulse);
            return;
        }

        if (state is State.Grounded)
        {
            var speed = Mathf.MoveTowards(rb.velocity.magnitude, stats.MaxSpeed, step);
            var targetVelocity = movement * speed;
            var smoothed = Vector3.MoveTowards(rb.velocity, targetVelocity, step);
            var newSpeed = Mathf.MoveTowards(rb.velocity.magnitude, targetVelocity.magnitude, step);
            var precise = targetVelocity.normalized * newSpeed;
            var slopePoint = Mathf.InverseLerp(0, 0.7f, Mathf.Abs(frameDirection.y));

            Vector3 moveVelocity = Vector3.Lerp(smoothed, precise, slopePoint);
            newVelocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
        }
        else if (state is State.Aerial)
        {
            var speed = Mathf.MoveTowards(rb.velocity.magnitude, stats.MaxSpeed, step * 10f);
            var targetVelocity = movement * speed;
            var smoothed = Vector3.MoveTowards(rb.velocity, targetVelocity, step);
            var newSpeed = Mathf.MoveTowards(rb.velocity.magnitude, targetVelocity.magnitude, step);
            var precise = targetVelocity.normalized * newSpeed;
            var slopePoint = Mathf.InverseLerp(0, 0.7f, Mathf.Abs(frameDirection.y));
            
            
            Vector3 moveVelocity = Vector3.Lerp(smoothed, precise, slopePoint);


            newVelocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
        }
        else if(state is State.Diving)
        {
            
            newVelocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
            
        }
        else if(state is State.WallSliding)
        {
            newVelocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
        }
        else if(state is State.WallJump)
        {
            newVelocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
        }
        else if(state is State.BellySlide)
        {
            newVelocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
        }
        else if(state is State.Bonked)
        {
            newVelocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
        }
            
            rb.velocity = newVelocity;
    }
#endregion

    #region Jump
    bool hasJumped;
    bool endedJumpEarly;
    bool jumpHeld;

    public void JumpCheck()
    {
        if (hasJumped)
        {
            Jump(GetState());
        }
        if(!endedJumpEarly && GetState() is State.Aerial && !jumpHeld)
        {
            endedJumpEarly = true;
        }
    }

    public void Jump(State state)
    {
        
        endedJumpEarly = false;
        if(state is State.Grounded)
        {
            animator.SetBool("Jumping", true);
            AddFrameForce(new Vector3(rb.velocity.x, stats.jumpForce, rb.velocity.z));
        }
    }
    #endregion

    #region Dive
    public bool hasDiveCancelled;
    bool hasDived;
    bool isCurrentlyDiving;
    bool canDive;
    public void DiveCheck(State state)
    {
        if (GroundCheck())
        {
            canDive = true;
        }
        if (hasDiveCancelled)
        {
            isCurrentlyDiving = false;
            DiveCancel();
        }
        if (hasDived && !(GetState() is State.Diving) && canDive)
        {
            canDive = false;
            Dive(GetState());
        }

        
    }  

    public void Dive(State state)
    {
        endedJumpEarly = true;
        isCurrentlyDiving = true;

        if (state is State.Grounded || state is State.Aerial || state is State.WallJump)
        {
            ActivateDiveCollider();
            if (frameInput != Vector2.zero)
            {
                AddFrameForce((orientation.forward * frameInput.normalized.y + orientation.right * frameInput.normalized.x) * stats.divingHorizontalForce + player.up * stats.divingVerticalForce, true);
                player.forward = orientation.forward * frameInput.normalized.y + orientation.right * frameInput.normalized.x;
            }
            else{
                AddFrameForce((player.forward) * stats.divingHorizontalForce + player.up * stats.divingVerticalForce, true);
            }
            
            
        }
        
    }

    public void DiveCancel()
    {
        if (!(GetState() is State.BellySlide)) {
            ActivateStandingCollider();
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            AddFrameForce(player.up * stats.divingCancelJumpForce, true);
        }
    }


    #endregion

    #region Belly Slide
    public bool isCurrentlyBellySliding = false;
    public bool hasBellySlideCancelled;
    float bellySlideStart;
    

    public void BellySlideCheck()
    {
        if (hasBellySlideCancelled)
        {
            isCurrentlyBellySliding = false;
            BellySlideCancel();
        }
        if ((GetState() is State.Diving && GroundCheck()) || isCurrentlyBellySliding)
        {
            if (GetState() is State.Diving && GroundCheck())
            { 
                bellySlideStart = Time.time;
            }
            isCurrentlyDiving = false;
            BellySlide();
        }
    }
    public void BellySlide()
    {
        ActivateDiveCollider();
        isCurrentlyBellySliding = true;
        rb.velocity = new Vector3(rb.velocity.x * 0.96f, rb.velocity.y, rb.velocity.z * 0.96f);
        
    }

    public bool perfectBellySlideCancel;
    public void BellySlideCancel()
    {
        ActivateStandingCollider();
        if (Time.time - bellySlideStart <= stats.bellySlideTiming)
        {
            
            animator.SetBool("PerfectSlideCancel", true);
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            AddFrameForce(player.up * stats.bellySlideCancelJumpForce);
            Debug.Log("Perfect slide cancel!");
            
        }
        else
        {
            animator.SetBool("PerfectSlideCancel", false);
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            AddFrameForce(player.up * stats.bellySlideCancelJumpForce, true);
        }
 
        
    }

    #endregion

    #region Wall Slide
    bool wallAhead;
    Vector3 wallNormal;
    bool isCurrentlyWallSliding;

    public void WallCheck()
    {

        if (rb.velocity.y >= 0)
        {
            wallAhead = Physics.Raycast(transform.position, player.forward, out stats.wallHit, stats.wallCheckDistance, stats.Wall) && (Mathf.Abs(stats.wallHit.normal.x) > 0.8 || Mathf.Abs(stats.wallHit.normal.z) > 0.8);
        }
        else if (GetState() is State.Diving)
        {
            wallAhead = Physics.Raycast(transform.position + new Vector3(0,0.5f,0), player.forward, out stats.wallHit, stats.wallCheckDistance * 4.5f, stats.Wall)
                && (Mathf.Abs(stats.wallHit.normal.x) > 0.8 || Mathf.Abs(stats.wallHit.normal.z) > 0.8);
            Debug.DrawRay(transform.position, stats.wallHit.normal);
            

        }
        else
        {
            wallAhead = Physics.Raycast(transform.position, player.forward, out stats.wallHit, stats.fallingWallCheckDistance, stats.Wall);
        }
            
    }

    public void WallSlide()
    {
        isCurrentlyWallSliding = true;
        wallNormal = stats.wallHit.normal;
        player.forward = -wallNormal;
        AddFrameForce(new Vector3(0, -stats.wallSlideGravity, 0), true);
    }

    public void WallSlideCheck()
    {
        if (rb.velocity.y >= 0 || GroundCheck())
        {
            isCurrentlyWallSliding = false;
        }
        if (GetState() is State.WallSliding)
        {
                WallSlide();
        }
    }

    #endregion

    #region Wall Jump

    bool hasWallJumped;
    public bool isCurrentlyWallJumping;
    public void WallJumpCheck()
    {
        if(rb.velocity.y < 0 && isCurrentlyWallJumping)
        {
            isCurrentlyWallJumping = false;
        }
        if (hasWallJumped)
        {
            WallJump();
        }

        
    }

    public void WallJump()
    {
        wallNormal = stats.wallHit.normal;
        isCurrentlyWallJumping = true;
        
        player.forward = wallNormal;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        AddFrameForce(wallNormal.normalized * stats.wallJumpHorizontalForce + player.up * stats.wallJumpVerticalForce, true);
    }

    #endregion

    #region Bonked
    bool hasBonked;

    public void BonkCheck()
    {
        if (GroundCheck())
        {
            hasBonked = false;
        }
        if (GetState() is State.Bonked)
        {
            hasBonked = true;
            
            Bonk();
        }
    }

    public void Bonk()
    {

        if (isCurrentlyDiving && wallAhead)
        {
            
            ActivateStandingCollider();
            isCurrentlyDiving = false;
            rb.velocity = Vector3.zero;
            AddFrameForce(-player.forward.normalized * stats.bonkDistance + player.up.normalized * stats.bonkVerticalDistance);

        }
        isCurrentlyDiving = false;

    }

    #endregion

    #region States
    public State GetState()
    {
        if ((isCurrentlyDiving && wallAhead) || hasBonked)
        {
            return State.Bonked;
        }
        if (isCurrentlyDiving && !isCurrentlyBellySliding) {
            return State.Diving;
        }
        if (isCurrentlyBellySliding)
        {
            return State.BellySlide;
        }
        if (wallAhead && !(GroundCheck()) && rb.velocity.y < 1 && (frameInput != Vector2.zero || isCurrentlyWallSliding || isCurrentlyWallJumping))
        {
            return State.WallSliding;
        }
        if (isCurrentlyWallJumping == true)
        {
            return State.WallJump;
        }
        if(GroundCheck())
        {
            
            return State.Grounded;
        }
        else if(isCurrentlyDiving != true)
        {
            return State.Aerial;
        }


        return State.Grounded;
    }

    bool floorRay;
    public Vector3 floorNormal;
    public bool GroundCheck()
    {
        Collider[] gc = new Collider[0];

       
        Physics.queriesHitTriggers = false;
        if (standingCollider.enabled)
        {
            gc = Physics.OverlapBox(new Vector3(player.position.x, standingCollider.bounds.min.y, player.position.z), stats.idleColliderSize, transform.parent.rotation, stats.Grounded);

        }
        else if (diveCollider.enabled)
        {
            gc = Physics.OverlapBox(new Vector3(player.position.x, diveCollider.bounds.min.y, player.position.z) + player.forward * 2, stats.diveColliderSize, transform.parent.rotation, stats.Grounded);
            floorRay = Physics.Raycast(new Vector3(player.position.x, diveCollider.bounds.min.y, player.position.z) + player.forward * 1.5f, -player.up, out stats.floorHit, stats.floorCheckDistance, stats.Grounded);
            floorNormal = stats.floorHit.normal;

        }
        
        Physics.queriesHitTriggers = false;

         return gc.Length > 0;
    }


    public enum State
    {
        Grounded,
        Aerial,
        Diving,
        BellySlide,
        WallSliding,
        WallJump,
        Bonked,
    }
    #endregion
    
    #region Inputs
    public void GatherInput()
    {
        //float horizontalInput = Input.GetAxisRaw("Horizontal");
        //float verticalInput = Input.GetAxisRaw("Vertical");
        //frameInput = new Vector2(horizontalInput, verticalInput);
        jumpHeld = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasJumped = true;
        }
        if (Input.GetKeyDown(KeyCode.Space) && GetState() is State.Diving)
        {
            hasDiveCancelled = true;
        }
        if(Input.GetKeyDown(KeyCode.Space) && GetState() is State.BellySlide)
        {
            hasBellySlideCancelled = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && !hasJumped)
        {
            hasDived = true;
        }
        if (Input.GetKeyDown(KeyCode.Space) && ((GetState() is State.WallSliding) || (GetState() is State.WallJump && wallAhead)))
        {
            hasWallJumped = true;
        }

    }
    #endregion

    #region Frame Data
    private void CalculateDirection()
    {
        frameDirection = frameInput.x * transform.right + frameInput.y * transform.forward;
        frameDirection = frameDirection.normalized;
    }

    public void CleanFrameData()
    {
        frameInput = Vector2.zero;
        forceToApplyThisFrame = Vector3.zero;
        hasJumped = false;
        hasDived = false;
        hasWallJumped = false;
        hasDiveCancelled = false;
        hasBellySlideCancelled = false;
        perfectBellySlideCancel = false;
        
    }

    public void AddFrameForce(Vector3 force, bool resetVelocity = false)
    {
        if (resetVelocity) rb.velocity = Vector3.zero;
        forceToApplyThisFrame += force;
    }

    public void SetFrameData()
    {
        horizontalVelocities = new Vector3(rb.velocity.x, 0, rb.velocity.z);
    }
    #endregion

    #region Miscellaneous

    public void SpeedControl(State state)
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (state == State.Diving)
        {
            if (flatVel.magnitude > stats.divingMaxSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * stats.divingMaxSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
        else
        {
            if (flatVel.magnitude > stats.MaxSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * stats.MaxSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }


    public void ActivateStandingCollider()
    {
        standingCollider.enabled = true;
        diveCollider.enabled = false;
    }

    public void ActivateDiveCollider()
    {
        standingCollider.enabled = false;
        diveCollider.enabled = true;
    }


    private void OnDrawGizmos()
    {
        Ray ray = new Ray(transform.position + new Vector3(0,0.5f,0), player.forward);
        if (col != null) {
            Gizmos.DrawWireCube(new Vector3(player.position.x, diveCollider.bounds.min.y, player.position.z) + player.forward * 2, stats.diveColliderSize);
        }
        Gizmos.DrawRay(new Vector3(player.position.x, diveCollider.bounds.min.y, player.position.z) + player.forward * 2, -player.up);
    }
    #endregion
}
