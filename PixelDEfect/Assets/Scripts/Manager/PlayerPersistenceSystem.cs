using UnityEngine;

/// <summary>
/// 플레이어 오브젝트의 씬 간 지속성을 관리하는 싱글톤 시스템
/// 책임: 플레이어 DontDestroyOnLoad 관리, 위치 지정, 활성화/비활성화
/// </summary>
public class PlayerPersistenceSystem : Singleton<PlayerPersistenceSystem>
{
    [Header("디버그")]
    [SerializeField] private bool debugMode = false;

    // 플레이어 상태
    private GameObject persistentPlayer;
    private Vector3 nextSpawnPosition = Vector3.zero;
    private bool hasSpawnPosition = false;

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

        if (debugMode)
        {
            Debug.Log($"[PlayerPersistenceSystem] Awake 호출 - 씬: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        }

        // SceneSystem 이벤트 구독
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.OnSceneLoadStart += OnSceneLoadStart;
            SceneSystem.Instance.OnSceneLoadComplete += OnSceneLoadComplete;
        }
    }

    private void Start()
    {
        // 씬에 플레이어가 있으면 참조 저장
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (SceneSystem.Instance != null && SceneSystem.Instance.IsPlayerRequired(currentScene))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                persistentPlayer = player;

                if (debugMode)
                {
                    Debug.Log($"[PlayerPersistenceSystem] 플레이어 참조 저장: {player.name}");
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        if (debugMode)
        {
            Debug.Log($"[PlayerPersistenceSystem] OnDestroy 호출 - 씬: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        }

        base.OnDestroy(); // Singleton<T>의 OnDestroy 호출

        // 이벤트 구독 해제
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.OnSceneLoadStart -= OnSceneLoadStart;
            SceneSystem.Instance.OnSceneLoadComplete -= OnSceneLoadComplete;
        }
    }

    #endregion

    #region Scene Events

    /// <summary>
    /// 씬 로드 시작 시 호출
    /// </summary>
    private void OnSceneLoadStart(string sceneName)
    {
        if (debugMode)
        {
            Debug.Log($"[PlayerPersistenceSystem] ===== 씬 로드 시작: {sceneName} =====");
            Debug.Log($"[PlayerPersistenceSystem] 현재 플레이어: {(persistentPlayer != null ? persistentPlayer.name : "없음")}");
            Debug.Log($"[PlayerPersistenceSystem] 스폰 위치 설정됨: {hasSpawnPosition} ({nextSpawnPosition})");
        }

        // 플레이어가 필요 없는 씬이면 참조 초기화
        if (!SceneSystem.Instance.IsPlayerRequired(sceneName))
        {
            if (debugMode)
            {
                Debug.Log($"[PlayerPersistenceSystem] {sceneName}은 플레이어가 필요 없는 씬 → 참조 초기화");
            }
            persistentPlayer = null;
            return;
        }

        // 플레이어 입력 비활성화
        DisablePlayerInput();

        if (debugMode)
        {
            Debug.Log($"[PlayerPersistenceSystem] 플레이어 입력 비활성화");
        }
    }

    /// <summary>
    /// 씬 로드 완료 시 호출
    /// </summary>
    private void OnSceneLoadComplete(string sceneName)
    {
        if (debugMode)
        {
            Debug.Log($"[PlayerPersistenceSystem] ===== 씬 로드 완료: {sceneName} =====");
        }

        // 플레이어가 필요 없는 씬이면 처리 안 함
        if (!SceneSystem.Instance.IsPlayerRequired(sceneName))
        {
            if (debugMode)
            {
                Debug.Log($"[PlayerPersistenceSystem] {sceneName}은 플레이어가 필요 없는 씬 → 처리 안 함");
            }
            return;
        }

        // 플레이어 처리
        HandlePlayerInNewScene();

        if (debugMode)
        {
            Debug.Log($"[PlayerPersistenceSystem] 최종 플레이어 상태:");
            Debug.Log($"  - 플레이어: {(persistentPlayer != null ? persistentPlayer.name : "없음")}");
            Debug.Log($"  - 위치: {(persistentPlayer != null ? persistentPlayer.transform.position.ToString() : "N/A")}");
            Debug.Log($"  - 입력 활성화: {(persistentPlayer != null && persistentPlayer.GetComponent<PlayerController>() != null ? persistentPlayer.GetComponent<PlayerController>().enabled.ToString() : "N/A")}");
        }
    }

    #endregion

    #region Player Persistence

    /// <summary>
    /// 플레이어 참조 설정 (씬 로드 완료 시 호출)
    /// </summary>
    public void SetPlayerPersistent(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("[PlayerPersistenceSystem] player가 null입니다.");
            return;
        }

        persistentPlayer = player;

        if (debugMode)
        {
            Debug.Log($"[PlayerPersistenceSystem] 플레이어 참조 설정: {player.name}");
        }
    }

    /// <summary>
    /// 다음 씬에서 스폰할 위치 설정
    /// </summary>
    public void SetNextSpawnPosition(Vector3 position)
    {
        nextSpawnPosition = position;
        hasSpawnPosition = true;

        if (debugMode)
        {
            Debug.Log($"PlayerPersistenceSystem: 다음 스폰 위치 설정 - {position}");
        }
    }

    /// <summary>
    /// 플레이어가 지속 중인지 확인
    /// </summary>
    public bool HasPersistentPlayer() => persistentPlayer != null;

    /// <summary>
    /// 지속 중인 플레이어 반환
    /// </summary>
    public GameObject GetPersistentPlayer() => persistentPlayer;

    #endregion

    #region Player Control

    /// <summary>
    /// 플레이어 입력 비활성화
    /// </summary>
    public void DisablePlayerInput()
    {
        GameObject player = GetOrFindPlayer();
        if (player == null) return;

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }

    /// <summary>
    /// 플레이어 입력 활성화
    /// </summary>
    public void EnablePlayerInput()
    {
        GameObject player = GetOrFindPlayer();
        if (player == null) return;

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    /// <summary>
    /// 플레이어 위치 이동
    /// </summary>
    public void MovePlayer(Vector3 position)
    {
        GameObject player = GetOrFindPlayer();
        if (player != null)
        {
            player.transform.position = position;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 새로운 씬에서 플레이어 처리
    /// </summary>
    private void HandlePlayerInNewScene()
    {
        // 씬에서 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("[PlayerPersistenceSystem] 씬에서 플레이어를 찾을 수 없습니다!");
            return;
        }

        // 플레이어 참조 저장
        persistentPlayer = player;

        if (debugMode)
        {
            Debug.Log($"[PlayerPersistenceSystem] 플레이어 발견: {player.name}");
        }

        // 스폰 위치가 설정되어 있으면 이동
        if (hasSpawnPosition)
        {
            player.transform.position = nextSpawnPosition;
            hasSpawnPosition = false;

            if (debugMode)
            {
                Debug.Log($"[PlayerPersistenceSystem] 플레이어 위치 이동: {nextSpawnPosition}");
            }

            // 플레이어 컨트롤러 리셋
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.ResetOnRespawn();
            }
        }

        // 입력 활성화
        EnablePlayerInput();
    }

    /// <summary>
    /// 플레이어 찾기 또는 반환
    /// </summary>
    private GameObject GetOrFindPlayer()
    {
        if (persistentPlayer != null)
        {
            return persistentPlayer;
        }

        return GameObject.FindGameObjectWithTag("Player");
    }

    #endregion
}
