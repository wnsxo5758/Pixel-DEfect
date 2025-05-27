using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchBT : ManaBT
{

    [Header("공격 관련")]
    [SerializeField]
    private GameObject wrenchBulletPrefab;
    [SerializeField]
    private Transform firePos;
    [SerializeField]
    private float bulletSpeed; // 총알 속도
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private float attackRange = 6f; //공격가능 범위

    [Header("스킬 관련")]
    [SerializeField]
    private int skillBulletCount = 5; // 스킬로 생성되는 렌치수
    [SerializeField]
    private float skillAngle = 60f; 

    private float attackTimer = 0f;
    private bool canAttack = true;
    private bool isAttacking = false;
    private float attackCooldown;
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        if(!canAttack)
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

    private NodeState PerformAttack()
    {
        if (isHit || isDead) return NodeState.Failure;

        canAttack = false;
        isAttacking = true;
        attackTimer = 0f;

        blackboard.SetValue("CanAttack", false);
        blackboard.SetValue("IsAttacking", true);

        Transform currentTarget = blackboard.GetValue<Transform>("Target");
        float directionToTarget = Mathf.Sign(currentTarget.position.x - transform.position.x);
        SetDirection(directionToTarget);

        movement?.MoveTo(0);

        animator?.SetMovementAnim(0);
        animator?.SetChasingState(true);
        animator?.TriggerAttackAnim();

        return NodeState.Running;
    }
    private bool IsInAttackRange()
    {
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return false;

        return Vector2.Distance(transform.position, target.position) <= attackRange;
    }



    public void OnSkillEffectTrigger() // 스킬 트리거
    {
        float baseAngle = GetDirection() > 0 ? 0f : 180f; // 방향 따라 기준
        float startAngle = baseAngle - skillAngle / 2f;
        float angleStep = skillAngle / (skillBulletCount - 1);

        for (int i = 0; i < skillBulletCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;

            GameObject bullet = Instantiate(wrenchBulletPrefab, firePos.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().velocity = dir.normalized * bulletSpeed;
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
    public override void OnAttackAnimationEvent()
    {
        if (isHit) return;

        Vector2 dir = new Vector2(GetDirection(), 0f);
        GameObject bullet = Instantiate(wrenchBulletPrefab, firePos.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = dir.normalized * bulletSpeed;
    }

    public override void OnAttackAnimationFinished()
    {
        isAttacking = false;
        blackboard.SetValue("IsAttacking", false);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
