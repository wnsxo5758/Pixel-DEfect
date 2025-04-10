using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly int VelocityX = Animator.StringToHash("VelocityX");
    private static readonly int VelocityY = Animator.StringToHash("VelocityY");
    private static readonly int IsJump = Animator.StringToHash("IsJump");
    private static readonly int IsAttack = Animator.StringToHash("IsAttack");
    private static readonly int IsConnected = Animator.StringToHash("IsConnected");
    private static readonly int IsClimbing = Animator.StringToHash("IsClimbing");
    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
    
    // 공격 애니메이션
    private static readonly int HasWeapon = Animator.StringToHash("HasWeapon");
    private static readonly int Attack = Animator.StringToHash("Attack");

    private Animator animator; // 애니메이션 
    private MovementRigidbody2D movement; // 움직임
    private PlayerAttack attack; // 플레이어 공격
    private PlayerInteraction playerInteraction; // 상호작용

    private float pnpDirection;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<MovementRigidbody2D>();
        attack = GetComponentInParent<PlayerAttack>();
        playerInteraction = GetComponentInParent<PlayerInteraction>();
    }
    
    public void MovementAnim(float x)
    {
        if (movement.IsGrounded)
        {
            animator.SetFloat(VelocityX, Mathf.Abs(x)); // X 값에 따라 변경
        }
        else
        {
            animator.SetFloat(VelocityY, movement.Velocity.y); // Y 값에 따라 변경 -> y가 작으면 공중에서 내려가는 모션, 높으면 올라가는 모션
        }
        
        animator.SetBool(IsJump, !movement.IsGrounded); // 땅에 닿은 상태가 아닌 경우
    }

    public void SetCrouchAnim(bool isCrouching)
    {
        animator.SetBool(IsCrouching, isCrouching);
    }
    
    public void CrawlAnim(float x)
    {
        animator.SetFloat(VelocityX, Mathf.Abs(x));
    }
    
    public void EnterHoldAnim(float dir)
    {
        animator.SetBool(IsConnected, playerInteraction.IsConnected);
        pnpDirection = dir;
    }
    
    public void PushAndPullAnim(float x)
    {
        animator.SetFloat(VelocityX, pnpDirection * x);
        
        animator.SetBool(IsConnected, playerInteraction.IsConnected);
    }

    public void SetClimbAnim(bool isOnLadder)
    {
        animator.SetBool(IsClimbing, isOnLadder);
    }
    
    public void ClimbAnim(float y)
    {
        animator.SetFloat(VelocityY, Mathf.Abs(y));
    }

    public void SetHasWeapon(bool hasWeapon)
    {
        animator.SetBool(HasWeapon, hasWeapon);
    }

    public void TriggerAttackAnim()
    {
        animator.SetTrigger(Attack);
    }

    // 공격 타이밍 이벤트
    private void HandleAttackEvent()
    {
        attack.PerformMeleeAttack();
    }
}
