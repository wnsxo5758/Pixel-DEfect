using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBT : MonoBehaviour
{
    protected BehaviorTree behaviorTree;
    protected Blackboard blackboard;

    [Header("체력 설정")] 
    [SerializeField] protected int maxHp = 3;
    [SerializeField] protected float normalStunDuration = 0.5f; // 일반 공격 스턴 시간
    [SerializeField] protected float throwStunDuration = 3f; // 던지기 공격 스턴 시간
    [SerializeField] protected float knockBackForce = 3f; // 넉백 힘
    [SerializeField] protected float deathDelay = 2f;
    
    [Header("기본 AI 설정")]
    [SerializeField] protected float detectionRange; // 플레이어 인지 거리
    [SerializeField] protected float loseTargetRange; // 추적 최대 거리
    [SerializeField] protected Transform target; // 플레이어 타깃
    [SerializeField] protected LayerMask targetLayer; // 플레이어 레이어

    [Header("패트롤 설정")] 
    [SerializeField] protected float patrolDirection = 1f; // 초기 패트롤 방향

    [Header("벽 감지 설정")]
    [SerializeField] protected float wallCheckDistance = 0.5f;
    [SerializeField] protected Vector2 wallCheckOffset = new Vector2(0.5f, 0);
    [SerializeField] protected Vector2 groundCheckOffset = new Vector2(0.5f, -0.5f);
    [SerializeField] protected LayerMask wallLayer; // 벽 레이어
    [SerializeField] protected LayerMask groundLayer; // 지면 레이어
    
    protected Rigidbody2D rb;
    protected MovementRigidbody2D movement;
    protected EnemyAnimator animator;
    protected Collider2D enemyCollider;
    
    // 상태 변수
    protected int currentHp;
    protected bool isHit = false;
    protected bool isDead = false;
    protected float stunTimer = 0f;
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<MovementRigidbody2D>();
        animator = GetComponentInChildren<EnemyAnimator>();
        
        blackboard = new Blackboard();
        
        // 초기 체력 설정
        currentHp = maxHp;
        
        // 블랙보드에 초기 데이터 설정
        blackboard.SetValue("Target", target);
        blackboard.SetValue("PlayerDetected", false);
        blackboard.SetValue("PatrolDirection", patrolDirection);
        blackboard.SetValue("IsHit", false);
        blackboard.SetValue("IsDead", false);
        blackboard.SetValue("StunTimer", 0f);
        blackboard.SetValue("CurrentHp", currentHp);
        blackboard.SetValue("MaxHp", maxHp);
        
        // 초기 방향 설정
        SetDirection(patrolDirection);
    }

    protected virtual void Start()
    {
        // 기본 행동 트리 설정
        SetupBaseBehaviorTree();
    }

    protected virtual void Update()
    {
        if (isDead) return;

        // 피격 상태 처리
        if (isHit)
        {
            stunTimer -= Time.deltaTime;
            blackboard.SetValue("StunTimer", stunTimer);

            if (stunTimer <= 0)
            {
                isHit = false;
                blackboard.SetValue("IsHit", false);
            }
        }
        
        // 타깃 감지
        if (!isHit)
        {
            DetectTarget();
        }
        
        // Behavior Tree 평가
        if (behaviorTree != null)
        {
            behaviorTree.Evaluate();
        }
    }
    
    // 기본 행동 트리 설정
    private void SetupBaseBehaviorTree()
    {
        // 루트 노드 (셀렉터)
        Selector rootSelector = new Selector();
        
        // 사망 시퀀스 (최우선)
        Sequence deathSequence = new Sequence();
        ConditionNode isDeadCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsDead"));
        ActionNode deadAction = new ActionNode(HandleDeath);
        deathSequence.AddChild(isDeadCondition);
        deathSequence.AddChild(deadAction);
        
        // 피격 시퀀스 (다음 우선순위)
        Sequence hitSequence = new Sequence();
        ConditionNode isHitCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsHit"));
        ActionNode hitAction = new ActionNode(HandleHit);
        hitSequence.AddChild(isHitCondition);
        hitSequence.AddChild(hitAction);
        
        // 공격 시퀀스 (하위 클래스에서 구현)
        Node attackSequence = CreateAttackSequence();
        
        // 추적 시퀀스
        Sequence chaseSequence = new Sequence();
        ConditionNode isPlayerDetected = new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected"));
        ActionNode chaseAction = new ActionNode(ChaseTarget);
        chaseSequence.AddChild(isPlayerDetected);
        chaseSequence.AddChild(chaseAction);
        
        // 패트롤 시퀀스
        Sequence patrolSequence = new Sequence();
        ActionNode patrolAction = new ActionNode(Patrol);
        patrolSequence.AddChild(patrolAction);
        
        // 트리 구성 (우선순위 순)
        rootSelector.AddChild(deathSequence);   // 사망 상태 (최우선)
        rootSelector.AddChild(hitSequence);     // 피격 상태 (다음 우선순위)
        rootSelector.AddChild(attackSequence);  // 공격 가능하면 공격
        rootSelector.AddChild(chaseSequence);   // 공격 불가능하면 추적
        rootSelector.AddChild(patrolSequence);  // 추적 불가능하면 패트롤
        
        // Behavior Tree 생성
        behaviorTree = new BehaviorTree(rootSelector)
        {
            Blackboard = blackboard
        };
    }
    
    // 하위 클래스에서 오버라이드할 공격 시퀀스 생성 메서드
    protected virtual Node CreateAttackSequence()
    {
        return new ConditionNode(() => false);
    }

    // 타겟 감지 메서드
    protected virtual void DetectTarget()
    {
        // 피격 중이거나 사망 상태면 타겟 감지하지 않음
        if (isHit || isDead) return;
        
        if (target == null)
        {
            // 플레이어 자동 탐색
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, 
                                                    detectionRange, targetLayer);
            if (playerCollider != null)
            {
                target = playerCollider.transform;
                blackboard.SetValue("Target", target);
            }
        }
        else
        {
            // 타겟 거리 확인
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            
            // 시야 내에 있는지 확인 (레이캐스트)
            bool canSeeTarget = false;
            if (distanceToTarget <= detectionRange)
            {
                Vector2 directionToTarget = (target.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget,
                                    detectionRange, targetLayer);

                if (hit.collider != null && hit.collider.transform == target)
                {
                    canSeeTarget = true;
                }
            }
            
            // 타겟을 너무 멀리 쫒아가면 포기
            if (distanceToTarget > loseTargetRange)
            {
                canSeeTarget = false;
            }
            
            // 블랙보드 업데이트
            blackboard.SetValue("PlayerDetected", canSeeTarget);
        }
    }
    
    // 타겟 추적 메서드
    protected virtual NodeState ChaseTarget()
    {
        // 피격 중이거나 사망 상태면 추적하지 않음
        if (isHit || isDead) return NodeState.Failure;
        
        Transform currentTarget = blackboard.GetValue<Transform>("Target");

        if (currentTarget == null)
        {
            return NodeState.Failure;
        }
        
        // 타겟 방향으로 이동
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        float moveDirection = Mathf.Sign(direction.x);
        
        // 방향 설정
        if (moveDirection != 0)
        {
            SetDirection(moveDirection);
        }
        
        //벽 체크
        bool isWallAhead = CheckWall(moveDirection);
        if (isWallAhead)
        {
            // 벽이 있으면 이동 중지
            moveDirection = 0;
        }

        if (movement != null)
        {
            movement.MoveTo(moveDirection);
        }

        if (animator != null)
        {
            
        }

        return NodeState.Running;
    }

    // 패트롤 메서드
    protected virtual NodeState Patrol()
    {
        // 피격 중이거나 사망 상태면 패트롤하지 않음
        if (isHit || isDead) return NodeState.Failure;
        
        float direction = blackboard.GetValue<float>("PatrolDirection");
        
        // 벽, 땅 체크
        bool isWallAhead = CheckWall(direction);
        bool isGroundAhead = CheckGround(direction);

        // 벽이 있거나 바닥이 없으면 방향 전환
        if (isWallAhead || !isGroundAhead)
        {
            direction *= -1;
            blackboard.SetValue("PatrolDirection", direction);
            SetDirection(direction);
        }
        
        // 이동
        if (movement != null)
        {
            movement.MoveTo(direction);
        }

        if (animator != null)
        {
            
        }

        return NodeState.Running;
    }
    
    // 피격 처리 메서드
    protected virtual NodeState HandleHit()
    {
        // 움직임 멈춤
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        else if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // 피격 애니메이션 재생
        if (animator != null)
        {
            
        }

        return NodeState.Running;
    }
    
    //사망 처리 메서드
    protected virtual NodeState HandleDeath()
    {
        // 이미 사망 처리 완료됐으면 Success 반환
        if (!gameObject.activeSelf) return NodeState.Success;
        
        // 움직임 멈춤
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        else if (rb != null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        
        // 사망 애니메이션 재생
        if (animator != null)
        {
            
        }
        
        // 오브젝트 제거 (딜레이 적용)
        StartCoroutine(DestroyAfterDelay(deathDelay));
        
        return NodeState.Success;
    }

    // 딜레이 후 오브젝트 제거
    protected IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 오브젝트 풀링 비활성화
        gameObject.SetActive(false);
    }
    
    // 데미지 적용 메서드
    public virtual void DecreaseHp(int damage, bool isThrownWeapon = false)
    {
        if (isDead) return;

        currentHp -= damage;
        blackboard.SetValue("CurrentHp", currentHp);
        
        // 사망 체크
        if (currentHp <= 0)
        {
            currentHp = 0;
            isDead = true;
            blackboard.SetValue("IsDead", true);
            return;
        }
        
        // 피격 상태 설정
        isHit = true;
        blackboard.SetValue("IsHit", true);
        
        // 스턴 타이머 설정 (던진 무기인 경우 더 긴 스턴)
        stunTimer = isThrownWeapon ? throwStunDuration : normalStunDuration;
        blackboard.SetValue("StunTimer", stunTimer);
        
        // 넉백 적용 (피격 방향의 반대로)
        if (target != null && rb != null)
        {
            Vector2 knockBackDirection = ((Vector2)transform.position - (Vector2)target.position).normalized;
            rb.velocity = Vector2.zero;
            rb.AddForce(knockBackDirection * knockBackForce, ForceMode2D.Impulse);
        }
    }
    
    // 방향 설정 메서드
    protected virtual void SetDirection(float direction)
    {
        if (direction != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);
            transform.localScale = scale;
        }
    }

    protected float GetDirection()
    {
        return Mathf.Sign(transform.localScale.x);
    }
    
    // 벽 체크 메서드
    protected virtual bool CheckWall(float direction)
    {
        Vector2 originPos = (Vector2)transform.position + 
                            new Vector2(wallCheckOffset.x * direction, wallCheckOffset.y);
        
        RaycastHit2D hit = Physics2D.Raycast(originPos, new Vector2(direction, 0), wallCheckDistance, wallLayer);
        Debug.DrawRay(originPos, new Vector2(direction, 0), hit ? Color.red : Color.green);

        return hit;
    }
    
    // 땅 체크 메서드 (낭떠러지 감지)
    protected virtual bool CheckGround(float direction)
    {
        Vector2 originPos = (Vector2)transform.position +
                            new Vector2(groundCheckOffset.x * direction, groundCheckOffset.y);
        
        RaycastHit2D hit = Physics2D.Raycast(originPos, Vector2.down, wallCheckDistance, groundLayer);
        Debug.DrawRay(originPos, Vector2.down * wallCheckDistance, hit ? Color.green : Color.red);

        return hit;
    }
    
    // 거리 계산 메서드
    protected float DistanceToTarget()
    {
        Transform currentTarget = blackboard.GetValue<Transform>("Target");

        if (currentTarget == null)
        {
            return float.MaxValue;
        }

        return Vector2.Distance(transform.position, currentTarget.position);
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
    }
}
