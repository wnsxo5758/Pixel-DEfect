using System.Collections;
using UnityEngine;

/// <summary>
/// 게임의 전체 라이프사이클을 관리하는 싱글톤 매니저
/// 책임: 게임 시작/종료, 플레이어 사망/리스폰, 게임 상태 관리
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("게임 설정")]
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private float invulnerabilityTime = 2f;

    [Header("UI")]
    [SerializeField] private GameObject toBeContinued;

    // 게임 상태
    private bool isPlayerDead = false;
    private GameObject currentPlayer;

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake(); // Singleton<T>의 Awake 호출 (중복 제거 + DontDestroyOnLoad)

        Debug.Log($"[GameManager] Awake 호출 - 씬: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, InstanceID: {GetInstanceID()}");
    }

    private void Start()
    {
        // SceneSystem 이벤트 구독
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.OnSceneLoadComplete += OnSceneLoaded;
        }
    }

    protected override void OnDestroy()
    {
        Debug.Log($"[GameManager] OnDestroy 호출 - 씬: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, InstanceID: {GetInstanceID()}");

        base.OnDestroy(); // Singleton<T>의 OnDestroy 호출 (인스턴스 정리)

        // 이벤트 구독 해제
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.OnSceneLoadComplete -= OnSceneLoaded;
        }
    }

    #endregion

    #region Scene Events

    /// <summary>
    /// 씬 로드 완료 시 호출되는 콜백
    /// </summary>
    private void OnSceneLoaded(string sceneName)
    {
        StartCoroutine(InitializeSceneDelayed());
    }

    /// <summary>
    /// 씬 초기화 (플레이어 찾기 등)
    /// </summary>
    private IEnumerator InitializeSceneDelayed()
    {
        yield return new WaitForSeconds(0.1f);

        // 플레이어 레퍼런스 갱신
        currentPlayer = GameObject.FindGameObjectWithTag("Player");
        isPlayerDead = false;
    }

    #endregion

    #region Player Death & Respawn

    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    public void PlayerDied()
    {
        if (isPlayerDead) return;

        isPlayerDead = true;

        // 시간 정지 효과 해제
        DisableTimeEffects();
    }

    /// <summary>
    /// 플레이어 리스폰 (UI 버튼 등에서 호출)
    /// </summary>
    public void RespawnPlayer()
    {
        if (!isPlayerDead)
        {
            Debug.LogWarning("GameManager: 플레이어가 사망 상태가 아닙니다.");
            return;
        }

        StartCoroutine(RespawnSequence());
    }

    /// <summary>
    /// 플레이어 리스폰 시퀀스
    /// </summary>
    private IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (currentPlayer == null)
        {
            Debug.LogWarning("GameManager: 플레이어를 찾을 수 없어 게임을 재시작합니다.");
            RestartGame();
            yield break;
        }

        // 체크포인트가 있으면 해당 위치로 리스폰
        if (CheckpointManager.Instance != null)
        {
            // 플레이어 컨트롤러 리셋
            PlayerController controller = currentPlayer.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.ResetOnRespawn();
            }

            // 체크포인트로 이동
            CheckpointManager.Instance.RespawnAtCheckpoint(currentPlayer);

            // 체력 회복
            PlayerHp playerHp = currentPlayer.GetComponent<PlayerHp>();
            if (playerHp != null)
            {
                playerHp.SetHp(playerHp.GetMaxHp());
            }

            isPlayerDead = false;
        }
        else
        {
            // 체크포인트가 없으면 씬 재시작
            RestartCurrentScene();
        }
    }

    #endregion

    #region Fall Damage

    /// <summary>
    /// 플레이어 낙사 처리
    /// </summary>
    /// <param name="fallDamage">낙사 데미지</param>
    /// <param name="isWater">물에 빠졌는지 여부</param>
    public void ProcessPlayerFall(int fallDamage, bool isWater)
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("GameManager: 플레이어를 찾을 수 없습니다.");
            return;
        }

        StartCoroutine(FallDamageSequence(fallDamage, isWater));
    }

    /// <summary>
    /// 낙사 데미지 처리 시퀀스
    /// </summary>
    private IEnumerator FallDamageSequence(int fallDamage, bool isWater)
    {
        PlayerHp playerHp = currentPlayer.GetComponent<PlayerHp>();
        if (playerHp != null)
        {
            // 낙사 데미지 적용
            playerHp.TakeFallDamage(fallDamage, isWater);

            // 사망했으면 리스폰 처리는 PlayerHp가 담당
            if (playerHp.GetCurrentHp() <= 0)
            {
                yield break;
            }
        }

        // 살아있으면 Fallback 체크포인트로 이동
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.MoveToFallbackCheckpoint(currentPlayer);
        }
    }

    #endregion

    #region Game Flow Control

    /// <summary>
    /// 게임 시작 (타이틀 → Stage1)
    /// </summary>
    public void StartGame()
    {
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.LoadScene("Stage1");
        }
    }

    /// <summary>
    /// 현재 씬 재시작
    /// </summary>
    public void RestartCurrentScene()
    {
        StartCoroutine(RestartSequence());
    }

    private IEnumerator RestartSequence()
    {
        yield return new WaitForSeconds(respawnDelay);

        // 체크포인트 초기화
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.ResetCheckpoints();
        }

        // 현재 씬 재시작
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.RestartCurrentScene();
        }
    }

    /// <summary>
    /// 게임 재시작 (타이틀로 복귀)
    /// </summary>
    public void RestartGame()
    {
        StartCoroutine(RestartGameSequence());
    }

    private IEnumerator RestartGameSequence()
    {
        yield return new WaitForSeconds(respawnDelay);

        // 체크포인트 초기화
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.ResetCheckpoints();
        }

        // 플레이어 데이터 초기화
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ResetPlayerData();
        }

        // 타이틀로 이동
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.LoadTitleScene();
        }
    }

    /// <summary>
    /// 게임 종료 (엔딩 씬으로)
    /// </summary>
    public void EndGame()
    {
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.LoadEndScene();
        }
    }

    /// <summary>
    /// Stage2 클리어 시 호출
    /// </summary>
    public void GameClear()
    {
        // To Be Continued UI 표시
        if (toBeContinued != null)
        {
            toBeContinued.SetActive(true);
        }

        // 엔딩 씬으로 이동
        StartCoroutine(GameClearSequence());
    }

    private IEnumerator GameClearSequence()
    {
        yield return new WaitForSeconds(3f); // UI 표시 시간
        EndGame();
    }

    #endregion

    #region Utility

    /// <summary>
    /// 시간 정지 효과 해제
    /// </summary>
    private void DisableTimeEffects()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.IsTimeFrozen())
        {
            TimeManager.Instance.ResumeTime();
        }
    }

    /// <summary>
    /// 현재 플레이어가 죽었는지 확인
    /// </summary>
    public bool IsPlayerDead() => isPlayerDead;

    /// <summary>
    /// 현재 플레이어 오브젝트 반환
    /// </summary>
    public GameObject GetCurrentPlayer() => currentPlayer;

    #endregion
}
