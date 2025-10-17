# CLAUDE.md

이 파일은 Claude Code (claude.ai/code)가 이 저장소의 코드 작업을 할 때 참고할 가이드를 제공합니다.

## 프로젝트 개요

**PixelDEfect**는 Unity 2022.3.55f1로 제작된 Unity 2D 액션 플랫포머 게임입니다. 시간 조작 메커니즘, 행동 트리 기반 적 AI, 체크포인트 시스템, 메모리 효율적인 오브젝트 풀링 기능을 포함합니다.

## 개발 환경 설정

### Unity 버전
- **Unity 2022.3.55f1** (필수)
- 플랫폼: Windows (주 개발 환경)

### 프로젝트 구조
```
Assets/Scripts/
├── Manager/          # 싱글톤 매니저들 (GameManager, SceneSystem, TimeManager 등)
├── Player/           # 플레이어 컨트롤러, 상태 머신, 체력, 스킬
├── Enemy/            # 행동 트리 AI 시스템 및 적 타입들
├── Weapon/           # 무기 및 총알 시스템
├── DesignPattern/    # 재사용 가능한 패턴 (Singleton 기본 클래스)
├── Stage/            # 씬 관리 (ScenePortal, StageData)
├── UI/               # 사용자 인터페이스 컴포넌트
└── [기타 시스템]      # Camera, Items, Obstacles 등
```

### 빌드 및 테스트
- Unity Hub에서 Unity 2022.3.55f1로 프로젝트 열기
- Unity Editor에서 Play 버튼으로 테스트
- 빌드: **File > Build Settings**
- 주 브랜치: `Develop`

## 핵심 아키텍처

### 1. 매니저 시스템 (싱글톤 기반 - SOLID 원칙 적용)

모든 매니저는 싱글톤 패턴을 사용하여 전역 단일 인스턴스를 보장합니다. 각 매니저는 **단일 책임 원칙(SRP)**을 따르며, 이벤트 기반으로 느슨하게 결합되어 있습니다.

**제네릭 싱글톤 베이스** ([DesignPattern/Singleton.cs](Assets/Scripts/DesignPattern/Singleton.cs))
```csharp
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
```
- 인스턴스가 없으면 자동 생성
- `DontDestroyOnLoad`로 중복 방지
- 사용 매니저: TimeManager, CheckpointManager, SkillManager, AudioManager, PlayerPersistenceSystem, PlayerDataManager

**수동 싱글톤**
- GameManager ([Manager/GameManager.cs](Assets/Scripts/Manager/GameManager.cs))
- SceneSystem ([Manager/SceneSystem.cs](Assets/Scripts/Manager/SceneSystem.cs))

#### 주요 매니저 (역할별 분리)

**SceneSystem** - 씬 전환 전담 (단일 책임: 씬 로드/언로드)
- 책임: 씬 로드, 페이드 효과, 전환 이벤트 발행
- 메서드:
  - `LoadScene(sceneName)` - 씬 전환
  - `LoadTitleScene()`, `LoadStage1()`, `LoadStage2()`, `LoadEndScene()` - 특정 씬으로 이동
  - `RestartCurrentScene()` - 현재 씬 재시작
  - `IsTransitioning()` - 전환 중 확인
- 이벤트: `OnSceneLoadStart`, `OnSceneLoadComplete`, `OnTransitionStart`, `OnTransitionComplete`
- 특징: 페이드 효과 통합, 씬 타입 검증 (`IsGameplayScene()`, `IsTitleScene()` 등)

**GameManager** - 게임 라이프사이클 관리 (단일 책임: 게임 흐름 제어)
- 책임: 게임 시작/종료, 플레이어 사망/리스폰, 게임 상태 관리
- 메서드:
  - `PlayerDied()` - 플레이어 사망 처리
  - `ProcessPlayerFall(damage, water)` - 낙사 처리
  - `StartGame()` - 게임 시작 (Title → Stage1)
  - `RestartGame()` - 게임 재시작 (타이틀로 복귀)
  - `EndGame()` - 게임 종료 (엔딩 씬으로)
  - `GameClear()` - Stage2 클리어 시 호출
- 특징: SceneSystem 이벤트 구독, CheckpointManager와 통합

**PlayerPersistenceSystem** - 플레이어 오브젝트 지속성 (단일 책임: DontDestroyOnLoad 관리)
- 책임: 플레이어 오브젝트의 씬 간 지속성, 위치 지정, 활성화/비활성화
- 메서드:
  - `SetPlayerPersistent(player)` - 플레이어 DontDestroyOnLoad 설정
  - `SetNextSpawnPosition(position)` - 다음 씬 스폰 위치 설정
  - `DestroyPersistentPlayer()` - 플레이어 제거
  - `DisablePlayerInput()` / `EnablePlayerInput()` - 입력 제어
  - `MovePlayer(position)` - 플레이어 위치 이동
