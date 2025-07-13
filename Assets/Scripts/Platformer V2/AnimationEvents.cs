using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] Animator animator;

    public void PerfectSlideCancelEnd()
    {
        animator.SetBool("PerfectSlideCancel", false);
    }
    public void InitialBonkDone()
    {
        animator.SetBool("Bonked", false);
    }
}
