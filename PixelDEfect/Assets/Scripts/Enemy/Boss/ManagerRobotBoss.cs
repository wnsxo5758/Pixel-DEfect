using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private int headbuttDamage = 3;         // 박치기 데미지
    [SerializeField] private float headbuttKnockBackForce = 7f; // 플레이어 넉백
    [SerializeField] private LayerMask headbuttLayer;           // 박치기 벽 레이어
    [SerializeField] private BoxCollider2D headbuttCollider;    // 박치기 충돌 박스
    
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
    
    // 타이머 및 상태 관리 변수
    private float basicAttackTimer;
    private float headbuttStateTimer;
    
    // 패턴 액션 노드
    private ActionNode headbuttPatternNode;

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
            headbuttColliderObj.transform.localPosition = new Vector2(1.5f, 0.5f);
            
            headbuttCollider = headbuttColliderObj.AddComponent<BoxCollider2D>();
            headbuttCollider.size = new Vector2(0.5f, 7);
            headbuttCollider.offset = Vector2.zero;
            headbuttCollider.isTrigger = true;
        }

        if (headbuttCollider != null)
        {
            headbuttCollider.enabled = false;
        }
        
        // 페이즈 체력 임계값 설정 (50% 체력)
        if (phaseHpThreshold.Length > 1)
        {
            phaseHpThreshold[1] = 0.5f;
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
        UpdateHeadbuttState();
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
    
    // 패턴 초기화
    protected override void InitializePhasePatterns()
    {
        // 박치기 패턴 노드 생성
        headbuttPatternNode = CreatePatternNode(HeadbuttPattern);
        
        // 페이즈 1 패턴: 박치기
        AddPatternToPhase(1, headbuttPatternNode, true);
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
                    break;
                case HeadbuttState.Charging:
                    break;
                case HeadbuttState.None:
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
        Collider2D hitPlayer = Physics2D.OverlapBox(attackPos, attackHitBoxSize, 0, targetLayer);
        
        // 플레이어에게 데미지 적용
        if (hitPlayer != null)
        {
            PlayerHp playerHp = hitPlayer.GetComponent<PlayerHp>();
            if (playerHp != null)
            {
                Vector2 knockBackDirection = new Vector2(direction * knockBackForce, 4 * knockBackForce);
                playerHp.DecreaseHp(basicAttackDamage, knockBackDirection, true);
                
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
        transform.position += (Vector3)(headbuttDirection * headbuttSpeed * Time.deltaTime);
    }
    
    // 벽 충돌 감지
    private void CheckWallCollision()
    {
        // 짧은 레이캐스트로 전방 벽 충돌 확인
        Vector2 rayOrigin = (Vector2)transform.position + new Vector2(Mathf.Sign(headbuttDirection.x) * 2.5f, -2.5f);
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
            
            // 데미지 적용
            playerHp.DecreaseHp(headbuttDamage, knockBackDirection, true);
        }
        
        // 카메라 효과
        if (CameraController.Instance != null)
        {
            CameraController.Instance.ShakeScreen(0.3f, 0.2f, 0.1f);
        }
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

            for (int i = 0; i < phasePatterns[currentPhase].Count; i++)
            {
                if (phasePatterns[currentPhase][i] == headbuttPatternNode)
                {
                    headbuttIndex = i;
                    break;
                }
            }
            
            // 패턴 가용성 확인
            bool canUseHeadbuttNow = canHeadbutt && currentHeadbuttState == HeadbuttState.None;
            
            // 패턴 2개가 모두 사용 가능하면 박치기 우선
            if (canUseHeadbuttNow && phasePatterns[currentPhase].Count > 1)
            {
                return headbuttIndex;
            }
            // 박치기만 사용 가능하면
            else if (canUseHeadbuttNow)
            {
                return headbuttIndex;
            }
            // 다른 패턴만 사용 가능하면 해당 패턴 사용
            else if (phasePatterns[currentPhase].Count > 1)
            {
                // 박치기가 아닌 첫 번째 패턴 찾기
                for (int i = 0; i < phasePatterns[currentPhase].Count; i++)
                {
                    if (i != headbuttIndex)
                    {
                        return i;
                    }
                }
            }
        }
        // 페이즈가 0이거나 다른 페이즈의 경우 첫 번째 패턴 사용
        else if (phasePatterns[currentPhase].Count > 0)
        {
            return 0;
        }

        return -1;
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
