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

    [SerializeField] private float hitTiltAngle = 45f;

    private Quaternion originalRotation;
    private bool isTilted = false;

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
            initialGunRotation = gunPivot.rotation;
        }
        originalRotation = transform.localRotation;

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
        SetupFlyRangeBehaviorTree();
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
        base.Update(); // EnemyBT의 Update 호출

        if (!isHit && isTilted)
        {
            transform.localRotation = originalRotation;
            isTilted = false;
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
        Vector2 fireDirection = (firePoint.position - gunPivot.position).normalized;

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
        if (isDead) return;

        currentHp -= damage;
        blackboard.SetValue("CurrentHp", currentHp);

        if (currentHp <= 0)
        {
            currentHp = 0;
            isDead = true;
            blackboard.SetValue("IsDead", true);

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }

            return;
        }

        isHit = true;
        blackboard.SetValue("IsHit", true);

        stunTimer = isThrownWeapon ? throwStunDuration : normalStunDuration;
        blackboard.SetValue("StunTimer", stunTimer);

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashEffect());
        }

        if (!isThrownWeapon)
        {
            float tiltDir = transform.position.x - target.position.x;
            tiltDir = tiltDir == 0 ? 1f : Mathf.Sign(tiltDir);

            transform.localRotation = Quaternion.Euler(0f, 0f, hitTiltAngle * -tiltDir);
            isTilted = true;
        }

        // 넉백 적용
        if (target != null && rb != null && !isThrownWeapon)
        {
            float dirX = transform.position.x - target.position.x;
            dirX = dirX == 0 ? 1f : Mathf.Sign(dirX);
            Vector2 knockBack = new Vector2(dirX, 0f).normalized * knockBackForce;

            rb.velocity = Vector2.zero;
            rb.AddForce(knockBack, ForceMode2D.Impulse);
        }
    }

    protected override NodeState HandleHit()
    {
        // 움직임 중지 생략하여 넉백 유지
        if (animator != null)
        {
            animator.SetMovementAnim(0);
            animator.TriggerHitAnim();
        }

        return NodeState.Running;
    }

    protected override NodeState HandleDeath()
    {
        var state = base.HandleDeath();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 25f;
            rb.freezeRotation = false;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.angularDrag = 1f;

            // 방향 확인
            float dirX = transform.localScale.x;

            //회전 방향: 오른쪽 보면 음수(시계방향), 왼쪽 보면 양수(반시계)
            float spinDirection = dirX >= 0 ? -1f : 1f;
            rb.angularVelocity = spinDirection * 300f; // 꼬꾸라지는 느낌용 속도

        }

        return state;
    }
    private void OnDrawGizmos()
    {
        if (firePoint != null && gunPivot != null)
        {
            if (firePoint == null || gunPivot == null)
                return;

            Gizmos.color = Color.black;

            // firePoint → 총기 끝 방향 기준으로 선 그리기
            Vector3 direction = (firePoint.position - gunPivot.position).normalized;
            float length = 20f;

            Gizmos.DrawLine(firePoint.position, firePoint.position + direction * length);
        }
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
