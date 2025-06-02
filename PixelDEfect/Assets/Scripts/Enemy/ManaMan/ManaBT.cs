using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBT : EnemyBT
{

    [Header("상태 관련")]
    [SerializeField]
    private bool isPatrol = false;

    private readonly HashSet<string> ignoredStates = new HashSet<string>
    {
        "Crawl",
        "Attack"
    };


    [Header("스킬 관련")]
    [SerializeField]
    protected float skillRange = 4f; // 스킬 사용 거리
    [SerializeField]
    protected float skillCooldown = 5f; // 
    [SerializeField]
    protected AudioClip skillClip;


    protected bool canUseSkill = true;
    protected float skillTimer = 0f;
    protected ManaAnimator manaAnimator;

    [Header("점프 관련")]
    [SerializeField]
    private float jumpHeight = 10f;
    [SerializeField]
    private float jumpForwardOffset = 0.3f;
    [SerializeField]
    private LayerMask canJumpLayer;
    protected override void Awake()
    {
        base.Awake();
        manaAnimator = GetComponentInChildren<ManaAnimator>();
        blackboard.SetValue("IsPatrol", isPatrol);
    }
    protected override void Update()
    {
        base.Update();

        if (!canUseSkill)
        {
            skillTimer += Time.deltaTime;
            if (skillTimer >= skillCooldown)
            {
                skillTimer = 0f;
                canUseSkill = true;
            }
        }
    }
    protected override void SetupBaseBehaviorTree() //BT구성
    {
        // 루트 셀렉터
        Selector rootSelector = new Selector();

        // 사망 시퀀스
        Sequence deathSequence = new Sequence();
        deathSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("IsDead")));
        deathSequence.AddChild(new ActionNode(HandleDeath));

        // 피격 시퀀스
        Sequence hitSequence = new Sequence();
        hitSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("IsHit")));
        hitSequence.AddChild(new ActionNode(HandleHit));

        // 스킬 시퀀스
        Node skillSequence = CreateSkillSequence();

        // 공격 중 시퀀스
        Sequence attackingSequence = new Sequence();
        attackingSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("IsAttacking")));
        attackingSequence.AddChild(new ActionNode(MaintainAttackState));

        // 공격 시퀀스
        Node attackSequence = CreateAttackSequence();

        // 추적 시퀀스
        Sequence chaseSequence = new Sequence();
        chaseSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected")));
        chaseSequence.AddChild(new ActionNode(ChaseTarget));

        // 순찰 시퀀스
        Sequence patrolSequence = new Sequence();
        ConditionNode patrolEnabledCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsPatrol"));
        patrolSequence.AddChild(patrolEnabledCondition);
        patrolSequence.AddChild(new ActionNode(Patrol));

        // 대기 시퀀스
        Sequence idleSequence = new Sequence();
        idleSequence.AddChild(new ConditionNode(() => !blackboard.GetValue<bool>("IsPatrol")));
        idleSequence.AddChild(new ActionNode(Idle));


        // 트리 구성 순서
        rootSelector.AddChild(deathSequence);
        rootSelector.AddChild(hitSequence);
        rootSelector.AddChild(skillSequence);
        rootSelector.AddChild(attackingSequence);
        rootSelector.AddChild(attackSequence);
        rootSelector.AddChild(chaseSequence);
        rootSelector.AddChild(patrolSequence);
        rootSelector.AddChild(idleSequence);


        behaviorTree = new BehaviorTree(rootSelector)
        {
            Blackboard = blackboard
        };
    }

    protected virtual NodeState Idle() // 대기 상태(앉아서 대기)
    {
        if (isDead || isHit) return NodeState.Failure;

        movement.MoveTo(0);
        animator?.SetMovementAnim(0f);
        // Debug.Log("대기 중...");
        return NodeState.Running;
    }
    protected override void DetectTarget()
    {
        if (isHit || isDead) return;

        bool previouslyDetected = blackboard.GetValue<bool>("PlayerDetected");
        bool currentlyDetected = previouslyDetected;

        if (target == null)
        {
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, targetLayer);
            if (playerCollider != null)
            {
                target = playerCollider.transform;
                blackboard.SetValue("Target", target);
            }
        }
        else
        {
            //플레이어 상태 감지
            PlayerController player = target.GetComponent<PlayerController>();
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (player != null)
            {
                string playerState = player.GetCurrentStateName();
                if (ignoredStates.Contains(playerState)) // HashSet<string> ignoredStates는 클래스 필드로 선언
                {
                    Debug.Log($"[ManaBT] 플레이어 상태 '{playerState}' 감지 무시됨");
                    currentlyDetected = false;
                }
                else if (previouslyDetected)
                    {
                        //if (distanceToTarget > loseTargetRange)
                        //    currentlyDetected = false;
                        //else
                            currentlyDetected = true;
                    }
                else
                {
                    if (distanceToTarget <= detectionRange)
                    {
                        Vector2 dirToTarget = (target.position - transform.position).normalized;
                        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToTarget, detectionRange, targetLayer);
                        Debug.DrawRay(transform.position, dirToTarget * detectionRange, Color.red);

                        if (hit.collider != null && hit.collider.transform == target)
                            currentlyDetected = true;
                    }
                }
            }
        }
        blackboard.SetValue("PlayerDetected", currentlyDetected);


        // 상태 변화가 감지되었을 때 처리
        if (previouslyDetected != currentlyDetected)
        {
            if (animator != null)
            {
                animator.SetChasingState(currentlyDetected);
            }

            // Idle 상태 → 플레이어 감지 → Patrol 상태로 전환
            if (currentlyDetected && !isPatrol)
            {
                Debug.Log("[DetectTarget] 플레이어 감지됨. Idle → Patrol 전환");
                isPatrol = true;
                blackboard.SetValue("IsPatrol", true);
                manaAnimator?.TriggerPatrolAnim();
            }

            // 추적 포기 시
            if (!currentlyDetected && previouslyDetected)
            {
                HandleLostTarget();
            }
        }
    }

    protected virtual NodeState UseSkill() // 스킬 사용
    {
        Debug.Log("기본 스킬 사용 - 하위 클래스에서 오버라이드 필요");

        // 쿨타임 초기화
        canUseSkill = false;
        skillTimer = 0f;

        return NodeState.Running;
    }
    protected virtual Node CreateSkillSequence()
    {
        Sequence skillSequence = new Sequence();

        skillSequence.AddChild(new ConditionNode(() => !isDead && !isHit));
        skillSequence.AddChild(new ConditionNode(() => IsTargetInSkillRange()));
        skillSequence.AddChild(new ConditionNode(() => canUseSkill));
        skillSequence.AddChild(new ActionNode(UseSkill));

        return skillSequence;
    }

    protected virtual bool IsTargetInSkillRange() // 스킬 범위 내 플레이어 확인
    {
        if (!blackboard.GetValue<bool>("PlayerDetected")) return false;

        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.position) <= skillRange;
    }

    protected override NodeState Patrol()// 순찰
    {
        if (isHit || isDead) return NodeState.Failure;
        manaAnimator.TriggerPatrolAnim();
        animator?.SetChasingState(false);

        float direction = blackboard.GetValue<float>("PatrolDirection");

        if (float.IsNaN(direction) || direction == 0f)
        {
            direction = 1f;
            blackboard.SetValue("PatrolDirection", direction);
        }

        if (CheckWall(direction))
        {
            bool hitEnemyWall = blackboard.GetValue<bool>("HitEnemyWallCheck");

            if (hitEnemyWall)
            {
                // 그냥 뒤돌기
                direction *= -1;
                blackboard.SetValue("PatrolDirection", direction);
                SetDirection(direction);
                movement.MoveTo(0);
                return NodeState.Running;
            }

            // 점프 가능한 경우
            if (CanJumpOverWall())
            {
                movement.Jump();
                return NodeState.Running;
            }

            // 점프도 안되면 뒤돌기
            direction *= -1;
            blackboard.SetValue("PatrolDirection", direction);
            SetDirection(direction);
            movement.MoveTo(0);
            return NodeState.Running;
        }

        movement.MoveTo(direction);
        animator?.SetMovementAnim(Mathf.Abs(direction));
        return NodeState.Running;
    }

    protected override bool CheckWall(float direction)
    {
        Vector2 originPos = (Vector2)transform.position +
                            new Vector2(wallCheckOffset.x * direction, wallCheckOffset.y);

        RaycastHit2D hit = Physics2D.Raycast(originPos, new Vector2(direction, 0), wallCheckDistance, wallLayer);
        Debug.DrawRay(originPos, new Vector2(direction, 0) * wallCheckDistance, hit ? Color.red : Color.green);

        // EnemyWallCheck 레이어에 닿았으면 감지는 하되, 블랙보드에 기록
        if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("EnemyWall"))
        {
            blackboard.SetValue("HitEnemyWallCheck", true);
        }
        else
        {
            blackboard.SetValue("HitEnemyWallCheck", false);
        }

        return hit;
    }
    protected override NodeState ChaseTarget() // 플레이어 추적
    {
        if (isHit || isDead) return NodeState.Failure;
        manaAnimator.TriggerPatrolAnim();
        Transform currentTarget = blackboard.GetValue<Transform>("Target");
        if (currentTarget == null) return NodeState.Failure;

        float distance = Mathf.Abs(currentTarget.position.x - transform.position.x);

        // 1. 충분히 가까워졌다면 멈추고 상태 유지
        if (distance < 0.2f) // ← 오차 허용 (값은 속도/프레임에 따라 조정)
        {
            movement.MoveTo(0); // 속도 0 처리
            return NodeState.Running; // 또는 Running, 상황에 맞게
        }

        float direction = Mathf.Sign((currentTarget.position - transform.position).x);
        SetDirection(direction);

        // 점프 시도
        if (CheckWall(direction))
        {
            // 점프 가능한 경우
            if (CanJumpOverWall())
            {
                movement.Jump();
                return NodeState.Running;
            }
        }

        movement.MoveToFast(direction);
        return NodeState.Running;
    }

    //점프 관련 함수들
    private bool CanJumpOverWall() // 점프 확인
    {
        if (movement == null) return false;

        float dir = GetDirection();
        Vector2 origin = transform.position;

        // 1. 벽 앞 위치 계산
        Vector2 wallCheckOrigin = origin + new Vector2(wallCheckDistance * dir, 0.1f);

        // ✅ 2. 점프 착지 예상 위치: 벽 옆으로 충분히 떨어진 위치
        Vector2 jumpLandingPos = wallCheckOrigin + new Vector2(jumpForwardOffset * dir, 0);

        // 3. 그 위 공간 체크 위치
        Vector2 topCheck = jumpLandingPos + new Vector2(0, jumpHeight);

        // ✅ 4. 모든 레이어 대상 검사
        Collider2D topCollider = Physics2D.OverlapCircle(topCheck, 0.15f, ~0);

        // 디버그 시각화
        Debug.DrawLine(topCheck + Vector2.left * 0.1f, topCheck + Vector2.right * 0.1f, Color.cyan, 0.2f);

        if (topCollider == null)
        {
            Debug.Log("[점프 체크] 벽 옆 착지 위치 위 공간 비어 있음 → 점프 가능");
            return true;
        }
        else
        {
            Debug.Log($"[점프 체크] 점프 도착 지점 위 공간 막힘: {topCollider.name} → 점프 불가");
            return false;
        }
    }

    protected override void OnDrawGizmosSelected() // 범위 확인
    {
        base.OnDrawGizmosSelected();

        float dir = Application.isPlaying ? GetDirection() : 1f;
        Vector2 position = transform.position;

        // 벽 감지 위치
        Vector2 wallCheckOrigin = position + new Vector2(wallCheckDistance * dir, 0.1f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(wallCheckOrigin, wallCheckOrigin + Vector2.right * dir * 0.1f);

        // 점프 가능성 확인 위치 (벽보다 더 앞쪽)
        Vector2 jumpCheckOrigin = wallCheckOrigin + new Vector2(jumpForwardOffset * dir, 0);
        Vector2 topCheck = jumpCheckOrigin + new Vector2(0, jumpHeight);

        // 점프 감지 선 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(jumpCheckOrigin, topCheck);

        // 점프 확인 지점 (초록 원)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(topCheck, 0.15f);

    }

}