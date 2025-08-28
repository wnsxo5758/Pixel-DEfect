using System.Collections;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    protected BehaviorTree behaviorTree;
    protected Blackboard blackboard;

    [Header("기본 AI 설정")]
    [SerializeField] protected float detectionRange;
    [SerializeField] protected float loseTargetRange;
    [SerializeField] protected Transform target;
    [SerializeField] protected LayerMask targetLayer;

    [Header("패트롤")]
    [SerializeField] protected float patrolDirection = 1f;

    [Header("벽/땅 감지")]
    [SerializeField] protected float wallCheckDistance = 0.5f;
    [SerializeField] protected Vector2 wallCheckOffset = new Vector2(0.5f, 0);
    [SerializeField] protected Vector2 groundCheckOffset = new Vector2(0.5f, -0.5f);
    [SerializeField] protected LayerMask wallLayer;
    [SerializeField] protected LayerMask groundLayer;

    [Header("사망 처리")]
    [SerializeField] protected float deathDelay = 2f;

    protected Rigidbody2D rb;
    protected MovementRigidbody2D movement;
    protected EnemyAnimator animator;
    protected Collider2D enemyCollider;

    protected bool isDeathProcessed = false;
    protected bool isTimeFrozen = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<MovementRigidbody2D>();
        animator = GetComponentInChildren<EnemyAnimator>();
        enemyCollider = GetComponent<Collider2D>();

        blackboard = GetComponent<Blackboard>(); // EnemyHp가 RequireComponent로 붙여줌
        if (blackboard == null) blackboard = new Blackboard();

        // 초기 블랙보드
        blackboard.SetValue("Target", target);
        blackboard.SetValue("PlayerDetected", false);
        blackboard.SetValue("PatrolDirection", patrolDirection);

        SetDirection(patrolDirection);
    }

    protected virtual void Start()
    {
        SetupBaseBehaviorTree();
    }

    protected virtual void Update()
    {
        if (isTimeFrozen || isDeathProcessed) return;

        // 피격/사망 여부는 EnemyHp가 블랙보드에 기록
        bool isDead = blackboard.GetValue<bool>("IsDead");
        if (!isDead)
        {
            DetectTarget();
        }

        behaviorTree?.Evaluate();
    }

    protected virtual void SetupBaseBehaviorTree()
    {
        var rootSelector = new Selector();

        // 사망
        var deathSeq = new Sequence();
        deathSeq.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("IsDead")));
        deathSeq.AddChild(new ActionNode(HandleDeath));

        // 피격 유지 (스턴 동안 이동/공격 정지)
        var hitSeq = new Sequence();
        hitSeq.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("IsHit")));
        hitSeq.AddChild(new ActionNode(HandleHit));

        // 공격/추적/패트롤
        var attackingSeq = new Sequence();
        attackingSeq.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("IsAttacking")));
        attackingSeq.AddChild(new ActionNode(MaintainAttackState));

        var attackSeq = CreateAttackSequence();

        var chaseSeq = new Sequence();
        chaseSeq.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected")));
        chaseSeq.AddChild(new ActionNode(ChaseTarget));

        var patrolSeq = new Sequence();
        patrolSeq.AddChild(new ActionNode(Patrol));

        rootSelector.AddChild(deathSeq);
        rootSelector.AddChild(hitSeq);
        rootSelector.AddChild(attackingSeq);
        rootSelector.AddChild(attackSeq);
        rootSelector.AddChild(chaseSeq);
        rootSelector.AddChild(patrolSeq);

        behaviorTree = new BehaviorTree(rootSelector) { Blackboard = blackboard };
    }

    protected virtual Node CreateAttackSequence() => new ConditionNode(() => false);

    protected virtual NodeState HandleHit()
    {
        // 공격 중이면 종료
        if (blackboard.GetValue<bool>("IsAttacking")) OnAttackAnimationFinished();

        // 이동 정지
        if (movement != null) movement.MoveTo(0f);
        else if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);

        // 피격 애니메이션
        animator?.SetMovementAnim(0);
        animator?.TriggerHitAnim();

        return NodeState.Running;
    }

    protected virtual NodeState HandleDeath()
    {
        if (isDeathProcessed) return NodeState.Success;
        isDeathProcessed = true;

        if (movement != null) movement.MoveTo(0f);
        else if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);

        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        if (enemyCollider != null) enemyCollider.enabled = false;

        animator?.SetMovementAnim(0);
        animator?.SetChasingState(false);
        animator?.TriggerDeathAnim();

        StartCoroutine(DestroyAfterDelay(deathDelay));
        return NodeState.Success;
    }

    protected IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        var attachedWeapon = GetComponentInChildren<ThrownWeapon>();
        if (attachedWeapon != null)
            attachedWeapon.DetachFromEnemy(transform.position);

        gameObject.SetActive(false); // 풀 반환
    }

    protected virtual NodeState MaintainAttackState()
    {
        movement?.MoveTo(0f);
        return NodeState.Running;
    }

    // ===== 감지/이동/보조 =====

    protected virtual void DetectTarget()
    {
        if (blackboard.GetValue<bool>("IsHit") || blackboard.GetValue<bool>("IsDead"))
            return;

        bool prevDetected = blackboard.GetValue<bool>("PlayerDetected");
        bool curDetected = prevDetected;

        var t = blackboard.GetValue<Transform>("Target");
        if (t == null)
        {
            var col = Physics2D.OverlapCircle(transform.position, detectionRange, targetLayer);
            if (col != null)
            {
                t = col.transform;
                blackboard.SetValue("Target", t);
                curDetected = true;
            }
        }
        else
        {
            float dist = Vector2.Distance(transform.position, t.position);
            curDetected = prevDetected ? dist <= loseTargetRange : dist <= detectionRange;

            if (!prevDetected && curDetected)
            {
                // 가시선 체크 등을 추가하고 싶으면 여기서
            }
        }

        blackboard.SetValue("PlayerDetected", curDetected);

        if (prevDetected != curDetected)
        {
            animator?.SetChasingState(curDetected);
            if (!curDetected && prevDetected) HandleLostTarget();
        }
    }

    protected virtual void HandleLostTarget()
    {
        float dir = GetDirection();
        blackboard.SetValue("PatrolDirection", dir);
        if (CheckWall(dir))
        {
            dir *= -1f;
            blackboard.SetValue("PatrolDirection", dir);
            SetDirection(dir);
        }
    }

    protected virtual NodeState ChaseTarget()
    {
        if (blackboard.GetValue<bool>("IsHit") || blackboard.GetValue<bool>("IsDead"))
            return NodeState.Failure;

        var t = blackboard.GetValue<Transform>("Target");
        if (t == null)
        {
            animator?.SetChasingState(false);
            return NodeState.Failure;
        }

        animator?.SetChasingState(true);

        Vector2 dirVec = (t.position - transform.position).normalized;
        float dir = Mathf.Sign(dirVec.x);
        if (dir != 0) SetDirection(dir);

        bool inRange = IsTargetInAttackRange();
        if (inRange)
        {
            movement?.MoveTo(0f);
        }
        else
        {
            movement?.MoveToFast(dir);
        }

        return NodeState.Running;
    }

    protected virtual NodeState Patrol()
    {
        if (blackboard.GetValue<bool>("IsHit") || blackboard.GetValue<bool>("IsDead"))
            return NodeState.Failure;

        animator?.SetChasingState(false);

        float dir = blackboard.GetValue<float>("PatrolDirection");
        if (float.IsNaN(dir) || dir == 0f)
        {
            dir = 1f;
            blackboard.SetValue("PatrolDirection", dir);
        }

        if (CheckWall(dir))
        {
            dir *= -1f;
            blackboard.SetValue("PatrolDirection", dir);
            SetDirection(dir);
        }
        else
        {
            SetDirection(dir);
        }

        movement?.MoveTo(dir);
        animator?.SetMovementAnim(Mathf.Abs(dir));

        return NodeState.Running;
    }

    protected virtual void SetDirection(float direction)
    {
        if (direction == 0) return;
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * Mathf.Sign(direction);
        transform.localScale = s;
    }

    protected float GetDirection() => Mathf.Sign(transform.localScale.x);
    protected virtual bool CheckWall(float dir)
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(wallCheckOffset.x * dir, wallCheckOffset.y);
        var hit = Physics2D.Raycast(origin, new Vector2(dir, 0), wallCheckDistance, wallLayer);
        Debug.DrawRay(origin, new Vector2(dir, 0) * wallCheckDistance, hit ? Color.red : Color.green);
        return hit;
    }
    protected virtual bool CheckGround(float dir)
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(groundCheckOffset.x * dir, groundCheckOffset.y);
        var hit = Physics2D.Raycast(origin, Vector2.down, wallCheckDistance, groundLayer);
        Debug.DrawRay(origin, Vector2.down * wallCheckDistance, hit ? Color.green : Color.red);
        return hit;
    }

    protected virtual bool IsTargetInAttackRange() => false;
    protected virtual float GetAttackRange() => 0f;

    public virtual void FreezeTime()
    {
        isTimeFrozen = true;
        if (rb != null) { rb.velocity = Vector2.zero; rb.angularVelocity = 0f; rb.Sleep(); }
        movement?.MoveTo(0f);
        behaviorTree?.Pause();
    }

    public virtual void UnfreezeTime()
    {
        isTimeFrozen = false;
        rb?.WakeUp();
        behaviorTree?.Resume();
    }

    public virtual void OnAttackAnimationEvent() { }
    public virtual void OnAttackAnimationFinished() { }
}
