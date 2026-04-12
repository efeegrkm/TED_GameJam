using System;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public static PlayerAnimator Instance { get; private set; }

    private Animator animator;

    private readonly string isWalking = "IsWalking";
    private readonly string isHappy = "IsHappy";
    private readonly string isSad = "IsSad";

    public void AnimateBeingHappy()
    {
        animator.SetTrigger(isHappy);
    }

    public void AnimateBeingSad()
    {
        animator.SetTrigger(isSad);
    }

    private void Awake()
    {
        SetInstance();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        PlayerMovementManager.Instance.OnPlayerMoved += AnimateWalk;
        PlayerMovementManager.Instance.OnPlayerStoppedMoving += AnimateIdle;
    }

    private void Update()
    {
        
    }

    private void AnimateWalk(object sender, EventArgs e)
    {
        Debug.Log("Animate walk");
        if (!animator.GetBool(isWalking))
        {
            animator.SetBool(isWalking, true);
        }
    }

    private void AnimateIdle(object sender, EventArgs e)
    {
        if (animator.GetBool(isWalking))
        {
            animator.SetBool(isWalking, false);
        }
    }

    private void SetInstance()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }
}
