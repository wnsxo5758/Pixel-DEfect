using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyBT))]
public class EnemyTimeAffected : TimeAffectedEntity
{
    private EnemyBT enemyBehavior;
    private Rigidbody2D rb;
    private Animator animator;
    
    // 저장된 상태
    private Vector2 originalVelocity;
    private float originalAngularVelocity;
    private float originalAnimatorSpeed;
    
    private void Awake()
    {
        enemyBehavior = GetComponent<EnemyBT>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    public override void OnTimeStop()
    {
        base.OnTimeStop();
        
        // 물리 상태 저장 및 정지
        if (rb != null)
        {
            originalVelocity = rb.velocity;
            originalAngularVelocity = rb.angularVelocity;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
        
        
        // 애니메이션 정지
        if (animator != null)
        {
            originalAnimatorSpeed = animator.speed;
            animator.speed = 0f;
        }
        
        // 적 행동 일시정지
        enemyBehavior.FreezeTime();
    }

    public override void OnTimeResume()
    {
        base.OnTimeResume();
        
        // 물리 상태 복원
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = originalVelocity;
            rb.angularVelocity = originalAngularVelocity;
        }
        
        // 애니메이션 복원
        if (animator != null)
        {
            animator.speed = originalAnimatorSpeed;
        }
        
        // 적 행동 재개
        enemyBehavior.UnfreezeTime();
        
        // 축적된 데미지 적용
        ApplyPendingDamages();
    }
    
    // 축적된 데미지 적용
    private void ApplyPendingDamages()
    {
        if (pendingDamages.Count == 0)
            return;

        int totalDamage = 0;
        Vector2 averageDirection = Vector2.zero;
        
        // 모든 데미지 합산
        foreach (var damageInfo in pendingDamages)
        {
            totalDamage += damageInfo.damage;
            averageDirection += damageInfo.direction;
        }
        
        // 평균 방향 정규화
        if (averageDirection != Vector2.zero)
        {
            averageDirection.Normalize();
        }
        
        // 데미지 적용
        enemyBehavior.DecreaseHp(totalDamage);
        
        // 목록 초기화
        pendingDamages.Clear();
    }
}
