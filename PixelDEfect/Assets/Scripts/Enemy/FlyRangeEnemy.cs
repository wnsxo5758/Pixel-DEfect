using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyRangeEnemy : EnemyBT
{

    [Header("원거리 적 기본 세팅")]

    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask playerLayer;


    [Header("총알 관련")]
    [SerializeField]
    private Transform attackPos;
    [SerializeField]
    private GameObject bullet;

    private MemoryPool pool; // 총알 관리를 위한 메모리풀

    private float attackTimer = 0f;
    private bool canAttack = true;
    private bool isAttacking = false;

    protected virtual void Awake()
    {
        base.Awake();
        // 공격 관련 값 블랙보드에 추가
        blackboard.SetValue("AttackRange", attackRange);
        blackboard.SetValue("CanAttack", true);
        blackboard.SetValue("AttackTimer", 0f);
        blackboard.SetValue("IsAttacking", false);

    }

    protected override void Update()
    {
        // 공격 쿨다운 관리
        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            blackboard.SetValue("AttackTimer", attackTimer);

            if (attackTimer >= attackCooldown)
            {
                canAttack = true;
                attackTimer = 0f;
                blackboard.SetValue("CanAttack", true);
            }
        }

        // 공격 중이면서 피격 중이 아닐 때만 공격 상태 유지
        if (isAttacking && !isHit)
        {

        }
        else
        {
            base.Update();
        }
    }


    protected override Node CreateAttackSequence()
    {
        // 공격 시퀀스
        Sequence attackSequence = new Sequence();

        // 조건 노드들
        ConditionNode isNotHit = new ConditionNode(() => !blackboard.GetValue<bool>("IsHit"));
        ConditionNode isPlayerDetected = new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected"));
        ConditionNode isInAttackRange = new ConditionNode(IsInAttackRange);
        ConditionNode canAttackNow = new ConditionNode(() => blackboard.GetValue<bool>("CanAttack"));
        ConditionNode isNotAttacking = new ConditionNode(() => !blackboard.GetValue<bool>("IsAttacking"));

        // 공격 액션 노드
        ActionNode performAttack = new ActionNode(PerformAttack);

        // 공격 시퀀스 구성
        attackSequence.AddChild(isNotHit);          // 피격 상태가 아닌지
        attackSequence.AddChild(isPlayerDetected);  // 플레이어가 감지되었는지
        attackSequence.AddChild(isInAttackRange);   // 공격 범위 내에 있는지
        attackSequence.AddChild(canAttackNow);      // 공격 쿨다운이 끝났는지
        attackSequence.AddChild(isNotAttacking);    // 이미 공격 중이 아닌지
        attackSequence.AddChild(performAttack);     // 공격 수행

        return attackSequence;
    }


    protected override bool IsTargetInAttackRange()
    {
        Transform currentTarget = blackboard.GetValue<Transform>("Target");

        if (currentTarget == null)
        {
            return false;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);
        return distance <= attackRange;
    }

    // 공격 범위 반환 메서드
    protected override float GetAttackRange()
    {
        return attackRange;
    }

    // 공격 범위 내 체크
    private bool IsInAttackRange()
    {
        Transform currentTarget = blackboard.GetValue<Transform>("Target");

        if (currentTarget == null)
        {
            return false;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // 타겟이 공격 범위 내에 있으면 해당 방향으로 적 방향 설정
        if (distance <= attackRange)
        {
            return true;
        }

        return false;
    }

    // 공격 수행
    private NodeState PerformAttack()
    {
        // 피격 중이면 공격 취소
        if (isHit || isDead) return NodeState.Failure;

        // 공격 쿨다운 설정
        canAttack = false;
        isAttacking = true;
        attackTimer = 0f;

        // 블랙보드 업데이트
        blackboard.SetValue("CanAttack", false);
        blackboard.SetValue("IsAttacking", true);

        Transform currentTarget = blackboard.GetValue<Transform>("Target");
        float directionToTarget = Mathf.Sign(currentTarget.position.x - transform.position.x);
        SetDirection(directionToTarget);

        //이동 중지
        if (movement != null)
        {
            movement.MoveTo(0);
        }

        // 애니메이션 재생
        if (animator != null)
        {
            animator.SetMovementAnim(0);
            animator.SetChasingState(true);
            animator.TriggerAttackAnim();
        }

        return NodeState.Success;
    }

    private void FireBullet()
    {
        GameObject bullet = pool.ActivePoolItem();
        if (bullet != null)
        {
            bullet.transform.position = attackPos.position;
            bullet.transform.rotation = attackPos.rotation;
            bullet.SetActive(true);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float bulletSpeed = 10f;
                Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
                rb.velocity = dir * bulletSpeed;
            }
        }
    }
}
