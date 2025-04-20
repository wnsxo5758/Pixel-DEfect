using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private readonly int velocityX = Animator.StringToHash("VelocityX");
    private readonly int velocityY = Animator.StringToHash("VelocityY");
    private readonly int isJump = Animator.StringToHash("IsJump");
    private readonly int isAttack = Animator.StringToHash("IsAttack");
    private readonly int isConnected = Animator.StringToHash("IsConnected");
    private readonly int isClimbing = Animator.StringToHash("IsClimbing");
    private readonly int isCrouching = Animator.StringToHash("IsCrouching");
    private readonly int roll = Animator.StringToHash("Roll");
    private readonly int death = Animator.StringToHash("Death");
    
    // 공격 애니메이션
    private readonly int hasWeapon = Animator.StringToHash("HasWeapon");
    private readonly int attack = Animator.StringToHash("Attack");
    private readonly int throwWeapon = Animator.StringToHash("Throw");

    private Animator animator; // 애니메이션 
    private MovementRigidbody2D movement; // 움직임
    private PlayerAttack playerAttack; // 플레이어 공격
    private PlayerInteraction playerInteraction; // 상호작용

    private float pnpDirection;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<MovementRigidbody2D>();
        playerAttack = GetComponentInParent<PlayerAttack>();
        playerInteraction = GetComponentInParent<PlayerInteraction>();
    }
    
    public void MovementAnim(float x)
    {
        if (movement.IsGrounded)
        {
            animator.SetFloat(velocityX, Mathf.Abs(x)); // X 값에 따라 변경
        }
        else
        {
            animator.SetFloat(velocityY, movement.Velocity.y); // Y 값에 따라 변경 -> y가 작으면 공중에서 내려가는 모션, 높으면 올라가는 모션
        }
        
        animator.SetBool(isJump, !movement.IsGrounded); // 땅에 닿은 상태가 아닌 경우
    }

    public void SetCrouchAnim(bool crouching)
    {
        animator.SetBool(isCrouching, crouching);
        animator.SetBool(isJump, false);
    }
    
    public void CrawlAnim(float x)
    {
        animator.SetFloat(velocityX, Mathf.Abs(x));
    }

    public void StartRollAnim()
    {
        animator.SetTrigger(roll);
    }
    
    public void EnterHoldAnim(float dir)
    {
        animator.SetBool(isConnected, playerInteraction.IsConnected);
        pnpDirection = dir;
    }
    
    public void PushAndPullAnim(float x)
    {
        animator.SetFloat(velocityX, pnpDirection * x);
        
        animator.SetBool(isConnected, playerInteraction.IsConnected);
    }

    public void SetClimbAnim(bool isOnLadder)
    {
        animator.SetBool(isClimbing, isOnLadder);
    }
    
    public void ClimbAnim(float y)
    {
        animator.SetFloat(velocityY, Mathf.Abs(y));
    }

    public void SetHasWeapon(bool weapon)
    {
        animator.SetBool(hasWeapon, weapon);
    }

    public void TriggerAttackAnim()
    {
        animator.SetTrigger(attack);
    }

    public void TriggerDeathAnim()
    {
        animator.SetTrigger(death);
    }

    public void TriggerThrowAnim()
    {
        animator.SetTrigger(throwWeapon);
    }

    // 공격 타이밍 이벤트
    private void HandleAttackEvent()
    {
        playerAttack.PerformMeleeAttack();
    }

    private void FinishedMeleeAttackEvent()
    {
        playerAttack.FinishedMeleeAttackAnim();
    }

    private void FinishedThrowWeaponEvent()
    {
        playerAttack.FinishedThrowAnim();
    }
    
    private void RestartGameEvent()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }
}
