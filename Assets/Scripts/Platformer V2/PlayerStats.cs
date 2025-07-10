using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class PlayerStats : ScriptableObject
{
    [Header("Movement")]
    public float MaxSpeed;
    public float acceleration;
    public float airAcceleration;
    public float friction;
    public float gravity;
    [Header("Jump")]
    public float jumpForce;
    public float EarlyJumpEndMult;
    public float airFriction;
    [Header("Diving")]
    public float divingHorizontalForce;
    public float divingVerticalForce;
    public float divingMaxSpeed;
    public float divingCancelJumpForce;
    public float bonkDistance;
    public float bonkVerticalDistance;
    [Header("BellySliding")]
    public float bellySlideFriction;
    public float bellySlideCancelJumpForce;
    public float bellySlideTiming;
    [Header("WallJumping")]
    public float wallCheckDistance;
    public float floorCheckDistance;
    public RaycastHit wallHit;
    public RaycastHit floorHit;
    public float fallingWallCheckDistance;
    public float wallSlideGravity;
    public float wallJumpVerticalForce;
    public float wallJumpHorizontalForce;
    public float wallJumpFriction;
    [Header("Colliders")]
    public LayerMask Grounded;
    public LayerMask Wall;
    public Vector3 idleColliderSize;
    public Vector3 diveColliderSize;
    [Header("Camera")]
    public float rotationSpeed;

}

