using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchBT : ManaBT
{

    [Header("공격 관련")]
    [SerializeField]
    private float attackCooldown = 2f;
    [SerializeField]
    private GameObject wrenchPrefab;
    [SerializeField]
    private Transform firePos;
    [SerializeField]
    private float bulletSpeed; // 총알 속도
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private float attackRange = 6f; //공격가능 범위


    private float attackTimer = 0f;
    private bool canAttack = true;
    private bool isAttacking = false;

    [Header("스킬 관련")]
    [SerializeField]
    private int skillBulletCount = 5; // 스킬로 생성되는 렌치수
    [SerializeField]
    private float skillAngle = 60f; //부채꼴 범위

    //총알 메모리풀
    private MemoryPool bulletPool;

    protected override void Awake()
    {
        base.Awake();
        blackboard.SetValue("AttackRange", attackRange);
        blackboard.SetValue("CanAttack", true);
        blackboard.SetValue("IsAttacking", false);
    }

    protected override void Start()
    {
        base.Start();
        bulletPool = new MemoryPool(wrenchPrefab);
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

        //공격 상태 설정 (스킬 사용시 공격도 초기화)
        canAttack = false;
        attackTimer = 0f;
        blackboard.SetValue("CanAttack", false);


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
        Debug.Log("렌치봇 스킬 사용");
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return;

        Vector2 targetPos = (Vector2)target.position + new Vector2(0f, 0.8f);
        Vector2 centerDir = (targetPos - (Vector2)firePos.position).normalized;

        float baseAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;

        int totalBullets = skillBulletCount;
        if (totalBullets <= 1) totalBullets = 1; // 최소 1발

        float angleStep = skillAngle / (totalBullets - 1);

        for (int i = 0; i < totalBullets; i++)
        {
            float offsetAngle = -skillAngle / 2f + angleStep * i;
            float finalAngle = baseAngle + offsetAngle;

            Vector2 dir = Quaternion.Euler(0, 0, finalAngle) * Vector2.right;

            GameObject bullet = bulletPool.ActivePoolItem();
            if (bullet != null)
            {
                bullet.transform.position = firePos.position;
                bullet.transform.rotation = Quaternion.identity;

                BulletBase bulletScript = bullet.GetComponent<BulletBase>();
                if (bulletScript != null)
                {
                    bulletScript.SetUp(dir, bulletPool);
                }
            }
        }
    }
    public void OnSkillAnimationFinished() // 스킬이 끝났을 경우
    {
        Debug.Log("렌치봇 스킬 끝!");
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

        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return;

        Vector2 targetPos = (Vector2)target.position + new Vector2(0f, 0.8f);
        Vector2 dir = (targetPos - (Vector2)firePos.position).normalized;

        if (wrenchPrefab != null && firePos != null)
        {
            GameObject bullet = bulletPool.ActivePoolItem();
            if (bullet != null)
            {
                bullet.transform.position = firePos.position;
                bullet.transform.rotation = Quaternion.identity;

                BulletBase bulletScript = bullet.GetComponent<BulletBase>();
                if (bulletScript != null)
                {
                    bulletScript.SetUp(dir, bulletPool);
                }

            }
        }


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


        if (firePos != null)
        {
            Gizmos.color = Color.cyan;

            Transform target = Application.isPlaying
                ? blackboard.GetValue<Transform>("Target")
                : null;

            Vector2 centerDir = Vector2.right * GetDirection();
            if (Application.isPlaying && target != null)
            {
                Vector2 targetPos = (Vector2)target.position + new Vector2(0f, 0.5f);
                centerDir = (targetPos - (Vector2)firePos.position).normalized;
            }

            float baseAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;

            int totalBullets = skillBulletCount;
            if (totalBullets <= 1) totalBullets = 1;

            float angleStep = skillAngle / (totalBullets - 1);

            for (int i = 0; i < totalBullets; i++)
            {
                float offsetAngle = -skillAngle / 2f + angleStep * i;
                float finalAngle = baseAngle + offsetAngle;
                Vector2 dir = Quaternion.Euler(0, 0, finalAngle) * Vector2.right;

                Gizmos.DrawRay(firePos.position, dir.normalized * 3f);
            }
        }
    }



}
