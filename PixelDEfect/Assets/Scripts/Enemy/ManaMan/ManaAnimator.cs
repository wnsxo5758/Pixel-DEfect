using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaAnimator : MonoBehaviour
{
    private Animator animator;
    private EnemyAnimator baseAnimator;

    // Hash된 파라미터 이름들
    private readonly int castSkill = Animator.StringToHash("CastSkill");
    private readonly int manaDeath = Animator.StringToHash("ManaDeath");
    private readonly int jump = Animator.StringToHash("Jump");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        baseAnimator = GetComponent<EnemyAnimator>();

        if (animator == null)
            Debug.LogError("Animator 컴포넌트가 없습니다!");

        if (baseAnimator == null)
            Debug.LogWarning("EnemyAnimator 컴포넌트가 없습니다!");
    }

    // 🎮 기본 기능 위임 (필요한 것만)
    public void SetMovement(float speed)
    {
        baseAnimator?.SetMovementAnim(speed);
    }

    public void SetChasing(bool isChasing)
    {
        baseAnimator?.SetChasingState(isChasing);
    }

    public void TriggerAttack()
    {
        baseAnimator?.TriggerAttackAnim();
    }

    public void TriggerHit()
    {
        baseAnimator?.TriggerHitAnim();
    }

    public void TriggerDeath()
    {
        baseAnimator?.TriggerDeathAnim(); // 일반 사망
    }

    public void TriggerSkillCast()
    {
        animator?.SetTrigger(castSkill);
    }

    public void TriggerManaDeath()
    {
        animator?.SetTrigger(manaDeath);
    }

    public void TriggerJump()
    {
        //animator?.SetTrigger(jump);
    }
}