- 특징: SceneSystem 이벤트 구독, 타이틀/엔딩 씬에서 자동 제거

**PlayerDataManager** - 플레이어 데이터 저장/복원 (단일 책임: 데이터 관리)
- 책임: HP, 무기, 스킬 정보 저장/복원, 게임 재시작 시 초기화
- 메서드:
  - `SavePlayerData()` - 플레이어 데이터 저장
  - `RestorePlayerData()` - 플레이어 데이터 복원
  - `ResetPlayerData()` - 데이터 초기화
  - `IsFirstStage()` - 첫 스테이지 여부 확인
- 특징: SceneSystem 이벤트 구독, PlayerStateData 사용, Stage1에서는 복원하지 않음

**TimeManager** - 시간 정지 스킬 시스템
- 반경 기반 시간 정지 메커니즘 구현
- 엔티티용 `ITimeAffected` 인터페이스 사용
- 시각적 확장/축소 효과
- 쿨다운 관리

**CheckpointManager** - 체크포인트 시스템
- 두 가지 체크포인트 타입: Respawn (메인), Fallback (낙하 안전)
- 메서드: `SetRespawnCheckpoint()`, `SetFallbackCheckpoint()`, `RespawnAtCheckpoint()`

**SkillManager** - 플레이어 스킬 추적
- 해금된 스킬 추적: `Teleport`, `TimeStop`
- 이벤트: `OnSkillAcquired`

### 2. 적 AI - 행동 트리 아키텍처

**핵심 시스템** ([Enemy/BT/](Assets/Scripts/Enemy/BT/))

모든 적 AI는 전통적인 FSM이 아닌 행동 트리 패턴을 사용합니다:

**노드 타입:**
- `Node` (추상 베이스) - `NodeState` 반환 (Running/Success/Failure)
- `Sequence` - 모든 자식이 성공해야 함 (AND 로직)
- `Selector` - 첫 번째 성공이 반환됨 (OR 로직)
- `Inverter` - 자식 결과를 반전
- `ActionNode` - 람다 함수 실행
- `ConditionNode` - 불린 체크

**Blackboard:** 행동 컨텍스트를 위한 공유 데이터 딕셔너리
```csharp
SetValue<T>(key, value) / GetValue<T>(key)
```

**적 계층 구조:**
```
EnemyBT (기본 AI)
└── ManaBT (점프, 스킬 추가)
    └── WrenchBT (부채꼴 공격)
```

**EnemyBT** ([Enemy/EnemyBT.cs](Assets/Scripts/Enemy/EnemyBT.cs)) - 모든 적의 기본
- 우선순위: Death → Hit → Attacking → Attack → Chase → Patrol
- 주요 메서드:
  - `DetectTarget()` - 원형 감지 + 레이캐스트 검증
  - `ChaseTarget()` - 플레이어 추적
  - `Patrol()` - 벽 감지와 함께 랜덤 이동
  - `DecreaseHp()` - 스턴과 함께 데미지 처리
  - `FreezeTime()` / `UnfreezeTime()` - 시간 정지 통합
- Blackboard 값: `Target`, `PlayerDetected`, `IsHit`, `IsDead`, `StunTimer`, `CurrentHp`

**ManaBT** ([Enemy/ManaMan/ManaBT.cs](Assets/Scripts/Enemy/ManaMan/ManaBT.cs))
- Skill Sequence 추가 (공격보다 높은 우선순위)
- 점프 메커니즘: `CanJumpOverWall()`
- 정지형 적을 위한 Idle 상태

**WrenchBT** ([Enemy/ManaMan/WrenchBT.cs](Assets/Scripts/Enemy/ManaMan/WrenchBT.cs))
- 부채꼴 패턴 렌치 투척
- 총알 관리를 위해 메모리 풀 사용
- 메서드: `PerformAttack()`, `OnSkillEffectTrigger()` (부채꼴 패턴)

### 3. 플레이어 시스템

**상태 머신** ([Player/PlayerStateMachine.cs](Assets/Scripts/Player/))
```csharp
PlayerStateMachine<T> / State<T>
```
- 3단계 상태: `Enter()`, `Execute()`, `Exit()`
- 전역 상태 및 상태 히스토리 지원
- 상태들: Idle, Run, Jump, Crawl, Hit, Attack 등

**PlayerHp** ([Player/PlayerHp.cs](Assets/Scripts/Player/PlayerHp.cs)) - 체력 및 데미지
- 최대 HP: 3 (설정 가능)
- 두 가지 데미지 메서드:
  - `DecreaseHp(damage, DeathData)` - 단순
  - `DecreaseHp(damage, knockBack, DeathData)` - 넉백 포함
