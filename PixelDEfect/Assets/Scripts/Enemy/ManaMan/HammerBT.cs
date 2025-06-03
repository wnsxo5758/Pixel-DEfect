using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerBT : ManaBT
{
    [Header("공격 관련")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 2;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    [SerializeField] private Vector2 attackBoxOffset = new Vector2(0.8f, 0);
    [SerializeField] private LayerMask playerLayer;


    [Header("스킬 관련")]
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private float skillSpawnDelay = 0.5f;

    private float attackTimer = 0f;
    private bool canAttack = true;
    private bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();
        blackboard.SetValue("AttackRange", attackRange);
        blackboard.SetValue("CanAttack", true);
        blackboard.SetValue("IsAttacking", false);
    }

    protected override void Update()
    {
        base.Update();
        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                canAttack = true;
                attackTimer = 0f;
                blackboard.SetValue("CanAttack", true);
            }
        }


    }


    protected override Node CreateSkillSequence()
    {
        Sequence skillSequence = new Sequence();

        skillSequence.AddChild(new ConditionNode(() => !isDead && !isHit));
        skillSequence.AddChild(new ConditionNode(() => IsTargetInSkillRange()));
        skillSequence.AddChild(new ConditionNode(() => canUseSkill));
        skillSequence.AddChild(new ActionNode(UseSkill));

        return skillSequence;
    }

    protected override NodeState UseSkill()
    {
        // 피격 또는 사망 중이면 스킬 취소
        if (isHit || isDead) return NodeState.Failure;

        Transform currentTarget = blackboard.GetValue<Transform>("Target");
        if (currentTarget == null) return NodeState.Failure;

        // 스킬 타겟 위치 저장
        Vector2 skillTargetPos = currentTarget.position;
        blackboard.SetValue("SkillTargetPosition", skillTargetPos);

        // 방향 설정
        float directionToTarget = Mathf.Sign(currentTarget.position.x - transform.position.x);
        SetDirection(directionToTarget);

        // 이동 중지
        movement?.MoveTo(0);

        // 애니메이션 실행
        if (animator != null)
        {
            animator.SetMovementAnim(0);
            animator.SetChasingState(true);
            manaAnimator.TriggerSkillAnim(); // 트리거 이름은 "Skill"
        }

        // 스킬 상태 설정
        canUseSkill = false;
        skillTimer = 0f;
        isAttacking = true;
        blackboard.SetValue("IsAttacking", true);

        return NodeState.Running;
    }

    protected override Node CreateAttackSequence()
    {
        Sequence attackSequence = new Sequence();

        attackSequence.AddChild(new ConditionNode(() => !blackboard.GetValue<bool>("IsHit")));
        attackSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected")));
        attackSequence.AddChild(new ConditionNode(IsInAttackRange));
        attackSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("CanAttack")));
        attackSequence.AddChild(new ConditionNode(() => !blackboard.GetValue<bool>("IsAttacking")));
        attackSequence.AddChild(new ActionNode(PerformAttack));

        return attackSequence;
    }

    private bool IsInAttackRange()
    {
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.position) <= attackRange;
    }

    protected override bool IsTargetInAttackRange() => IsInAttackRange();
    protected override float GetAttackRange() => attackRange;

    private NodeState PerformAttack()
    {
        if (isHit || isDead) return NodeState.Failure;

        canAttack = false;
        isAttacking = true;
        attackTimer = 0f;

        blackboard.SetValue("CanAttack", false);
        blackboard.SetValue("IsAttacking", true);

        Transform target = blackboard.GetValue<Transform>("Target");
        float dir = Mathf.Sign(target.position.x - transform.position.x);
        SetDirection(dir);

        movement?.MoveTo(0);

        animator?.SetMovementAnim(0);
        animator?.SetChasingState(true);
        animator?.TriggerAttackAnim();

        return NodeState.Running;
    }

    public override void OnAttackAnimationEvent() //공격 트리거
    {
        if (isHit) return;
        DealDamage();
    }

    public override void OnAttackAnimationFinished() //공격 끝났을 경우
    {
        isAttacking = false;
        blackboard.SetValue("IsAttacking", false);

        if (IsTargetInAttackRange())
        {
            // 공격 범위 내에 있으면 플레이어 방향만 바라보도록 설정
            Transform currentTarget = blackboard.GetValue<Transform>("Target");
            if (currentTarget != null)
            {
                float directionToTarget = Mathf.Sign(currentTarget.position.x - transform.position.x);
                SetDirection(directionToTarget);

                if (movement != null)
                {
                    movement.MoveTo(0);
                }
            }
        }
    }
    public void OnSkillEffectTrigger() // 스킬 트리거
    {
        Vector2 pos = blackboard.GetValue<Vector2>("SkillTargetPosition");

        PlaySound(skillClip);
        if (lightningPrefab != null)
        {
            Instantiate(lightningPrefab, pos, Quaternion.identity);
        }
    }
    public void OnSkillAnimationFinished() // 스킬이 끝났을 경우
    {
        isAttacking = false;
        blackboard.SetValue("IsAttacking", false);

        if (IsTargetInAttackRange())
        {
            // 공격 범위 내에 있으면 플레이어 방향만 바라보도록 설정
            Transform currentTarget = blackboard.GetValue<Transform>("Target");
            if (currentTarget != null)
            {
                float directionToTarget = Mathf.Sign(currentTarget.position.x - transform.position.x);
                SetDirection(directionToTarget);

                if (movement != null)
                {
                    movement.MoveTo(0);
                }
            }
        }
    }

    private void DealDamage()
    {
        float dir = GetDirection();
        Vector2 attackPos = attackPoint != null
            ? attackPoint.position
            : (Vector2)transform.position + new Vector2(attackBoxOffset.x * dir, attackBoxOffset.y);

        Collider2D hitPlayer = Physics2D.OverlapBox(attackPos, attackBoxSize, 0, playerLayer);

        if (hitPlayer != null)
        {
            PlayerHp hp = hitPlayer.GetComponent<PlayerHp>();
            if (hp != null)
            {
                DeathData deathData = new DeathData(DeathCause.Hammer, dir);
                hp.DecreaseHp(attackDamage, deathData,true, true);
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        float dir = Application.isPlaying ? GetDirection() : Mathf.Sign(transform.localScale.x);
        Vector2 attackPos = attackPoint != null
            ? attackPoint.position
            : (Vector2)transform.position + new Vector2(attackBoxOffset.x * dir, attackBoxOffset.y);
        Gizmos.DrawWireCube(attackPos, attackBoxSize);
    }
}
