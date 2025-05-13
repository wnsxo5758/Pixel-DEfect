using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossBT : EnemyBT
{
    [Header("보스 기본 설정")] 
    [SerializeField] protected int currentPhase = 0;
    [SerializeField] protected int maxPhase = 2;
    [SerializeField] protected float[] phaseHpThreshold; // 각 페이즈 전환 체력 비율
    [SerializeField] protected float phaseTransitionTime = 1f; // 페이즈 전환 연출 시간
    [SerializeField] protected bool isInvincibleDuringTransition = true; // 페이즈 전환 중 무적

    [Header("보스 UI 설정")] 
    [SerializeField] protected GameObject bossHpBar;
    [SerializeField] protected string bossName = "Boss";

    [Header("보스 패턴 설정")] 
    [SerializeField] protected float patternCooldown = 5f; // 패턴 간 쿨타임
    [SerializeField] protected float initialDelay = 2f; // 보스 등장 후 첫 행동까지 딜레이

    [Header("보스 기절 상태 설정")] 
    [SerializeField] protected float stunDuration = 4f; // 기절 시간
    [SerializeField] protected int stunThreshold = 3; // 기절 임계값

    // 패턴 관련 변수
    protected List<Node>[] phasePatterns; // 각 페이즈별 패턴 노드 리스트
    protected List<bool>[] patternStunFlags; // 각 패턴이 기절 카운트를 증가시키는지
    protected float lastPatternTime; // 마지막 패턴 실행 시간
    protected bool isChangingPhase = false; // 페이즈 전환중인지
    protected bool isUsingPattern = false; // 패턴을 사용 중인지
    protected bool isBattleStarted = false; // 전투 시작 여부
    
    // 기절 관련 변수
    protected bool isStunned = false; // 기절 상태 여부
    protected float stunTimer = 0f; // 기절 타이머
    protected int stunCount = 0; // 패턴 사용 카운터
    
    // 페이즈 관련 변수
    protected bool[] phaseActivated; // 각 페이즈가 활성화되었는지 여부
    
    // 보스 행동 관련 변수
    protected List<System.Type> immuneStatusList = new List<System.Type>(); // 면역 상태 목록
    protected float patternTimer = 0f; // 패턴 타이머
    protected int lastPatternIndex = -1;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 페이즈 초기화
        phaseActivated = new bool[maxPhase];
        phasePatterns = new List<Node>[maxPhase];
        patternStunFlags = new List<bool>[maxPhase];

        for (int i = 0; i < maxPhase; i++)
        {
            phaseActivated[i] = false;
            phasePatterns[i] = new List<Node>();
            patternStunFlags[i] = new List<bool>();
        }
        
        // 기절 카운트 초기화
        stunCount = 0;
        
        // 블랙보드에 보스 관련 데이터 추가
        blackboard.SetValue("CurrentPhase", currentPhase);
        blackboard.SetValue("IsChangingPhase", isChangingPhase);
        blackboard.SetValue("IsUsingPattern", isUsingPattern);
        blackboard.SetValue("PatternTimer", patternTimer);
        blackboard.SetValue("IsBattleStarted", isBattleStarted);
        blackboard.SetValue("IsStunned", isStunned);
        blackboard.SetValue("StunTimer", stunTimer);
        blackboard.SetValue("StunCount", stunCount);
        blackboard.SetValue("StunThreshold", stunThreshold);
        
        // 초기화
        lastPatternTime = -patternCooldown;
    }

    protected override void Start()
    {
        // 보스별 패턴 초기화
        InitializePhasePatterns();
        
        // 보스 행동 트리 설정
        SetupBossBehaviorTree();
        
        // 보스 UI 초기화
        InitializeBossUI();
        
        // 첫 페이즈 활성화
        ActivatePhase(0);
        
        // 전투 시작 코루틴
        StartCoroutine(StartBattleSequence());
    }
    
    // 전투 시작 시퀸스
    protected virtual IEnumerator StartBattleSequence()
    {
        // 초기 딜레이 (보스 등장 연출)
        yield return new WaitForSeconds(initialDelay);
        
        // 전투 시작 설정
        isBattleStarted = true;
        blackboard.SetValue("IsBattleStarted", true);
    }
    
    protected override void Update()
    {
        // 전투가 시작되지 않았으면 업데이트 건너뛰기
        if (!isBattleStarted)
            return;
        
        // 페이즈 전환 중이면 다른 업데이트 건너뛰기
        if (isChangingPhase)
            return;
        
        // 기절 상태 업데이트
        if (isStunned)
        {
            UpdateStunState();

            if (isStunned)
                return;
        }
        
        // 타깃 감지
        if (!isStunned && !isDead && !isUsingPattern)
        {
            DetectTarget();
        }
        
        // 패턴 쿨다운 업데이트
        UpdatePatternCooldown();
        
        // 페이즈 체크
        CheckPhaseTransition();
        
        // 패턴 업데이트
        if (!isStunned && !isDead && !isUsingPattern)
        {
            CheckAndTriggerPattern();
        }
        
        // BehaviorTree 평가
        if (behaviorTree != null)
        {
            behaviorTree.Evaluate();
        }
    }
    
    // 기절 상태 업데이트
    protected virtual void UpdateStunState()
    {
        if (!isStunned)
            return;
        
        stunTimer -= Time.deltaTime;
        blackboard.SetValue("StunTimer", stunTimer);

        if (stunTimer <= 0)
        {
            // 기절 상태 종료
            RecoverFromStun();
        }
    }

    // 보스 UI 초기화
    protected virtual void InitializeBossUI()
    {
        if (bossHpBar != null)
        {
            bossHpBar.SetActive(true);
        }
    }
    
    // 각 페이즈별 패턴 초기화 (자식클래스에서 구현)
    protected virtual void InitializePhasePatterns()
    {
        // 기본 패턴
    }

    // 보스 행동 트리 설정
    protected virtual void SetupBossBehaviorTree()
    {
        // 루트 노드 (셀렉터)
        Selector rootSelector = new Selector();
        
        // 사망 시퀀스 (최우선)
        Sequence deathSequence = new Sequence();
        ConditionNode isDeadCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsDead"));
        ActionNode deadAction = new ActionNode(HandleDeath);
        deathSequence.AddChild(isDeadCondition);
        deathSequence.AddChild(deadAction);
        
        // 기절 시퀸스
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
        
        // 패턴 사용 시퀀스
        Sequence patternSequence = new Sequence();
        ConditionNode isUsingPatternCondition = new ConditionNode(() => blackboard.GetValue<bool>("IsUsingPattern"));
        ActionNode patternAction = new ActionNode(ExecuteCurrentPattern);
        patternSequence.AddChild(isUsingPatternCondition);
        patternSequence.AddChild(patternAction);
        
        // 기본 공격 시퀸스
        Node attackSequence = CreateAttackSequence();
        
        // 추적 시퀸스 
        Sequence chaseSequence = new Sequence();
        ConditionNode isPlayerDetected = new ConditionNode(() => blackboard.GetValue<bool>("IsPlayerDetected"));
        ConditionNode isNotUsingPattern = new ConditionNode(() => !blackboard.GetValue<bool>("IsUsingPattern"));
        ActionNode chaseAction = new ActionNode(ChaseTarget);
        chaseSequence.AddChild(isPlayerDetected);
        chaseSequence.AddChild(isNotUsingPattern);
        chaseSequence.AddChild(chaseAction);
        
        // 대기 시퀸스 
        Sequence idleSequence = new Sequence();
        ActionNode idleAction = new ActionNode(IdleState);
        idleSequence.AddChild(idleAction);
        
        // 트리구성
        rootSelector.AddChild(deathSequence);       // 사망 상태 (최우선)
        rootSelector.AddChild(stunSequence);        // 기절 상태
        rootSelector.AddChild(phaseChangeSequence); // 페이즈 전환
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
    
    // 대기 상태 (보스는 제자리에서 대기)
    protected virtual NodeState IdleState()
    {
        if (isStunned || isDead || isUsingPattern || isChangingPhase)
            return NodeState.Failure;
        
        // 이동 정지
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        
        // Idle 애니메이션 재생

        return NodeState.Running;
    }
    
    // 기절 상태 처리
    protected virtual NodeState HandleStunState()
    {
        if (!isStunned)
            return NodeState.Failure;
        
        // 이동 및 모든 액션 중지
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        
        // 기절 애니메이션 재생
        
        return NodeState.Running;
    }
    
    // 패턴 쿨다운 업데이트
    protected virtual void UpdatePatternCooldown()
    {
        if (patternTimer > 0)
        {
            patternTimer -= Time.deltaTime;
            blackboard.SetValue("PatternTimer", patternTimer);
        }
    }
    
    // 패턴 실행 가능 여부 확인 및 트리거
    protected virtual void CheckAndTriggerPattern()
    {
        // 전투가 시작되지 않았거나, 패턴 사용 중이면 건너뛰기
        if (isStunned || !isBattleStarted || isUsingPattern)
            return;
        
        // 타겟이 없거나 감지되지 않았으면 건너뛰기
        if (target == null || !blackboard.GetValue<bool>("PlayerDetected"))
            return;
        
        // 쿨다운 확인
        if (Time.time - lastPatternTime < patternCooldown)
            return;
        
        // 현재 페이즈의 패턴이 없으면 건너뛰기
        if (currentPhase >= phasePatterns.Length || phasePatterns[currentPhase].Count == 0)
            return;
        
        // 현재 페이즈의 패턴 중 하나 선택
        int patternIndex = GetNextPatternIndex();

        if (patternIndex >= 0 && patternIndex < phasePatterns[currentPhase].Count)
        {
            // 패턴 시작
            isUsingPattern = true;
            blackboard.SetValue("IsUsingPattern", true);
            lastPatternTime = Time.time;
            lastPatternIndex = patternIndex;
            
            // 패턴 실행 준비
            if (movement != null)
            {
                movement.MoveTo(0);
            }
        }
    }
    
    // 보스에게 기절 상태 적용
    public virtual void IncrementStunCount()
    {
        if (isStunned || isDead || isChangingPhase)
            return;
        
        // 기절 카운트 증가
        stunCount++;
        blackboard.SetValue("StunCount", stunCount);
        
        // UI 업데이트
        UpdateBossUI();
        
        // 기절 임계값에 도달하면 기절 적용
        if (stunCount >= stunThreshold)
        {
            StartStun();
        }
    }
    
    // 기절 상태 시작
    protected virtual void StartStun()
    {
        // 기절 상태 설정
        isStunned = true;
        blackboard.SetValue("IsStunned", true);
        
        // 기절 카운트 초기화
        stunCount = 0;
        blackboard.SetValue("StunCount", stunCount);
        
        // 현재 패턴 중단
        if (isUsingPattern)
        {
            isUsingPattern = false;
            blackboard.SetValue("IsUsingPattern", false);
        }
        
        // 기절 타이머 설정
        stunTimer = stunDuration;
        blackboard.SetValue("StunTimer", stunTimer);
        
        // 기절 시각 효과 표시
        
        // 기절 애니메이션
        
        // UI 업데이트
        UpdateBossUI();
    }
    
    // 기절 회복
    protected virtual void RecoverFromStun()
    {
        isStunned = false;
        blackboard.SetValue("IsStunned", false);
        
        // 기절 시각 효과 제거
        
        // 기절 회복 애니메이션
    }
    
    // 다음 패턴 인덱스 선택
    protected virtual int GetNextPatternIndex()
    {
        // 작성 중
        return -1;
    }
    
    // 현재 패턴 실행
    protected virtual NodeState ExecuteCurrentPattern()
    {
        if (!isUsingPattern || currentPhase >= phasePatterns.Length || isStunned)
            return NodeState.Failure;

        if (lastPatternIndex < 0 || lastPatternIndex >= phasePatterns[currentPhase].Count)
        {
            isUsingPattern = false;
            blackboard.SetValue("IsUsingPattern", false);
            return NodeState.Failure;
        }
        
        NodeState patternState = phasePatterns[currentPhase][lastPatternIndex].Evaluate();

        if (patternState == NodeState.Success || patternState == NodeState.Failure)
        {
            // 패턴 완료 또는 실패
            isUsingPattern = false;
            blackboard.SetValue("IsUsingPattern", false);
            
            // 패턴 사용 후 쿨다운 설정
            patternTimer = patternCooldown;
            blackboard.SetValue("PatternTimer", patternTimer);
            
            // 패턴이 기절 카운트를 증가시키는지 확인
            if (currentPhase < patternStunFlags.Length &&
                lastPatternIndex < patternStunFlags[currentPhase].Count &&
                patternStunFlags[currentPhase][lastPatternIndex])
            {
                // 기절 카운트 증가
                IncrementStunCount();
            }
            
            return patternState;
        }

        return NodeState.Running;
    }
    
    // 페이즈 전환 체크
    protected virtual void CheckPhaseTransition()
    {
        if (isStunned ||isDead || isChangingPhase) return;
        
        // 체력 비율 계산
        float hpRatio = (float)currentHp / maxHp;
        
        // 다음 페이즈로 전환할지 체크
        for (int phase = phaseActivated.Length - 1; phase > currentPhase; phase--)
        {
            if (!phaseActivated[phase] && phaseHpThreshold.Length > phase && hpRatio <= phaseHpThreshold[phase])
            {
                StartPhaseTransition(phase);
                return;
            }
        }
    }
    
    // 페이즈 전환 시작
    protected virtual void StartPhaseTransition(int newPhase)
    {
        isChangingPhase = true;
        blackboard.SetValue("IsChangingPhase", true);
        
        // 기절 회복
        if (isStunned)
        {
            RecoverFromStun();
        }
        
        // 기절 카운트 초기화
        stunCount = 0;
        blackboard.SetValue("StunCount", stunCount);
        
        // 현재 진행 중인 패턴 중단
        isUsingPattern = false;
        blackboard.SetValue("IsUsingPattern", false);
        
        // 이동 중지
        if (movement != null)
        {
            movement.MoveTo(0);
        }
        
        // 페이즈 전환 코루틴 시작
        StartCoroutine(PhaseTransitionSequence(newPhase));
    }
    
    // 페이즈 전환 시퀸스
    protected virtual IEnumerator PhaseTransitionSequence(int newPhase)
    {
        // 페이즈 전환 연출 시간 동안 대기
        yield return new WaitForSeconds(phaseTransitionTime);
        
        // 새 페이즈 활성화
        ActivatePhase(newPhase);
        
        // 페이즈 전환 완료
        isChangingPhase = false;
        blackboard.SetValue("IsChangingPhase", false);
    }
    
    // 페이즈 활성화
    protected virtual void ActivatePhase(int phase)
    {
        if (phase >= 0 && phase < phaseActivated.Length)
        {
            currentPhase = phase;
            blackboard.SetValue("CurrentPhase", currentPhase);

            phaseActivated[phase] = true;
            
            // 페이즈별 초기화 작업 수행
            OnPhaseActivated(phase);
        }
    }
    
    // 페이즈 활성화 시 호출 (자식 클래스에서 오버라이드)
    protected virtual void OnPhaseActivated(int phase)
    {
        // 페이즈별 특수 처리
    }
    
    // 페이즈 전환 처리
    protected virtual NodeState HandlePhaseChange()
    {
        // 페이즈 전환 중에는 다른 상태 진입 방지
        return NodeState.Running;
    }
    
    // 피격 처리 오버라이드
    public override void DecreaseHp(int damage, bool isThrownWeapon = false)
    {
        // 페이즈 전환 중에 무적이면 데미지 무시
        if (isChangingPhase && isInvincibleDuringTransition)
            return;
        
        // 기본 데미지 처리
        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);
        blackboard.SetValue("CurrentHp", currentHp);
        
        // 사망 체크
        if (currentHp <= 0)
        {
            isDead = true;
            blackboard.SetValue("IsDead", true);
        }
        
        // 보스 UI 업데이트
        UpdateBossUI();
    }
    
    // 보스 UI 업데이트
    protected virtual void UpdateBossUI()
    {
        
    }
    
    // 면역 상태 추가
    public void AddImmuneStatus(System.Type statusType)
    {
        if (!immuneStatusList.Contains(statusType))
        {
            immuneStatusList.Add(statusType);
        }
    }
    
    // 면역 상태 제거
    public void RemoveImmuneStatus(System.Type statusType)
    {
        if (immuneStatusList.Contains(statusType))
        {
            immuneStatusList.Remove(statusType);
        }
    }
    
    // 면역 상태 확인
    public bool IsImmuneToStatus(System.Type statusType)
    {
        return immuneStatusList.Contains(statusType);
    }
    
    // 패턴 노드 추가 헬퍼 메서드
    protected void AddPatternToPhase(int phaseIndex, Node patternNode, bool causesStun)
    {
        if (phaseIndex >= 0 && phaseIndex < phasePatterns.Length)
        {
            phasePatterns[phaseIndex].Add(patternNode);
            patternStunFlags[phaseIndex].Add(causesStun);
        }
    }
    
    // 보스별 공격 패턴 노드 생성 헬퍼 메서드
    protected ActionNode CreatePatternNode(System.Func<NodeState> patternAction)
    {
        return new ActionNode(patternAction);
    }
    
    // 공격 범위 표시 (디버깅용)
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
    }
    
    // 현재 기절 카운트 반환
    public int GetStunCount()
    {
        return stunCount;
    }
    
    // 기절 상태 확인
    public bool IsStunned()
    {
        return isStunned;
    }

    public override void FreezeTime()
    {
        
    }

    public override void UnfreezeTime()
    {
        
    }
}