- 시각적 깜빡임과 함께 무적 프레임
- 스턴 지속시간: 일반 0.5초, 특수 상태 (Crawl, Attack) 0.25초
- 사망 원인: `RangedAttack`, `Press`, `Drowning`, `Fall`, `Environmental`
- 이벤트: `OnPlayerDeath`, `OnPlayerDeathWithCause`, `OnHpChanged`

### 4. 전투 및 데미지 시스템

**IDamageable 인터페이스** ([IDamageable.cs](Assets/Scripts/IDamageable.cs))
```csharp
void DecreaseHP(int amount);
void IncreaseHP(int amount);
void Death();
```
- 구현: `PlayerHp` (완전), `EnemyHp` (스텁)

**무기** ([Weapon/WeaponBase.cs](Assets/Scripts/Weapon/WeaponBase.cs))
- 추상 베이스: weaponName, damage, attackCooldown
- 서브클래스: Crowbar, 투척 무기들

**총알** ([Bullet/BulletBase.cs](Assets/Scripts/Bullet/BulletBase.cs))
- 물리 기반 이동 (Rigidbody2D)
- 방향에 맞춰 자동 회전
- 메모리 풀 호환
- 충돌: 플레이어에 트리거, 데미지 적용, 회피 시스템 호출
- 시간 정지 지원

### 5. 오브젝트 풀링

**MemoryPool** ([MemoryPool.cs](Assets/Scripts/MemoryPool.cs))
```csharp
MemoryPool pool = new MemoryPool(prefab);
GameObject obj = pool.ActivePoolItem();
pool.DeactivatePoolItems(obj);
```
- 제네릭 템플릿 기반 풀링
- 초기 배치: 5개 오브젝트
- 부족 시 5개씩 자동 확장
- 사용처: 총알, 적, 이펙트

**특수 풀:**
- `ImpactMemoryPool` - 파티클 이펙트
- `EnemyMemoryPool` - 적 스폰

### 6. 씬 관리

**ScenePortal** ([Stage/ScenePortal.cs](Assets/Scripts/Stage/ScenePortal.cs))
- 포털 기반 씬 전환
- 설정: 타겟 씬, 스폰 위치, 자동 전환, 일회용, 딜레이
- 상호작용 프롬프트
- 흐름: Trigger → Prompt → `SceneSystem.LoadScene()` → Fade → Spawn

### 7. 스킬 시스템

**ITimeAffected 인터페이스** ([Player/Skill/ITimeAffected.cs](Assets/Scripts/Player/Skill/ITimeAffected.cs))
```csharp
void OnTimeStop();
void OnTimeResume();
void ReceiveDamageInFrozenTime(int damage, Vector2 direction);
bool IsInTimeFreezeRange(Vector3 originPosition, float range);
```

**시간 정지 흐름:**
1. 플레이어가 공격을 성공적으로 회피
2. `TimeManager.TriggerTimeStopOnDodge()` 호출
3. 반경 체크로 영향받는 엔티티 찾기 (Physics2D.OverlapCircleAll)
4. 시각 효과 확장
5. 모든 엔티티가 `OnTimeStop()` 콜백 수신
6. 행동 트리 일시정지, 물리 정지
7. 지속시간 카운트다운
8. `OnTimeResume()`으로 모든 엔티티 복원
9. 쿨다운 적용

## 사용된 디자인 패턴

1. **Singleton** - 모든 매니저 클래스
2. **State Machine** - 플레이어 제어 (제네릭 템플릿)
3. **Behavior Tree** - 적 AI (조합 가능한 노드)
4. **Object Pool** - 메모리 효율적 재사용
5. **Observer** - 전반적인 이벤트 시스템 (SceneSystem, TimeManager, SkillManager, PlayerHp)
6. **Inheritance Hierarchy** - EnemyBT → ManaBT → WrenchBT

## 주요 상호작용 흐름

### 전투 흐름
```
무기 발사 → 풀에서 총알 생성 → 물리 이동
→ OnTriggerEnter2D → Enemy.DecreaseHp()
→ 플래시 + 스턴 + 넉백 → BT 재평가
→ 총알 풀로 반환
```

### 사망 흐름
```
HP ≤ 0 → 사망 상태 → 애니메이션 트리거
→ 물리 비활성화 → 페이드/파괴
→ 체크포인트에서 리스폰 → 체력 완전 회복 → 입력 활성화
```

