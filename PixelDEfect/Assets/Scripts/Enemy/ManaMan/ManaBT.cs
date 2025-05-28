using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBT : EnemyBT
{

    [Header("스킬 관련")]
    [SerializeField]
    protected float skillRange = 4f; // 스킬 사용 거리
    [SerializeField]
    protected float skillCooldown = 5f; // 

    protected bool canUseSkill = true;
    protected float skillTimer = 0f;
    protected ManaAnimator manaAnimator;

    [Header("점프 관련")]
    [SerializeField]
    private float jumpHeight = 10f;
    [SerializeField]
    private float jumpForwardOffset = 0.3f;
    protected override void Awake()
    {
        base.Awake();
        manaAnimator = GetComponentInChildren<ManaAnimator>();
    }
    protected override void Update()
    {
        base.Update();

        if(!canUseSkill)
        {
            skillTimer += Time.deltaTime;
            if(skillTimer >= skillCooldown)
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

        // 스킬 시퀀스 (여기만 ManaBT 추가)
        Node skillSequence = CreateSkillSequence();

        // 공격 중 시퀀스
        Sequence attackingSequence = new Sequence();
        attackingSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("IsAttacking")));
        attackingSequence.AddChild(new ActionNode(MaintainAttackState));

        // 공격 시퀀스 (하위 클래스에서 생성)
        Node attackSequence = CreateAttackSequence();

        // 추적 시퀀스
        Sequence chaseSequence = new Sequence();
        chaseSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected")));
        chaseSequence.AddChild(new ActionNode(ChaseTarget));

        // 패트롤 시퀀스
        Sequence patrolSequence = new Sequence();
        patrolSequence.AddChild(new ActionNode(Patrol));

        // 우선순위에 따라 트리 구성
        rootSelector.AddChild(deathSequence);
        rootSelector.AddChild(hitSequence);
        rootSelector.AddChild(skillSequence);      // 
        rootSelector.AddChild(attackingSequence);
        rootSelector.AddChild(attackSequence);
        rootSelector.AddChild(chaseSequence);
        rootSelector.AddChild(patrolSequence);

        // 트리 생성
        behaviorTree = new BehaviorTree(rootSelector)
        {
            Blackboard = blackboard
        };
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
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.position) <= skillRange;
    }

    protected override NodeState Patrol()// 순찰
    {
        if (isHit || isDead) return NodeState.Failure;

        animator?.SetChasingState(false);

        float direction = blackboard.GetValue<float>("PatrolDirection");

        if (float.IsNaN(direction) || direction == 0f)
        {
            direction = 1f;
            blackboard.SetValue("PatrolDirection", direction);
        }

        // 점프 시도

        // 앞에 벽이 있을 경우 방향 전환
        if (CheckWall(direction))
        {

            if (CanJumpOverWall())
            {
                movement.Jump();
                return NodeState.Running;
            }


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


    protected override NodeState ChaseTarget() // 플레이어 추적
    {
        if (isHit || isDead) return NodeState.Failure;

        Transform currentTarget = blackboard.GetValue<Transform>("Target");
        if (currentTarget == null) return NodeState.Failure;

        float direction = Mathf.Sign((currentTarget.position - transform.position).x);
        SetDirection(direction);

        // 점프 시도
        if (CanJumpOverWall())
        {
            movement.Jump();
            return NodeState.Running;
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

        // 벽 감지 위치와 점프 감지 시작 위치를 동일하게 설정
        Vector2 wallCheckOrigin = origin + new Vector2(wallCheckDistance * dir, 0.1f);
        // 2. 벽 위 공간 감지 (OverlapCircle)
        Vector2 jumpCheckOrigin = wallCheckOrigin + new Vector2(jumpForwardOffset * dir, 0);
        Vector2 topCheck = jumpCheckOrigin + new Vector2(0, jumpHeight);
        Collider2D topCollider = Physics2D.OverlapCircle(topCheck, 0.15f, movement.GroundCheckLayer);

        if (topCollider == null)
        {
            Debug.Log("[점프 체크] 위 공간 비어 있음 → 점프 가능");
            return true;
        }
        else
        {
            Debug.Log("[점프 체크] 위 공간 막혀 있음 → 점프 불가");
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