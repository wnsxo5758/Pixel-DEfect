using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBT : EnemyBT
{

    [Header("스킬 관련")]
    [SerializeField] protected float skillRange = 4f; // 스킬 사용 거리
    [SerializeField] protected float skillCooldown = 5f; // 

    protected bool canUseSkill = true;
    protected float skillTimer = 0f;

    protected ManaAnimator manaAnimator;

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
    protected override void SetupBaseBehaviorTree()
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
    protected virtual NodeState UseSkill()
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

    protected virtual bool IsTargetInSkillRange()
    {
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.position) <= skillRange;
    }

    protected override NodeState Patrol()
    {
        if (isHit || isDead) return NodeState.Failure;

        animator?.SetChasingState(false);

        float direction = blackboard.GetValue<float>("PatrolDirection");

        if (float.IsNaN(direction) || direction == 0f)
        {
            direction = 1f;
            blackboard.SetValue("PatrolDirection", direction);
        }

        // 앞에 벽이 있을 경우 방향 전환
        if (CheckWall(direction))
        {
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


    protected override NodeState ChaseTarget()
    {
        if (isHit || isDead) return NodeState.Failure;

        Transform currentTarget = blackboard.GetValue<Transform>("Target");
        if (currentTarget == null) return NodeState.Failure;

        float direction = Mathf.Sign((currentTarget.position - transform.position).x);
        SetDirection(direction);

        // 앞에 벽이 있을 경우 정지
        if (CheckWall(direction))
        {
            movement.MoveTo(0);
            return NodeState.Running;
        }

        movement.MoveToFast(direction);
        return NodeState.Running;
    }


    protected override void DetectTarget()
    {
        if (isHit || isDead) return;

        if (ShouldIgnoreTarget())
        {
            blackboard.SetValue("PlayerDetected", false);
            return;
        }

        base.DetectTarget();
    }

    protected virtual bool ShouldIgnoreTarget()
    {
        if (target == null) return false;

        // TODO: Player 상태를 체크하는 로직 삽입 예정
        return false;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        float dir = Application.isPlaying ? GetDirection() : 1f;

        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.6f); // 연한 하늘색
        Gizmos.DrawWireSphere(transform.position, skillRange);


    }

}