### 게임 흐름 (Title → Stage1 → Stage2 → Ending)
```
[Title Scene]
    ↓ GameManager.StartGame()
[Stage1]
    ↓ 플레이어 진입 (PlayerPersistenceSystem이 DontDestroyOnLoad 설정)
    ↓ 보스 처치
    ↓ ScenePortal 트리거
[Stage2]
    ↓ PlayerDataManager가 Stage1 데이터 복원 (HP, 무기, 스킬)
    ↓ 스테이지 클리어
    ↓ GameManager.GameClear()
[Ending Scene]
```

### 매니저 의존성 (이벤트 기반 결합)
```
SceneSystem (루트 - 이벤트 발행)
├── OnSceneLoadStart / OnSceneLoadComplete
├── OnTransitionStart / OnTransitionComplete
│
└── 구독자들 (느슨한 결합):
    ├── GameManager (씬 로드 완료 시 플레이어 찾기)
    ├── PlayerPersistenceSystem (씬 전환 시 플레이어 관리)
    └── PlayerDataManager (씬 전환 시 데이터 저장/복원)

GameManager
├── CheckpointManager (리스폰 위치)
├── PlayerDataManager (게임 재시작 시 데이터 초기화)
└── TimeManager (시간 정지 해제)

PlayerPersistenceSystem
└── PlayerController (입력 활성화/비활성화)

PlayerDataManager
├── PlayerHp (체력 저장/복원)
├── PlayerAttack (무기 저장/복원)
└── SkillManager (스킬 저장/복원)

TimeManager
├── SkillManager (시간 정지 검증)
└── ITimeAffected 엔티티들
```

## 중요 개발 참고사항

### 적 작업 시
- 새로운 적은 `EnemyBT` 또는 `ManaBT`를 상속해야 함 (FSM 클래스 아님)
- 생성자에서 항상 행동 트리 구조 구현
- 노드 간 공유 상태는 Blackboard 사용
- 시간 정지 호환성을 위해 `ITimeAffected` 구현
- GC 스파이크 방지를 위해 투사체에 메모리 풀 사용

### 매니저 작업 시
- 싱글톤 매니저의 다중 인스턴스 절대 생성 금지
- **SOLID 원칙 준수**: 각 매니저는 단일 책임만 가져야 함
- **이벤트 기반 통신**: 폴링 대신 SceneSystem 이벤트 구독 사용
- **씬 전환**: SceneSystem만 사용 - `SceneManager.LoadScene()` 직접 사용 금지
- **플레이어 지속성**: PlayerPersistenceSystem 사용 - `DontDestroyOnLoad(player)` 직접 호출 금지
- **플레이어 데이터**: PlayerDataManager 사용 - 씬 간 데이터는 이벤트로 자동 처리
- CheckpointManager는 두 가지 체크포인트 타입 필요: respawn, fallback

### 플레이어 작업 시
- 플레이어 상태 머신이 상태 전환 강제
- PlayerHp는 여러 이벤트 발생 - 적절한 이벤트 구독
- 회피 시스템은 TimeManager와 통합
- 특수 상태 (Crawl, Attack)는 감소된 스턴 지속시간

### 씬 작업 시
- 전환은 항상 SceneSystem 사용
- **ScenePortal에서 스폰 위치 설정**: `PlayerPersistenceSystem.SetNextSpawnPosition()` 호출
- **플레이어 지속성**: PlayerPersistenceSystem이 자동 처리
- **플레이어 데이터 복원**: PlayerDataManager가 자동 처리 (Stage1 → Stage2)
- 로드 후 초기화는 SceneSystem 이벤트 구독 사용

### 풀링 작업 시
- Awake() 또는 Start()에서 풀 생성
- 항상 오브젝트를 풀로 비활성화 (파괴 금지)
- 풀은 자동 확장 - 수동 크기 관리 불필요
- 풀에서 활성화 시 오브젝트 상태 리셋

## 최근 개발 활동

최근 커밋 기준:
- 출혈 효과 구현
- 총알 벽 관통 수정
- 매니저 싱글톤 재구조화
- 마나봇 피격 감지 수정

**2025년 주요 재구조화 (SOLID 원칙 적용):**
- 게임 흐름 관리 시스템 완전 재작성
- 매니저 책임 분리: GameManager, SceneSystem, PlayerPersistenceSystem, PlayerDataManager
- 이벤트 기반 느슨한 결합 구현
- 기존 SceneTransitionManager, PlayerPersistenceManager, PlayerStateManager 대체
- ScenePortal 새 시스템에 맞게 수정

## 코드 스타일 참고사항

- 매니저는 PascalCase static Instance 프로퍼티 사용
- 이벤트 네이밍: `OnEventName` (예: `OnPlayerDeath`)
- 행동 트리 노드는 생성자에서 조합
- Blackboard 키는 문자열 리터럴 사용
- 비동기 작업에 코루틴 사용 (페이드, 딜레이)
