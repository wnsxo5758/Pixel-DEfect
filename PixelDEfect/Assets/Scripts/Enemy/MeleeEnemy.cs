using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : EnemyBT
{
    [Header("근접 공격 설정")] [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.2f, 0.8f);
    [SerializeField] private Vector2 attackBoxOffset = new Vector2(0.6f, 0);
    [SerializeField] private LayerMask playerLayer;

    [Header("접촉 데미지 설정")] [SerializeField] private bool enableContactDamage = true;
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float contactDamageCooldown = 2f;
    [SerializeField] private float contactRange = 1f;

    private float attackTimer = 0f;
    private bool canAttack = true;
    private bool isAttacking = false;

    private float contactDamageTimer = 0f;
    private bool canDealContactDamage = true;
    private readonly float contactCheckInterval = 0.2f;
    private float contactCheckTimer = 0f;

    protected override void Awake()
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

        // 접촉 데미지 쿨다운 관리
        if (!canDealContactDamage)
        {
            contactDamageTimer += Time.deltaTime;

            if (contactDamageTimer >= contactDamageCooldown)
            {
                canDealContactDamage = true;
                contactDamageTimer = 0f;
            }
        }

        // 최적화를 위해 일정 간격으로만 접촉 체크
        if (enableContactDamage && !isDead)
        {
            contactCheckTimer += Time.deltaTime;

            if (contactCheckTimer >= contactCheckInterval)
            {
                CheckPlayerContact();
                contactCheckTimer = 0f;
            }
        }

        base.Update();
    }

    // 공격 시퀀스 생성 오버라이드
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
        attackSequence.AddChild(isNotHit); // 피격 상태가 아닌지
        attackSequence.AddChild(isPlayerDetected); // 플레이어가 감지되었는지
        attackSequence.AddChild(isInAttackRange); // 공격 범위 내에 있는지
        attackSequence.AddChild(canAttackNow); // 공격 쿨다운이 끝났는지
        attackSequence.AddChild(isNotAttacking); // 이미 공격 중이 아닌지
        attackSequence.AddChild(performAttack); // 공격 수행

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

        return NodeState.Running;
    }

    // 애니메이션 이벤트 호출
    public override void OnAttackAnimationEvent()
    {
        // 피격 중이면 공격 판정 취소
        if (isHit) return;

        DealDamage();
    }

    // 애니메이션 종료 이벤트 호출
    public override void OnAttackAnimationFinished()
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

    // 데미지 처리
    private void DealDamage()
    {
        // 캐릭터 방향에 따라 공격 박스 위치 조정
        float direction = GetDirection();
        Vector2 attackPos = attackPoint != null
            ? attackPoint.position
            : (Vector2)transform.position + new Vector2(attackBoxOffset.x * direction, attackBoxOffset.y);

        // 공격 반정 박스 생성
        Collider2D hitPlayer = Physics2D.OverlapBox(attackPos, attackBoxSize, 0, playerLayer);

        // 플레이어에게 데미지
        if (hitPlayer != null)
        {
            PlayerHp playerHp = hitPlayer.GetComponent<PlayerHp>();
            if (playerHp != null)
            {
                playerHp.DecreaseHp(attackDamage, true, true);

                // 히트 효과
            }
        }
    }

    // 플레이어 접촉 처리
    private void CheckPlayerContact()
    {
        if (!canDealContactDamage)
            return;

        // 플레이어 감지
        Vector2 contactPos = new Vector2(transform.position.x + enemyCollider.offset.x * GetDirection(),
            transform.position.y + enemyCollider.offset.y);
        Collider2D playerCollider = Physics2D.OverlapCircle(contactPos, contactRange, playerLayer);

        if (playerCollider != null)
        {
            PlayerHp playerHp = playerCollider.GetComponent<PlayerHp>();
            if (playerHp != null)
            {
                playerHp.DecreaseHp(contactDamage, true);

                // 쿨다운 적용
                canDealContactDamage = false;
                contactDamageTimer = 0f;
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 공격 판정 박스
        Gizmos.color = Color.magenta;
        float direction = Application.isPlaying ? GetDirection() : Mathf.Sign(transform.localScale.x);
        Vector2 attackPos = attackPoint != null
            ? attackPoint.position
            : (Vector2)transform.position + new Vector2(attackBoxOffset.x * direction, attackBoxOffset.y);
        Gizmos.DrawWireCube(attackPos, attackBoxSize);

        Gizmos.color = Color.yellow;
        if (enemyCollider != null && enableContactDamage)
        {
            Vector2 contactPos = new Vector2(transform.position.x + enemyCollider.offset.x * GetDirection(),
                transform.position.y + enemyCollider.offset.y);
            
            Gizmos.DrawWireSphere(contactPos, contactRange);
        }
        
    }

}
