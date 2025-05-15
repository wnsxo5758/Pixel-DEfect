using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerRobotBoss : BossBT
{
    [Header("기본 공격 설정")] 
    [SerializeField] private float basicAttackRange = 3f;           // 기본 공격 범위
    [SerializeField] private float basicAttackCooldown = 5f;        // 기본 공격 쿨다운
    [SerializeField] private float attackPreparationTime = 1.5f;    // 공격 준비 시간
    [SerializeField] private float attackExecuteTime = 0.5f;        // 공격 실행 시간
    [SerializeField] private int basicAttackDamage = 2;             // 기본 공격 데미지

    [Header("공격 히트박스 설정")] 
    [SerializeField] private Transform attackPoint;                 // 공격 중심점
    [SerializeField] private Vector2 attackHitBoxSize = new Vector2(5f, 2f); // 공격 히트박스 크기
    [SerializeField] private Vector2 attackHitBoxOffset = new Vector2(1f, 0f); // 공격 히트박스 오프셋
    [SerializeField] private LayerMask playerLayer;                 // 플레이어 레이어
    
    // 공격 상태 관리
    private enum AttackState { None, Preparation, Execute, Cooldown }
    private AttackState currentAttackState = AttackState.None;
    private float attackStateTimer = 0f;
    private bool canBasicAttack = true;
    
    // 타이머 및 상태 관리 변수
    private float basicAttackTimer = 0f;
    
    // 패턴 액션 노드
    private ActionNode basicAttackNode;

    protected override void Awake()
    {
        base.Awake();
        
        // 블랙보드에 공격 관련 데이터 추가
        blackboard.SetValue("BasicAttackRange", basicAttackRange);
        blackboard.SetValue("CanBasicAttack", true);
        blackboard.SetValue("CurrentAttackState", (int)AttackState.None);
    }

    protected override void Start()
    {
        base.Start();
        
        // 히트박스 참조가 없으면 기본값 설정
        if (attackPoint == null)
        {
            // 빈 게임오브젝트 생성하여 공격 포인트로 사용
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.parent = transform;
            attackPointObj.transform.localPosition = Vector3.zero;
            attackPoint = attackPointObj.transform;
        }
    }

    protected override void Update()
    {
        base.Update();
        
        // 공격 쿨다운 관리
        if (!canBasicAttack)
        {
            basicAttackTimer += Time.deltaTime;
            if (basicAttackTimer >= basicAttackCooldown)
            {
                canBasicAttack = true;
                blackboard.SetValue("CanBasicAttack", true);
                basicAttackTimer = 0f;
            }
        }

        UpdateAttackState();
    }
    
    // 공격 상태 업데이트
    private void UpdateAttackState()
    {
        if (isStunned || isDead || isChangingPhase)
            return;

        switch (currentAttackState)
        {
            case AttackState.Preparation:
                attackStateTimer += Time.deltaTime;
                if (attackStateTimer >= attackPreparationTime)
                {
                    // 준비 완료, 공격 실행으로 전환
                    SetAttackState(AttackState.Execute);
                }
                break;
            
            case AttackState.Execute:
                attackStateTimer += Time.deltaTime;
                if (attackStateTimer >= attackExecuteTime)
                {
                    // 공격 종료, 쿨다운으로 전환
                    ExecuteAttack();
                    SetAttackState(AttackState.Cooldown);
                    canBasicAttack = false;
                    blackboard.SetValue("CanBasicAttack", false);
                    basicAttackTimer = 0f;
                }
                break;
            
            case AttackState.Cooldown:
                // 쿨다운은 Update에서 별도로 처리
                if (canBasicAttack)
                {
                    SetAttackState(AttackState.None);
                }
                break;
        }
    }
    
    // 공격 상태 설정
    private void SetAttackState(AttackState newState)
    {
        Debug.Log($"[ManagerRobotBoss] Attack state changed: {currentAttackState} -> {newState}");
        
        currentAttackState = newState;
        attackStateTimer = 0f;
        blackboard.SetValue("CurrentAttackState", (int)newState);
        
        // 상태에 따른 애니메이션 설정
        if (animator != null)
        {
            switch (newState)
            {
                case AttackState.Preparation:
                    break;
                case AttackState.Execute:
                    break;
            }
        }
    }
    
    // 보스 행동 트리 설정 메서드 오버라이드
    protected override void SetupBossBehaviorTree()
    {
        // 루트 노드
        Selector rootSelector = new Selector();
        
        // 사망 시퀀스
        Sequence deathSequence = new Sequence();
        ConditionNode isDeadCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsDead"));
        ActionNode deadAction = new ActionNode(HandleDeath);
        deathSequence.AddChild(isDeadCondition);
        deathSequence.AddChild(deadAction);
        
        // 기절 시퀀스
        Sequence stunSequence = new Sequence();
        ConditionNode isStunnedCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsStunned"));
        ActionNode stunAction = new ActionNode(HandleStunState);
        stunSequence.AddChild(isStunnedCondition);
        stunSequence.AddChild(stunAction);
        
        // 페이즈 전환 시퀀스
        Sequence phaseChangeSequence = new Sequence();
        ConditionNode isChangingPhaseCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsChangingPhase"));
        ActionNode phaseChangeAction = new ActionNode(HandlePhaseChange);
        phaseChangeSequence.AddChild(isChangingPhaseCondition);
        phaseChangeSequence.AddChild(phaseChangeAction);
        
        // 공격 상태 시퀀스
        Sequence attackStateSequence = new Sequence();
        ConditionNode isInAttackStateCondition = new ConditionNode(() => currentAttackState != AttackState.None);
        ActionNode maintainAttackState = new ActionNode(MaintainAttackState);
        attackStateSequence.AddChild(isInAttackStateCondition);
        attackStateSequence.AddChild(maintainAttackState);

        // 패턴 사용 시퀀스
        Sequence patternSequence = new Sequence();
        ConditionNode isUsingPatternCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsUsingPattern"));
        ActionNode patternAction = new ActionNode(ExecuteCurrentPattern);
        patternSequence.AddChild(isUsingPatternCondition);
        patternSequence.AddChild(patternAction);
        
        // 기본 공격 시퀀스
        Node attackSequence = CreateAttackSequence();
        
        // 추적 시퀀스
        Sequence chaseSequence = new Sequence();
        ConditionNode isPlayerDetected = new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected"));
        ConditionNode isNotUsingPattern = new ConditionNode(() => !blackboard.GetValue<bool>("IsUsingPattern"));
        ConditionNode isOutOfAttackRange = new ConditionNode(() => !IsTargetInBasicAttackRange());
        ActionNode chaseAction = new ActionNode(ChaseTarget);
        chaseSequence.AddChild(isPlayerDetected);
        chaseSequence.AddChild(isNotUsingPattern);
        chaseSequence.AddChild(isOutOfAttackRange);
        chaseSequence.AddChild(chaseAction);
        
        // 대기 시퀸스
        Sequence idleSequence = new Sequence();
        ActionNode idleAction = new ActionNode(IdleState);
        idleSequence.AddChild(idleAction);
        
        // 트리구성
        rootSelector.AddChild(deathSequence);       // 사망 상태 (최우선)
        rootSelector.AddChild(stunSequence);        // 기절 상태
        rootSelector.AddChild(phaseChangeSequence); // 페이즈 전환
        rootSelector.AddChild(attackStateSequence); // 공격 진행 중
        rootSelector.AddChild(patternSequence);     // 패턴 실행 중
        rootSelector.AddChild(attackSequence);      // 기본 공격
        rootSelector.AddChild(chaseSequence);       // 추적
        rootSelector.AddChild(idleSequence);        // 대기
        
        // Behavior Tree 생성
        behaviorTree = new BehaviorTree(rootSelector)
        {
            Blackboard = blackboard
        };
    }
    
    // 패턴 초기화
    protected override void InitializePhasePatterns()
    {
        // 기본 공격 패턴 노드 생성
        basicAttackNode = CreatePatternNode(BasicAttackPattern);
        
        // 페이즈 0 패턴: 기본 공격
        AddPatternToPhase(0, basicAttackNode, false);
        
        // 고급 패턴은 추후 구현
    }
    
    // 기본 공격 패턴 실행
    private NodeState BasicAttackPattern()
    {
        // 이미 공격 중이면 진행 상태 반환
        if (currentAttackState != AttackState.None)
        {
            return NodeState.Running;
        }
        
        // 플레이어가 공격 범위 내에 있는지 확인
        if (!IsTargetInBasicAttackRange())
        {
            ChaseTarget();
            return NodeState.Failure;
        }
        
        // 공격 준비 상태로 전환
        SetAttackState(AttackState.Preparation);
        
        // 플레이어 방향으로 보스 회전
        if (target != null)
        {
            float direction = Mathf.Sign(target.position.x - transform.position.x);
            SetDirection(direction);
        }
        
        // 움직임 정지
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        
        // 패턴이 실행 중임을 반환
        return NodeState.Running;
    }
    
    // 공격 실행
    private void ExecuteAttack()
    {
        if (attackPoint != null)
        {
            Debug.Log($"[ManagerRobotBoss] Executing attack at position: {attackPoint.position}");
        }
        // 시각 효과 및 소리 재생
        
        // 공격 방향에 따른 히트박스 위치 조정
        float direction = GetDirection();
        Vector2 attackPos = attackPoint.position + new Vector3(attackHitBoxOffset.x * direction,
            attackHitBoxOffset.y);
        
        // OverlapBox로 히트박스 내의 플레이어 감지
        Collider2D hitPlayer = Physics2D.OverlapBox(attackPos, attackHitBoxSize, 0, playerLayer);
        
        // 플레이어에게 데미지 적용
        if (hitPlayer != null)
        {
            PlayerHp playerHp = hitPlayer.GetComponent<PlayerHp>();
            if (playerHp != null)
            {
                playerHp.DecreaseHp(basicAttackDamage, true);
                
                // 카메라 효과
                if (CameraController.Instance != null)
                {
                    CameraController.Instance.ShakeScreen(0.3f, 0.2f, 0.1f);
                }
            }
        }
    }
    
    // 플레이어가 기본 공격 범위 내에 있는지 확인
    private bool IsTargetInBasicAttackRange()
    {
        if (target == null)
            return false;

        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= basicAttackRange;
    }
    
    // 추적 메서드 오버라이드 
    protected override NodeState ChaseTarget()
    {
        if (isStunned || isDead || isChangingPhase || currentAttackState != AttackState.None)
            return NodeState.Failure;

        Transform currentTarget = blackboard.GetValue<Transform>("Target");

        if (currentTarget == null)
        {
            if (animator != null)
            {
                animator.SetChasingState(false);
            }
            return NodeState.Failure;
        }
        
        // 추적 애니메이션 활성화
        if (animator != null)
        {
            animator.SetChasingState(true);
        }
        
        // 공격 범위 내인지 다시 한번 확인
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance <= basicAttackRange)
        {
            // 공격 범위 내면 이동 중지하고 실패
            if (movement != null)
            {
                movement.MoveTo(0);
            }
            return NodeState.Failure;
        }
        
        // 공격 범위 밖에 있으면 플레이어를 향해 이동
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        float dirToTarget = Mathf.Sign(direction.x);
        
        // 방향 설정
        if (dirToTarget != 0)
        {
            SetDirection(dirToTarget);
        }
        
        // 이동
        if (movement != null)
        {
            movement.MoveToFast(dirToTarget);
        }

        return NodeState.Running;
    }
    
    // 기본 공격 시퀀스 오버라이드
    protected override Node CreateAttackSequence()
    {
        // 공격 시퀀스
        Sequence attackSequence = new Sequence();
        
        // 조건 노드들
        ConditionNode isNotHit = new ConditionNode(() => !blackboard.GetValue<bool>("IsHit"));
        ConditionNode isPlayerDetected = new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected"));
        ConditionNode isInAttackRange = new ConditionNode(IsTargetInBasicAttackRange);
        ConditionNode canAttackNow = new ConditionNode(() => blackboard.GetValue<bool>("CanBasicAttack"));
        ConditionNode isNotUsingPattern = new ConditionNode(() => !blackboard.GetValue<bool>("IsUsingPattern"));
        ConditionNode isNotInAttackState = new ConditionNode(() => currentAttackState == AttackState.None);
        
        // 공격 액션 노드
        ActionNode performAttack = new ActionNode(PerformBasicAttack);
        
        // 공격 시퀀스 구성
        attackSequence.AddChild(isNotHit); // 피격 상태가 아닌지
        attackSequence.AddChild(isPlayerDetected); // 플레이어가 감지되었는지
        attackSequence.AddChild(isInAttackRange); // 공격 범위 내에 있는지
        attackSequence.AddChild(canAttackNow); // 공격 쿨다운이 끝났는지
        attackSequence.AddChild(isNotUsingPattern); // 패턴 사용 중이 아닌지
        attackSequence.AddChild(isNotInAttackState); // 공격 상태가 아닌지
        attackSequence.AddChild(performAttack); // 공격 수행

        return attackSequence;
    }
    
    // 기본 공격 실행
    private NodeState PerformBasicAttack()
    {
        // 기본 공격 취소
        if (isStunned || isDead) return NodeState.Failure;
        
        // 공격 준비 상태로 전환
        SetAttackState(AttackState.Preparation);
        
        // 플레이어 방향으로 보스 회전
        if (target != null)
        {
            float direction = Mathf.Sign(target.position.x - transform.position.x);
            SetDirection(direction);
        }

        if (movement != null)
        {
            movement.MoveTo(0);
        }

        return NodeState.Success;
    }
    
    // 애니메이션 이벤트 (애니메이션에서 호출 가능)
    public void OnAttackHitFrame()
    {
        if (currentAttackState == AttackState.Execute)
        {
            ExecuteAttack();
        }
    }
    
    // 디버그용 시각화
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // 공격 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, basicAttackRange);
        
        // 공격 히트박스 표시
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            float direction = Application.isPlaying ? GetDirection() : Mathf.Sign(transform.localScale.x);
            Vector2 attackPos = (Vector2)attackPoint.position +
                                new Vector2(attackHitBoxOffset.x * direction, attackHitBoxOffset.y);
            Gizmos.DrawWireCube(attackPos, attackHitBoxSize);
        }
    }
}
