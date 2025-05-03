using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyRangeEnemy : EnemyBT
{
    [Header("원거리 공격 설정")]
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField] 
    private Transform firePoint;
    [SerializeField] 
    private float attackRange = 5f;
    [SerializeField]
    private float attackCooldown = 2f;
    [SerializeField] 
    private int bulletDamage = 1;
    [SerializeField]
    private float bulletSpeed = 10f;

    [Header("후퇴 설정")]
    [SerializeField] 
    private float retreatRange = 2f;
    [SerializeField] 
    private float retreatSpeedMultiplier = 0.5f;

    [Header("복귀 설정")]
    [SerializeField]
    private float returnSpeed = 2f;

    [Header("이동 설정")]
    [SerializeField] 
    protected float moveSpeed = 3f;



    [Header("총기 오브젝트")]
    [SerializeField] private Transform gunPivot; // 총기 회전 중심이 될 부모 객체
    [SerializeField] 
    private float rotateSpeed = 5f;
    private Quaternion initialGunRotation;
    private Vector3 initialGunLocalPosition;

    private Vector2 initialPosition;
    private float attackTimer = 0f;
    private bool canAttack = true;
    private bool isAttacking = false;
    private MemoryPool bulletPool;
    protected override void Awake()
    {
        base.Awake();
        initialPosition = transform.position;
        // 중력 제거 - 공중 적은 중력 영향 받지 않음
        rb.gravityScale = 0f;
        if (gunPivot != null)
        {

            initialGunLocalPosition = gunPivot.localPosition;
            initialGunRotation = gunPivot.rotation;
        }

        blackboard.SetValue("AttackRange", attackRange);
        blackboard.SetValue("RetreatRange", retreatRange);
        blackboard.SetValue("CanAttack", true);
        blackboard.SetValue("IsAttacking", false);
        blackboard.SetValue("InitialPosition", initialPosition);
    }

    protected override void Start()
    {
        base.Start();
        bulletPool = new MemoryPool(bulletPrefab);
    }
    private void SetupFlyRangeBehaviorTree()
    {
        Selector root = new Selector();

        // 사망
        Sequence deathSequence = new Sequence();
        deathSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("IsDead")));
        deathSequence.AddChild(new ActionNode(HandleDeath));

        // 피격
        Sequence hitSequence = new Sequence();
        hitSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("IsHit")));
        hitSequence.AddChild(new ActionNode(HandleHit));

        // 공격 및 후퇴
        Node attackNode = CreateAttackSequence();

        // 추적
        Sequence chaseSequence = new Sequence();
        chaseSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected")));
        chaseSequence.AddChild(new ActionNode(ChaseTarget));

        // 순찰
        Sequence patrolSequence = new Sequence();
        patrolSequence.AddChild(new ActionNode(Patrol));

        // 트리 구성
        root.AddChild(deathSequence);
        root.AddChild(hitSequence);
        root.AddChild(attackNode);
        root.AddChild(chaseSequence);
        root.AddChild(patrolSequence);

        behaviorTree = new BehaviorTree(root)
        {
            Blackboard = blackboard
        };
    }
    protected override void Update()
    {
        // ✅ 공격 쿨다운 처리
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

        // ✅ 피격 처리 (EnemyBT 복사 + 총기 위치 복원 추가)
        if (isHit)
        {
            stunTimer -= Time.deltaTime;
            blackboard.SetValue("StunTimer", stunTimer);

            if (stunTimer <= 0f)
            {
                isHit = false;
                blackboard.SetValue("IsHit", false);

                if (flashCoroutine != null)
                {
                    StopCoroutine(flashCoroutine);
                    flashCoroutine = null;
                }

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }

                // 총기 위치 복원
                if (gunPivot != null)
                {
                    gunPivot.localPosition = initialGunLocalPosition;
                }
            }
        }

        // 타겟 감지
        if (!isHit && !isDead)
        {
            DetectTarget();
        }

        // 행동 트리 평가
        if (behaviorTree != null)
        {
            behaviorTree.Evaluate();
        }

        //총기 회전
        UpdateGunRotation();
    }

    protected override Node CreateAttackSequence()
    {
        Sequence retreatSequence = new Sequence();
        retreatSequence.AddChild(new ConditionNode(() => !isAttacking)); // 공격 중엔 후퇴하지 않음
        retreatSequence.AddChild(new ConditionNode(() => IsTargetInRetreatRange()));
        retreatSequence.AddChild(new ActionNode(RetreatFromTarget));

        Sequence attackSequence = new Sequence();
        attackSequence.AddChild(new ConditionNode(() => !isHit));
        attackSequence.AddChild(new ConditionNode(() => IsTargetInAttackRange()));
        attackSequence.AddChild(new ConditionNode(() => canAttack));
        attackSequence.AddChild(new ActionNode(PerformRangedAttack));

        Selector attackSelector = new Selector();
        attackSelector.AddChild(retreatSequence);
        attackSelector.AddChild(attackSequence);

        return attackSelector;
    }

    // 공격 범위 내 판단
    protected override bool IsTargetInAttackRange()
    {
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return false;

        return Vector2.Distance(transform.position, target.position) <= attackRange;
    }

    private bool IsTargetInRetreatRange()
    {
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return false;

        return Vector2.Distance(transform.position, target.position) <= retreatRange;
    }

    private NodeState PerformRangedAttack()
    {
        if (isDead || isHit || !canAttack || isAttacking) return NodeState.Failure;

        isAttacking = true;
        canAttack = false;
        attackTimer = 0f;
        blackboard.SetValue("CanAttack", false);

        rb.velocity = Vector2.zero;

        // ✅ 방향 설정 (플레이어 기준)
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target != null)
        {
            float dirX = Mathf.Sign(target.position.x - transform.position.x);
            SetDirection(dirX);
        }

        if (animator != null)
        {
            animator.TriggerAttackAnim();
        }

        StartCoroutine(PerformTripleShot());

        return NodeState.Success;
    }

    private IEnumerator PerformTripleShot()
    {
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null)
        {
            isAttacking = false;
            yield break;
        }

        Vector2 fixedTargetPosition = target.position;
        Vector2 fireDirection = (fixedTargetPosition - (Vector2)firePoint.position).normalized;

        int shotCount = 3;
        float interval = 0.3f;

        for (int i = 0; i < shotCount; i++)
        {
            if (bulletPrefab != null && firePoint != null)
            {
                GameObject bullet = bulletPool.ActivePoolItem();
                if (bullet != null)
                {
                    bullet.transform.position = firePoint.position;
                    bullet.transform.rotation = Quaternion.identity;

                    BulletBase bulletScript = bullet.GetComponent<BulletBase>();
                    if (bulletScript != null)
                    {
                        bulletScript.SetUp(fireDirection, bulletPool);
                    }
                }
            }

            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(attackCooldown);
        if (gunPivot != null)
        {
            gunPivot.rotation = initialGunRotation;
        }
        canAttack = true;
        blackboard.SetValue("CanAttack", true);
        isAttacking = false;
    }

    protected override NodeState Patrol()
    {
        if (isHit || isDead || isAttacking) return NodeState.Failure;

        float direction = blackboard.GetValue<float>("PatrolDirection");

        if (float.IsNaN(direction) || direction == 0)
        {
            direction = 1f;
            blackboard.SetValue("PatrolDirection", direction);
        }

        if (CheckWall(direction))
        {
            direction *= -1;
            blackboard.SetValue("PatrolDirection", direction);
            SetDirection(direction);
        }

        rb.velocity = new Vector2(direction * moveSpeed, 0f);
        return NodeState.Running;
    }


    private void UpdateGunRotation()
    {
        if (gunPivot == null || isDead) return;
        if (isAttacking) return;
        bool playerDetected = blackboard.GetValue<bool>("PlayerDetected");
        Transform target = blackboard.GetValue<Transform>("Target");

        if (!playerDetected || target == null)
        {
            gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, initialGunRotation, Time.deltaTime * rotateSpeed);
            return;
        }

        // 🔁 방향 계산
        Vector2 dir = ((Vector2)target.position - (Vector2)gunPivot.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 👈 flip된 경우 회전 보정
        if (transform.localScale.x < 0)
        {
            angle += 180f;
        }

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }

    private NodeState RetreatFromTarget()
    {
        if (isDead || isHit || isAttacking) return NodeState.Failure;

        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return NodeState.Failure;

        Vector2 retreatDir = ((Vector2)transform.position - (Vector2)target.position).normalized;
        float retreatSpeed = moveSpeed * retreatSpeedMultiplier;

        rb.velocity = retreatDir * retreatSpeed;
        return NodeState.Running;
    }

    // 추적 상태에서 범위를 벗어나면 원래 위치로 복귀
    protected override void HandleLostTarget()
    {
        base.HandleLostTarget();
        StartCoroutine(ReturnToInitialPosition());
    }

    private IEnumerator ReturnToInitialPosition()
    {
        while (Vector2.Distance(transform.position, initialPosition) > 0.1f)
        {
            Vector2 direction = (initialPosition - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * returnSpeed * Time.deltaTime);
            yield return null;
        }
    }
    protected override NodeState ChaseTarget()
    {
        if (isHit || isDead || isAttacking) return NodeState.Failure;

        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return NodeState.Failure;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            rb.velocity = Vector2.zero;
            return NodeState.Success;
        }

        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        rb.velocity = dir * moveSpeed;

        if (dir.x != 0)
            SetDirection(Mathf.Sign(dir.x));

        return NodeState.Running;
    }
    protected override float GetAttackRange() => attackRange;

    public override void DecreaseHp(int damage, bool isThrownWeapon = false)
    {
        base.DecreaseHp(damage, isThrownWeapon);

        // 사망했으면 넉백 적용하지 않음
        if (isDead || target == null || rb == null) return;

        // 넉백 적용: 플레이어 반대 방향으로 밀림
        Vector2 knockBackDir = ((Vector2)transform.position - (Vector2)target.position).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockBackDir * knockBackForce, ForceMode2D.Impulse);

        if (gunPivot != null)
        {
            gunPivot.localPosition = new Vector3(-0.71f, -1.35f, 0f);
        }
    }

    protected override NodeState HandleDeath()
    {
        // 기존 사망 로직 그대로 실행
        var state = base.HandleDeath();

        //중력 적용
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 20f;           // 중력 강하게 설정
            rb.freezeRotation = false;      // 회전 허용
            rb.angularDrag = 1f;            // 회전 감속

            //회전도 랜덤하게 부여
            rb.angularVelocity = Random.Range(-250f, 250f);
        }

        return state;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, retreatRange);
    }
}
