using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ManagerRobotBoss : BossBT
{
    [Header("기본 공격 설정")] 
    [SerializeField] private float basicAttackRange = 3f;           // 기본 공격 범위
    [SerializeField] private float basicAttackCooldown = 5f;        // 기본 공격 쿨다운
    [SerializeField] private float attackPrepTime = 1f;             // 공격 준비 시간
    [SerializeField] private float attackExecuteTime = 0.5f;        // 공격 실행 시간
    [SerializeField] private int basicAttackDamage = 2;             // 기본 공격 데미지

    [Header("공격 히트박스 설정")] 
    [SerializeField] private Transform attackPoint;                 // 공격 중심점
    [SerializeField] private Vector2 attackHitBoxSize = new Vector2(5f, 2f); // 공격 히트박스 크기
    [SerializeField] private Vector2 attackHitBoxOffset;            // 공격 히트박스 오프셋

    [Header("박치기 패턴 설정")] 
    [SerializeField] private float headbuttPrepTime = 1f;       // 박치기 준비 시간
    [SerializeField] private float headbuttSpeed = 15f;         // 박치기 속도
    [SerializeField] private float headbuttCooldown = 8f;       // 박치기 쿨다운
    [SerializeField] private int headbuttDamage = 3;            // 박치기 데미지
    [SerializeField] private float headbuttKnockBackForce = 7f; // 플레이어 넉백
    [SerializeField] private LayerMask headbuttLayer;           // 박치기 벽 레이어
    [SerializeField] private BoxCollider2D headbuttCollider;    // 박치기 충돌 박스

    [Header("로봇 소환 패턴 설정")] 
    [SerializeField] private float summonCooldown = 20f;        // 로봇 소환 패턴 쿨다운
    [SerializeField] private float moveToCenterDuration = 1f;   // 중앙으로 이동하는 시간
    [SerializeField] private float ascentDuration = 2f;         // 천장에 올라가는 시간
    [SerializeField] private float descentDuration = 2f;        // 내려오는 시간
    [SerializeField] private float ceilingHeight = 10f;         // 천장 높이
    [SerializeField] private GameObject[] robotPrefabs;         // 소환할 로봇 프리팹
    [SerializeField] private int initialRobotCount = 5;         // 초기 소환 로봇 수
    [SerializeField] private int maxRobotCount = 7;             // 최대 소환 로봇 수
    [SerializeField] private float minSpawnTime = 5f;           // 소환 최소 간격
    [SerializeField] private float maxSpawnTime = 10f;          // 소환 최대 간격
    [SerializeField] private bool useDirectPositioning = true;  // 물리 무시하고 직접 위치 설정
    [SerializeField] private Transform ceilingPoint;            // 천장 위치
    [SerializeField] private Transform centerPoint;             // 보스방 중앙 위치 포인트

    [Header("마나 드레인 설정")] 
    [SerializeField] private GameObject manaColliderPrefab;     // 마나 콜라이더 프리팹
    [SerializeField] private Transform manaColliderSpawnPoint;  // 마나 콜라이더 생성 위치
    [SerializeField] private bool enableManaDrain = true;

    [Header("사망 잔해 설정")] 
    [SerializeField] private BoxCollider2D remainsCollider;     // 잔해용 콜라이더
    [SerializeField] private Transform skillTokenSpawnPoint;    // 스킬 토큰 생성 위치
    [SerializeField] private GameObject skillTokenPrefab;       // 스킬 토큰 프리팹
    [SerializeField] private float remainsColliderWidth = 6.4f;   // 잔해 콜라이더 너비
    [SerializeField] private float remainsColliderHeight = 2.5f;  // 잔해 콜라이더 높이
    [SerializeField] private Vector2 remainsColliderOffset = new Vector2(0.5f, -1.75f);
    
    // 사망 잔해 관련 변수
    private bool hasTransformedToRemains = false;
    
    // 마나 드레인 관련 변수
    private GameObject activeManaCollider;
    private bool canManaDrain = false;
    private bool isManaDraining = false;
    
    // 공격 상태 관리
    private enum AttackState { None, Preparation, Execute, Cooldown }
    private AttackState currentAttackState = AttackState.None;
    private float attackStateTimer;
    private bool canBasicAttack = true;
    
    // 박치기 상태 관리
    private enum HeadbuttState {None, Preparation, Charging, Cooldown }
    private HeadbuttState currentHeadbuttState = HeadbuttState.None;
    private Vector2 headbuttDirection = Vector2.zero;
    private bool canHeadbutt = true;
    private bool hasHitPlayer;          // 플레이어 충돌 추적
    private bool hasHitWall;            // 벽 충돌 추적
    
    // 로봇 소환 패턴 상태 관리
    private enum SummonState { None, MovingToCenter, Ascending, Summoning, Descending, Ending }
    private SummonState currentSummonState = SummonState.None;
    private float nextRobotSpawnTime;
    private bool canSummon = true;
    private List<GameObject> summonedRobots = new List<GameObject>();
    private bool isInvulnerable = false;
    private Vector3 originalPosition;       // 원래 위치 저장
    private Vector3 centerPosition;
    private Quaternion originalRotation;    // 원래 회전 저장
    private bool wasGravityEnabled = true;  // 중력 활성화 상태 저장
    private bool initialSpawnComplete = false; // 초기 소환 완료 여부
    
    // 타이머 및 상태 관리 변수
    private float basicAttackTimer;
    private float headbuttStateTimer;
    private float summonStateTimer;
    
    // 패턴 액션 노드
    private ActionNode headbuttPatternNode;
    private ActionNode summonPatternNode;

    protected override void Awake()
    {
        base.Awake();
        
        // 블랙보드에 공격 관련 데이터 추가
        blackboard.SetValue("BasicAttackRange", basicAttackRange);
        blackboard.SetValue("CanBasicAttack", true);
        blackboard.SetValue("CurrentAttackState", (int)AttackState.None);
        
        // 박치기 패턴 데이터 추가
        blackboard.SetValue("HeadbuttState", (int)HeadbuttState.None);
        blackboard.SetValue("CanHeadbutt", true);
        
        // 소환 관련 블랙보드 설정
        blackboard.SetValue("SummonState", (int)SummonState.None);
        blackboard.SetValue("CanSummon", true);
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
            attackPointObj.transform.localPosition = new Vector2(0, -2f);
            attackPoint = attackPointObj.transform;
        }

        if (headbuttCollider == null)
        {
            GameObject headbuttColliderObj = new GameObject("HeadbuttCollider");
            headbuttColliderObj.transform.parent = transform;
            headbuttColliderObj.transform.localPosition = new Vector2(2f, 0.5f);
            
            headbuttCollider = headbuttColliderObj.AddComponent<BoxCollider2D>();
            headbuttCollider.size = new Vector2(0.5f, 7);
            headbuttCollider.offset = Vector2.zero;
            headbuttCollider.isTrigger = true;
        }

        if (headbuttCollider != null)
        {
            headbuttCollider.enabled = false;
        }

        if (centerPoint == null)
        {
            Debug.LogWarning("보스의 CenterPoint가 설정되지 않았습니다. 보스방 중앙 위치를 설정해주세요.");
        }

        if (manaColliderSpawnPoint == null)
        {
            GameObject spawnPointObj = new GameObject("ManaColliderSpawnPoint");
            spawnPointObj.transform.parent = transform;
            spawnPointObj.transform.localPosition = new Vector3(0, 0, 0);
            manaColliderSpawnPoint = spawnPointObj.transform;
        }
        
        SetupRemainsCollider();
        
        // 페이즈 체력 임계값 설정 (50% 체력)
        if (phaseHpThreshold.Length > 1)
        {
            phaseHpThreshold[1] = 0.5f;
        }
    }

    protected override void Update()
    {
        if (hasTransformedToRemains) return;
        
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
        UpdateHeadbuttState();
        UpdateSummonState();
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
                if (attackStateTimer >= attackPrepTime)
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
    
    // 박치기 상태 업데이트
    private void UpdateHeadbuttState()
    {
        if (isStunned || isDead || isChangingPhase)
            return;

        switch (currentHeadbuttState)
        {
            case HeadbuttState.Preparation:
                headbuttStateTimer += Time.deltaTime;
                if (headbuttStateTimer >= headbuttPrepTime)
                {
                    // 준비 완료, 돌진 상태로 전환
                    SetHeadbuttState(HeadbuttState.Charging);
                    
                    // 박치기 방향 설정
                    SetHeadbuttDirection();
                    
                    // 초기화
                    hasHitPlayer = false;
                    hasHitWall = false;
                    
                    // 박치기 콜라이더 활성화
                    if (headbuttCollider != null)
                    {
                        headbuttCollider.enabled = true;
                    }
                }
                break;
            
            case HeadbuttState.Charging:
                // 벽 충돌 감지
                CheckWallCollision();
                
                // 벽에 부딪히면 쿨다운 상태로 전환
                if (hasHitWall)
                {
                    // 쿨다운 상태로 전환
                    SetHeadbuttState(HeadbuttState.Cooldown);
                    canHeadbutt = false;
                    blackboard.SetValue("CanHeadbutt", false);
                    
                    movement.MoveTo(0);
                    return;
                }

                if (!hasHitPlayer)
                {
                    CheckHeadbuttPlayerCollision();
                }
                
                // 돌진 이동 처리
                PerformHeadbuttMovement();
                break;
            
            case HeadbuttState.Cooldown:
                headbuttStateTimer += Time.deltaTime;
                if (headbuttStateTimer >= headbuttCooldown)
                {
                    SetHeadbuttState(HeadbuttState.None);
                    canHeadbutt = true;
                    blackboard.SetValue("CanHeadbutt", true);
                }
                break;
        }
    }
    
    // 소환 패턴 상태 업데이트
    private void UpdateSummonState()
    {
        if (isStunned || isDead || isChangingPhase)
            return;

        switch (currentSummonState)
        {
            case SummonState.MovingToCenter:
                // 보스가 중앙으로 이동하는 상태
                summonStateTimer += Time.deltaTime;
                
                // 중앙으로 이동
                float moveCenterProgress = Mathf.Clamp01(summonStateTimer / moveToCenterDuration);
                MoveTowardsCenter(moveCenterProgress);
                
                // 중앙 이동 완료되면 상승 단계로 전환
                if (summonStateTimer >= moveToCenterDuration)
                {
                    SetSummonState(SummonState.Ascending);
                    summonStateTimer = 0f;
                }
                break;
            
            case SummonState.Ascending:
                // 보스가 천장으로 올라가는 상태
                summonStateTimer += Time.deltaTime;
                
                // 천장으로 이동
                float ascendProgress = Mathf.Clamp01(summonStateTimer / ascentDuration);
                MoveTowardsCeiling(ascendProgress);
                
                // 올라가기 완료되면 소환 단계로 전환
                if (summonStateTimer >= ascentDuration)
                {
                    SetSummonState(SummonState.Summoning);
                    nextRobotSpawnTime = Time.time; // 바로 첫 로봇 소환
                    initialSpawnComplete = false;
                }
                break;
            
            case SummonState.Summoning:
                summonStateTimer += Time.deltaTime;
                
                // 초기 일괄 소환
                if (!initialSpawnComplete)
                {
                    SpawnInitialRobotBatch();
                    initialSpawnComplete = true;
                    
                    // 다음 추가 소환 시간 설정
                    nextRobotSpawnTime = Time.time + Random.Range(minSpawnTime, maxSpawnTime);
                }
                // 추가 소환
                else if (Time.time >= nextRobotSpawnTime && summonedRobots.Count < maxRobotCount)
                {
                    SpawnRobot();
                    nextRobotSpawnTime = Time.time + Random.Range(minSpawnTime, maxSpawnTime);
                }
                
                // 패턴 종료 조건 체크
                if (summonedRobots.Count == 0 && initialSpawnComplete)
                {
                    SetSummonState(SummonState.Descending);
                    summonStateTimer = 0f;
                }
                break;
            
            case SummonState.Descending:
                // 보스가 천장에서 내려오는 중
                summonStateTimer += Time.deltaTime;
                
                // 원래 위치로 내려오기
                float descendProgress = Mathf.Clamp01(summonStateTimer / descentDuration);
                MoveFromCeiling(descendProgress);
                
                // 내려오기 완료되면 패턴 종료
                if (summonStateTimer >= descentDuration)
                {
                    SetSummonState(SummonState.Ending);
                }
                break;
            
            case SummonState.Ending:
                break;
        }
        
        // 소환된 로봇 리스트 정리
        CleanupRobotList();
    }
    
    // 패턴 초기화
    protected override void InitializePhasePatterns()
    {
        // 박치기 패턴 노드 생성
        headbuttPatternNode = CreatePatternNode(HeadbuttPattern);
        
        // 소환 패턴 노드 생성
        summonPatternNode = CreatePatternNode(SummonPattern);
        
        // 페이즈 1 패턴: 박치기
        AddPatternToPhase(1, headbuttPatternNode, true);
        AddPatternToPhase(1, summonPatternNode, false);
    }

    // 공격 상태 설정
    private void SetAttackState(AttackState newState)
    {
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
                    animator.TriggerAttackAnim();
                    break;
            }
        }
    }
    
    // 박치기 상태 설정
    private void SetHeadbuttState(HeadbuttState newState)
    {
        currentHeadbuttState = newState;
        headbuttStateTimer = 0f;
        blackboard.SetValue("HeadbuttState", (int)newState);
        
        // 상태에 따른 애니메이션 설정
        if (animator != null)
        {
            switch (newState)
            {
                case HeadbuttState.Preparation:
                    animator.TriggerHeadbuttPrep();
                    break;
                case HeadbuttState.Charging:
                    animator.SetHeadbuttCharge(true);
                    break;
                case HeadbuttState.Cooldown:
                    animator.SetHeadbuttCharge(false);
                    break;
            }
        }
    }

    // 소환 상태 설정
    private void SetSummonState(SummonState newState)
    {
        currentSummonState = newState;
        blackboard.SetValue("SummonState", (int)newState);
        
        // 상태별 추가 처리 (애니메이션)
        switch (newState)
        {
            case SummonState.Ascending:
                animator.TriggerBossUp();
                break;
            case SummonState.Summoning:
                animator.SetBossUpIdle(true);
                break;
            case SummonState.Descending:
                animator.SetBossUpIdle(false);
                animator.SetBossDown(true);
                break;
            case SummonState.Ending:
                animator.SetBossDown(false);
                break;
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
        ConditionNode isOutOfAttackRange = new ConditionNode(() => !IsTargetInAttackRange());
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
    
    // 기본 공격 시퀀스 오버라이드
    protected override Node CreateAttackSequence()
    {
        // 공격 시퀀스
        Sequence attackSequence = new Sequence();
        
        // 조건 노드들
        ConditionNode isNotHit = new ConditionNode(() => !blackboard.GetValue<bool>("IsHit"));
        ConditionNode isPlayerDetected = new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected"));
        ConditionNode isInAttackRange = new ConditionNode(IsTargetInAttackRange);
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
    
    // 박치기 패턴 실행
    private NodeState HeadbuttPattern()
    {
        // 이미 박치기 중이면 진행 중 상태 반환
        if (currentHeadbuttState != HeadbuttState.None)
        {
            if (currentHeadbuttState == HeadbuttState.Cooldown && hasHitWall)
            {
                hasHitWall = false;
                return NodeState.Success;
            }
            return NodeState.Running;
        }
        
        // 박치기 쿨다운 중이면 실패
        if (!canHeadbutt)
        {
            return NodeState.Failure;
        }
        
        // 움직임 정지
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        
        // 플레이어 방향으로 보스 회전
        if (target != null)
        {
            float direction = Mathf.Sign(target.position.x - transform.position.x);
            SetDirection(direction);
        }
        
        // 박치기 준비 상태로 전환
        SetHeadbuttState(HeadbuttState.Preparation);

        return NodeState.Running;
    }
    
    // 소환 패턴 메서드
    private NodeState SummonPattern()
    {
        // 이미 소환 중이면 진행 중 상태 반환
        if (currentSummonState != SummonState.None)
        {
            // 패턴이 종료되었으면 성공 상태 반환
            if (currentSummonState == SummonState.Ending)
            {
                // 패턴 종료 처리
                Debug.Log("Ending State Start");
                EndSummonPattern();
                
                SetSummonState(SummonState.None);
                return NodeState.Success;
            }
            return NodeState.Running;
        }
        
        // 소환 쿨다운 중이면 실패
        if (!canSummon)
        {
            return NodeState.Failure;
        }
        
        // 움직임 정지
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        
        // 플레이어 방향 확인
        if (target != null)
        {
            float direction = Mathf.Sign(target.position.x - transform.position.x);
            SetDirection(direction);
        }
        
        // 패턴 시작
        StartSummonPattern();

        return NodeState.Running;
    }
    
    // 공격 실행
    private void ExecuteAttack()
    {
        // 시각 효과 및 소리 재생
        
        // 공격 방향에 따른 히트박스 위치 조정
        float direction = GetDirection();
        Vector2 attackPos = attackPoint.position + new Vector3(attackHitBoxOffset.x * direction,
            attackHitBoxOffset.y);
        
        // OverlapBox로 히트박스 내의 플레이어 감지
        Collider2D hitPlayer = Physics2D.OverlapBox(attackPos, attackHitBoxSize, 0, targetLayer);
        
        // 플레이어에게 데미지 적용
        if (hitPlayer != null)
        {
            PlayerHp playerHp = hitPlayer.GetComponent<PlayerHp>();
            if (playerHp != null)
            {
                Vector2 knockBackDirection = new Vector2(direction * knockBackForce, 4 * knockBackForce);
                DeathData deathData = new DeathData(DeathCause.MeleeAttack, GetDirection());
                playerHp.DecreaseHp(basicAttackDamage, knockBackDirection, deathData,true);
                
                // 카메라 효과
                if (CameraController.Instance != null)
                {
                    CameraController.Instance.ShakeScreen(0.3f, 0.2f, 0.1f);
                }
            }
        }
    }
    
    // 플레이어가 기본 공격 범위 내에 있는지 확인
    protected override bool IsTargetInAttackRange()
    {
        if (target == null)
            return false;

        Transform currentTarget = blackboard.GetValue<Transform>("Target");
        
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        return distance <= basicAttackRange;
    }
    
    // 페이즈 활성화 시 호출 오버라이드
    protected override void OnPhaseActivated(int phase)
    {
        if (phase == 1)
        {
            StartCoroutine(StartHeadbuttAfterDelay(0.5f));
        }
    }
    
    // 지연 후 박치기 시작 코루틴
    private IEnumerator StartHeadbuttAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 패턴 시작 플래그 설정
        isUsingPattern = true;
        blackboard.SetValue("IsUsingPattern", true);
        
        // 현재 패턴을 박차기로 지정
        lastPatternIndex = phasePatterns[currentPhase].IndexOf(headbuttPatternNode);
    }
    
    // 박치기 방향 설정
    private void SetHeadbuttDirection()
    {
        headbuttDirection = new Vector2(GetDirection(), 0).normalized;
    }
    
    // 박치기 이동 처리
    private void PerformHeadbuttMovement()
    {
        // 현재 방향으로 일덩 속도 이동
        transform.position += (Vector3)(headbuttDirection * (headbuttSpeed * Time.deltaTime));
    }
    
    // 벽 충돌 감지
    private void CheckWallCollision()
    {
        // 짧은 레이캐스트로 전방 벽 충돌 확인
        Vector2 rayOrigin = (Vector2)transform.position + new Vector2(Mathf.Sign(headbuttDirection.x) * 3f, -2.5f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, headbuttDirection, wallCheckDistance, headbuttLayer);
        
        // 디버그 레이 표시
        Debug.DrawRay(rayOrigin, headbuttDirection * wallCheckDistance, hit ? Color.red : Color.yellow);
        
        // 벽 충돌 감지
        if (hit.collider != null)
        {
            hasHitWall = true;
            
            // 벽에 부딪힐 때 카메라 효과
            if (CameraController.Instance != null)
            {
                CameraController.Instance.ShakeScreen(0.2f, 0.15f, 0.1f);
            }
        }
    }
    
    // 플레이어 충돌 감지
    private void CheckHeadbuttPlayerCollision()
    {
        // 이미 충돌했으면 다시 체크하지 않음
        if (hasHitPlayer) return;

        if (headbuttCollider != null && headbuttCollider.enabled)
        {
            // 콜라이더와 플레이어 간의 충돌 확인
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(targetLayer);
            filter.useTriggers = true;

            Collider2D[] results = new Collider2D[1];
            if (headbuttCollider.OverlapCollider(filter, results) > 0 && results[0] != null)
            {
                // 플레이어 발견
                PlayerHeadbuttCollision(results[0].gameObject);
            }
        }
    }
    
    // 플레이어 충돌 처리
    private void PlayerHeadbuttCollision(GameObject playerObj)
    {
        if (hasHitPlayer) return;
        hasHitPlayer = true;
        
        PlayerHp playerHp = playerObj.GetComponent<PlayerHp>();
        if (playerHp != null)
        {
            // 넉백
            Vector2 knockBackDirection = new Vector2(headbuttDirection.x * knockBackForce, 4 * knockBackForce);

            DeathData deathData = new DeathData(DeathCause.MeleeAttack, GetDirection());
            // 데미지 적용
            playerHp.DecreaseHp(headbuttDamage, knockBackDirection, deathData,true);
        }
        
        // 카메라 효과
        if (CameraController.Instance != null)
        {
            CameraController.Instance.ShakeScreen(0.3f, 0.2f, 0.1f);
        }
    }
    
    // 소환 패턴 시작
    private void StartSummonPattern()
    {
        // 시작 시 상태 저장
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // 중앙 위치 설정
        if (centerPoint != null)
        {
            centerPosition = centerPoint.position;
        }
        else
        {
            // centerPoint가 없으면 현재 위치를 중앙으로 사용
            centerPosition = transform.position;
        }
        
        // 리지드바디 상태 저장
        if (rb != null)
        {
            wasGravityEnabled = rb.gravityScale > 0;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            
            movement.DisableGravity();

            if (useDirectPositioning)
            {
                rb.isKinematic = true;
            }
        }
        
        // 무적 설정
        SetInvulnerable(true);
        
        // 소환된 로봇 리스트 초기화
        summonedRobots.Clear();
        
        // 타이머 초기화
        summonStateTimer = 0f;
        
        // 소환 상태로 전환
        SetSummonState(SummonState.MovingToCenter);
    }
    
    // 소환 패턴 종료
    private void EndSummonPattern()
    {
        // 남은 로봇 모두 제거 
        foreach (GameObject robot in summonedRobots)
        {
            if (robot != null)
            {
                Destroy(robot);
            }
        }
        summonedRobots.Clear();
        
        // 원래 상태로 복구
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        // 리지드바디 상태 복구
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.gravityScale = wasGravityEnabled ? 1f : 0f;
        }
        
        // 무적 상태 원래대로 복구
        SetInvulnerable(false);
        
        // 쿨다운 설정
        canSummon = false;
        blackboard.SetValue("CanSummon", false);
        StartCoroutine(SummonCooldownRoutine());
    }
    
    // 중앙으로 이동하는 메서드
    private void MoveTowardsCenter(float progress)
    {
        if (useDirectPositioning)
        {
            // 직접 위치 설정으로 중앙까지 이동
            transform.position = Vector3.Lerp(originalPosition, centerPosition, progress);
        }
    }
    
    // 천장으로 이동
    private void MoveTowardsCeiling(float progress)
    {
        // 천장 위치 계산
        Vector3 ceilingPosition;

        if (ceilingPoint != null)
        {
            // 지정된 천장 지점 사용
            ceilingPosition = ceilingPoint.position;
        }
        else
        {
            ceilingPosition = centerPosition + new Vector3(0, ceilingHeight, 0);
        }
        
        if (useDirectPositioning)
        {
            // 직접 위치 설정
            transform.position = Vector3.Lerp(centerPosition, ceilingPosition, progress);
        }
        else
        {
            // 리지드바디로 이동
            if (rb != null)
            {
                Vector3 targetPosition = Vector3.Lerp(centerPosition, ceilingPosition, progress);
                rb.MovePosition(targetPosition);
            }
        }
    }
    
    // 천장에서 내려오기
    private void MoveFromCeiling(float progress)
    {
        // 천장 위치 계산
        Vector3 ceilingPosition;
        if (ceilingPoint != null)
        {
            ceilingPosition = ceilingPoint.position;
        }
        else
        {
            ceilingPosition = centerPosition + new Vector3(0, ceilingHeight, 0);
        }
        
        if (useDirectPositioning)
        {
            // 반대 방향으로 보간
            transform.position = Vector3.Lerp(ceilingPosition, centerPosition, progress);
        }
        else
        {
            if (rb != null)
            {
                Vector3 targetPosition = Vector3.Lerp(ceilingPosition, centerPosition, progress);
                rb.MovePosition(targetPosition);
            }
        }
    }
    
    // 초기 로봇 일괄 소환 메서드
    private void SpawnInitialRobotBatch()
    {
        // 로봇 프리팹 체크
        if (robotPrefabs == null || robotPrefabs.Length == 0)
        {
            return;
        }
        
        // 초기에 5개 로봇 소환
        for (int i = 0; i < initialRobotCount; i++)
        {
            // 소환 위치 계산 
            Vector2 spawnPos = CalculateSpawnPosition(i, initialRobotCount);
            
            // 랜덤 로봇 선택
            int randomIndex = Random.Range(0, robotPrefabs.Length);
            GameObject robotPrefab = robotPrefabs[randomIndex];

            if (robotPrefab == null) continue;
            
            // 로봇 생성
            GameObject robot = Instantiate(robotPrefab, spawnPos, Quaternion.identity);
            
            // 리스트에 추가
            summonedRobots.Add(robot);
        }
    }
    
    // 로봇 소환
    private void SpawnRobot()
    {
        if (robotPrefabs == null || robotPrefabs.Length == 0)
        {
            return;
        }
        
        // 랜덤 로봇 선택
        int randomIndex = Random.Range(0, robotPrefabs.Length);
        GameObject robotPrefab = robotPrefabs[randomIndex];

        if (robotPrefab == null) return;
        
        // 소환 위치 계산 (보스 주변)
        Vector2 spawnPos = CalculateSpawnPosition();
        
        // 로봇 생성
        GameObject robot = Instantiate(robotPrefab, spawnPos, Quaternion.identity);
        
        // 리스트에 추가
        summonedRobots.Add(robot);
        
    }

    private Vector2 CalculateSpawnPosition(int index, int totalCount)
    {
        // 기본 위치
        Vector2 basePosition = new Vector2(centerPosition.x, centerPosition.y);
        
        // 소환 범위
        float spawnWidth = 10f;
        
        // 플레이어 위치 고려
        float playerOffset = 0f;
        if (target != null)
        {
            playerOffset = target.position.x - centerPosition.x;
            // 범위 제한
            playerOffset = Mathf.Clamp(playerOffset, -5f, 5f);
        }
        
        // 분포 계산
        float fraction = (float)index / (totalCount - 1);
        float positionX;

        if (totalCount <= 1)
        {
            positionX = basePosition.x + playerOffset;
        }
        else
        {
            positionX = basePosition.x - spawnWidth / 2 + spawnWidth * fraction + playerOffset * 0.5f;
        }
        
        // 최종 위치 계산
        Vector2 spawnPos = new Vector2(positionX, basePosition.y);
        
        // 바닥 레이캐스트로 확인
        RaycastHit2D floorHit = Physics2D.Raycast(spawnPos + Vector2.up * 5f, Vector2.down, 10f, groundLayer);
        if (floorHit.collider != null)
        {
            spawnPos.y = floorHit.point.y + 1f;
        }
        
        return spawnPos;
    }
    
    // 소환 위치 계산
    private Vector2 CalculateSpawnPosition()
    {
        // 기본 위치
        Vector2 basePosition = new Vector2(transform.position.x, centerPosition.y);
        
        // 소환 방향 랜덤화
        float spawnDir = (target != null && Random.value < 0.7f) 
            ? Mathf.Sign(target.position.x - centerPosition.x)
            : (Random.value < 0.5f ? 1f : -1f);
        
        // 소환 거리 랜덤화
        float spawnDistance = Random.Range(3f, 7f);
        
        // 최종 위치 계산
        Vector2 spawnPos = basePosition + new Vector2(spawnDir * spawnDistance, 0);
        
        // 바닥 레이캐스트 확인
        RaycastHit2D floorHit = Physics2D.Raycast(spawnPos + Vector2.up * 5f, Vector2.down, 10f, groundLayer);
        if (floorHit.collider != null)
        {
            spawnPos.y = floorHit.point.y + 1f;
        }
        
        return spawnPos;
    }
    
    // 소환된 로봇 리스트 정리
    private void CleanupRobotList()
    {
        for (int i = summonedRobots.Count - 1; i >= 0; i--)
        {
            GameObject robot = summonedRobots[i];

            if (robot == null || !robot.activeInHierarchy || robot.Equals(null))
            {
                summonedRobots.RemoveAt(i);
                Debug.Log($"Removed {robot.name}");
            }
        }
    }
    
    // 무적 설정
    private void SetInvulnerable(bool invulnerable)
    {
        // 무적 플래그 설정
        isInvulnerable = invulnerable;
        
        // 무적 상태일 때
    }
    
    // 소환 쿨다운 코루틴
    private IEnumerator SummonCooldownRoutine()
    {
        yield return new WaitForSeconds(summonCooldown);
        canSummon = true;
        blackboard.SetValue("CanSummon", true);
    }
    
    // 데미지 처리 오버라이드
    public override void DecreaseHp(int damage, bool isThrownWeapon = false)
    {
        // 무적 상태면 데미지 무시
        if (isInvulnerable)
        {
            return;

        }
        
        base.DecreaseHp(damage, isThrownWeapon);
    }
    
    protected override void StartStun()
    {
        base.StartStun();
        
        // 마나 드레인 활성화
        if (enableManaDrain)
        {
            ActivateManaDrain();
        }
    }

    protected override void RecoverFromStun()
    {
        base.RecoverFromStun();

        DeactivateManaDrain();
    }

    private void ActivateManaDrain()
    {
        if (activeManaCollider != null || !enableManaDrain) return;

        canManaDrain = true;
        
        // 마나 콜라이더 생성
        if (manaColliderPrefab != null)
        {
            activeManaCollider = Instantiate(manaColliderPrefab, manaColliderSpawnPoint.position, Quaternion.identity);
            activeManaCollider.transform.SetParent(transform);
            
            ManaCollider manaColliderComponent = activeManaCollider.GetComponent<ManaCollider>();
            if (manaColliderComponent != null)
            {
                manaColliderComponent.SetBossReference(this);
            }
            
            Debug.Log("마나 드레인 콜라이더 활성화");
        }
        else
        {
            Debug.LogWarning("마나 콜라이더 프리팹이 설정되지 않았습니다.");
        }
    }

    private void DeactivateManaDrain()
    {
        canManaDrain = false;
        isManaDraining = false;

        if (activeManaCollider != null)
        {
            Destroy(activeManaCollider);
            activeManaCollider = null;
            Debug.Log("마나 드레인 콜라이더 비활성화");
        }
    }

    public bool CanManaDrain()
    {
        return canManaDrain && isStunned && !isDead;
    }

    public void StartManaDraining()
    {
        isManaDraining = true;
    }

    public void StopManaDraining()
    {
        isManaDraining = false;
    }

    public bool IsManaDraining()
    {
        return isManaDraining;
    }
    
    // 애니메이션 이벤트 (애니메이션에서 호출 가능)
    public void OnAttackHitFrame()
    {
        if (currentAttackState == AttackState.Execute)
        {
            ExecuteAttack();
        }
    }

    // 패턴 인덱스 선택 메서드 오버라이드
    protected override int GetNextPatternIndex()
    {
        if (currentPhase >= phasePatterns.Length || phasePatterns[currentPhase].Count == 0)
        {
            return -1;
        }
        
        // 페이즈 1에서는 패턴 선택 로직 적용
        if (currentPhase == 1)
        {
            int headbuttIndex = -1;
            int summonIndex = -1;

            // 패턴 인덱스 찾기
            for (int i = 0; i < phasePatterns[currentPhase].Count; i++)
            {
                if (phasePatterns[currentPhase][i] == headbuttPatternNode)
                {
                    headbuttIndex = i;
                }
                else if (phasePatterns[currentPhase][i] == summonPatternNode)
                {
                    summonIndex = i;
                }
            }
            
            // 패턴 가용성 확인
            bool canUseHeadbuttNow = canHeadbutt && currentHeadbuttState == HeadbuttState.None;
            bool canUseSummonNow = canSummon && currentSummonState == SummonState.None;
            
            // 패턴 선택 로직
            if (canUseHeadbuttNow && canUseSummonNow)
            {
                return headbuttIndex;
            }
            else if (canUseHeadbuttNow)
            {
                return headbuttIndex;
            }
            else if (canUseSummonNow)
            {
                return summonIndex;
            }
        }

        return -1;
    }

    private void SetupRemainsCollider()
    {
        if (remainsCollider == null)
        {
            GameObject remainsColliderObj = new GameObject("RemainsCollider");
            remainsColliderObj.transform.SetParent(transform);
            remainsColliderObj.transform.localPosition = Vector3.zero;
            
            remainsCollider = remainsColliderObj.AddComponent<BoxCollider2D>();
            remainsCollider.size = new Vector2(remainsColliderWidth, remainsColliderHeight);
            remainsCollider.offset = remainsColliderOffset;
            remainsCollider.enabled = false;
        }
        else
        {
            remainsCollider.enabled = false;
        }
        
        // 스킬 토큰 생성 포인트 설정
        if (skillTokenSpawnPoint == null)
        {
            GameObject spawnPointObj = new GameObject("SkillTokenSpawnPoint");
            spawnPointObj.transform.SetParent(transform);
            spawnPointObj.transform.localPosition = new Vector3(0, 2f, 0);
            skillTokenSpawnPoint = spawnPointObj.transform;
        }
    }
    
    protected override NodeState HandleDeath()
    {
        if (isDeathProcessed)
        {
            return NodeState.Success;
        }
        
        isDeathProcessed = true;
        
        // 움직임 멈춤
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        
        // 물리 정지
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
        
        // 기존 적 콜라이더 비활성화
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        // 마나 드레인 비활성화
        DeactivateManaDrain();
        
        // 사망 애니메이션 재생
        if (animator != null)
        {
            animator.SetMovementAnim(0);
            animator.SetChasingState(false);
            animator.TriggerDeathAnim();
        }
        
        // PlaySound(deadClip);

        StartCoroutine(TransformToRemainsAfterAnimation());
        
        return NodeState.Success;
    }

    private IEnumerator TransformToRemainsAfterAnimation()
    {
        yield return new WaitForSeconds(deathDelay);
        
        // 잔해로 변환
        TransformToRemains();
    }
    
    // 잔해 상태로 변환
    private void TransformToRemains()
    {
        if (hasTransformedToRemains) return;

        hasTransformedToRemains = true;

        if (animator != null)
        {
            Animator animatorComponent = animator.GetComponent<Animator>();
            if (animatorComponent != null)
            {
                animatorComponent.enabled = false;
            }
        }

        if (remainsCollider != null)
        {
            remainsCollider.enabled = true;

            remainsCollider.gameObject.layer = LayerMask.NameToLayer("Ground");
        }
        
        // 스킬 토큰 생성
        // SpawnSkillToken();

        if (bossHpBar != null)
        {
            bossHpBar.SetActive(false);
        }
    }

    private void SpawnSkillToken()
    {
        if (skillTokenSpawnPoint == null) return;

        if (skillTokenPrefab != null)
        {
            GameObject skillToken = Instantiate(skillTokenPrefab,
                skillTokenSpawnPoint.position,
                Quaternion.identity);
            
            
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
