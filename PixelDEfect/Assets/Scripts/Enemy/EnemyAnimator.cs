using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private readonly int moveSpeed = Animator.StringToHash("Speed");
    private readonly int attack = Animator.StringToHash("Attack");
    private readonly int hit = Animator.StringToHash("Hit");
    private readonly int death = Animator.StringToHash("Death");
    private readonly int isChasing = Animator.StringToHash("IsChasing");
    
    // 보스 애니메이션 파라미터
    private readonly int headbuttPrep = Animator.StringToHash("HeadbuttPrep");
    private readonly int headbuttCharge = Animator.StringToHash("HeadbuttCharge");
    private readonly int bossUpTrigger = Animator.StringToHash("BossUp");
    private readonly int bossUpIdle = Animator.StringToHash("BossUpIdle");
    private readonly int bossDown = Animator.StringToHash("BossDown");
    private readonly int stunTrigger = Animator.StringToHash("Stun");
    private readonly int isStunned = Animator.StringToHash("IsStunned");
    
    private Animator animator;
    private MovementRigidbody2D movement; // 움직임

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<MovementRigidbody2D>();
    }

    // 새로운 animation 메소드

    public void SetMovementAnim(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat(moveSpeed, Mathf.Abs(speed));
        }
    }

    public void SetChasingState(bool _isChasing)
    {
        if (animator != null)
        {
            animator.SetBool(isChasing, _isChasing);
        }
    }

    public void TriggerAttackAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(attack);
        }
    }

    public void TriggerHitAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(hit);
        }
    }

    public void TriggerDeathAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(death);
        }
    }

    private void OnAttackEvent()
    {
        EnemyBT enemy = transform.GetComponentInParent<EnemyBT>();
        enemy.OnAttackAnimationEvent();
    }

    private void OnAttackFinished()
    {
        EnemyBT enemy = transform.GetComponentInParent<EnemyBT>();
        enemy.OnAttackAnimationFinished();
    }
    
    // 보스 전용 애니메이션 메서드
    public void TriggerHeadbuttPrep()
    {
        if (animator != null)
        {
            animator.SetTrigger(headbuttPrep);
        }
    }
    
    public void SetHeadbuttCharge(bool isCharge)
    {
        if (animator != null)
        {
            animator.SetBool(headbuttCharge, isCharge);
        }
    }
    
    public void TriggerBossUp()
    {
        if (animator != null)
        {
            animator.SetTrigger(bossUpTrigger);
        }
    }
    
    public void SetBossUpIdle(bool isUpIdle)
    {
        if (animator != null)
        {
            animator.SetBool(bossUpIdle, isUpIdle);
        }
    }

    public void SetBossDown(bool isDown)
    {
        if (animator != null)
        {
            animator.SetBool(bossDown, isDown);
        }
    }

    public void TriggerStunned(bool _isStunned)
    {
        if (animator != null)
        {
            animator.SetTrigger(stunTrigger);
            SetStunned(_isStunned);
        }
    }
    
    public void SetStunned(bool _isStunned)
    {
        if (animator != null)
        {
            animator.SetBool(isStunned, _isStunned);
        }
    }
}
