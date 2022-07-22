using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private Vector3 lastPosition;
    private bool isCrouching;
    private bool isCrawling;

    public void AnimateBasedOnSpeed()
    {
        lastPosition.y = transform.position.y;
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        animator.SetBool("IsMoving", distanceMoved > 0.01f);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsCrawling", isCrawling);

        lastPosition = transform.position;
    }

    public void SetCrouching(bool isCrouching)
    {
        this.isCrouching = isCrouching;
    }

    public void SetCrawling(bool isCrawling)
    {
        this.isCrawling = isCrawling;
    }
}
