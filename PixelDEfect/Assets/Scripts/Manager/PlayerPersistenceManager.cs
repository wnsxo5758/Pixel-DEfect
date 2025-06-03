using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistenceManager : MonoBehaviour
{
    public static PlayerPersistenceManager Instance { get; private set; }
    
    [Header("씬 전환 설정")]
    [SerializeField] private string[] destroyOnLoadScenes = { "Title", "EndScene" }; // DontDestroyOnLoad 해제할 씬들
    [SerializeField] private float transitionDelay = 0.1f; // 씬 로드 후 플레이어 이동 지연
    
    [Header("디버그")]
    [SerializeField] private bool debugMode = false;
    
    private GameObject persistentPlayer;
    private bool isPlayerPersistent = false;
    private Vector3 nextSceneSpawnPosition = Vector3.zero;
    private bool hasSpawnPosition = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 씬 로드 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 게임 시작 시 플레이어 찾기
        StartCoroutine(FindAndSetupPlayerDelayed());
    }
    
    private IEnumerator FindAndSetupPlayerDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        FindAndSetupPlayer();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
    }
    
    /// <summary>
    /// 플레이어를 찾아서 지속성 설정
    /// </summary>
    public void FindAndSetupPlayer()
    {
        if (persistentPlayer != null) return;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SetupPlayerPersistence(player);
        }
        else if (debugMode)
        {
            Debug.LogWarning("PlayerPersistenceManager: Player를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 플레이어 지속성 설정
    /// </summary>
    /// <param name="player">지속시킬 플레이어 오브젝트</param>
    public void SetupPlayerPersistence(GameObject player)
    {
        if (player == null || isPlayerPersistent) return;
        
        persistentPlayer = player;
        isPlayerPersistent = true;
        
        // DontDestroyOnLoad 적용
        DontDestroyOnLoad(persistentPlayer);
        
        if (debugMode)
        {
            Debug.Log("PlayerPersistenceManager: 플레이어 지속성 설정 완료");
        }
    }
    
    /// <summary>
    /// 다음 씬에서 플레이어가 스폰될 위치 설정
    /// </summary>
    /// <param name="spawnPosition">스폰 위치</param>
    public void SetNextSceneSpawnPosition(Vector3 spawnPosition)
    {
        nextSceneSpawnPosition = spawnPosition;
        hasSpawnPosition = true;
        
        if (debugMode)
        {
            Debug.Log($"PlayerPersistenceManager: 다음 씬 스폰 위치 설정 - {spawnPosition}");
        }
    }
    
    /// <summary>
    /// 씬 전환 시작
    /// </summary>
    /// <param name="sceneName">전환할 씬 이름</param>
    /// <param name="spawnPosition">플레이어 스폰 위치</param>
    public void TransitionToScene(string sceneName, Vector3 spawnPosition)
    {
        SetNextSceneSpawnPosition(spawnPosition);
        
        // 특정 씬들로 전환 시 플레이어 지속성 해제
        if (ShouldDestroyPlayer(sceneName))
        {
            DestroyPersistentObjects();
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// 씬 전환 (스폰 위치 없음)
    /// </summary>
    /// <param name="sceneName">전환할 씬 이름</param>
    public void TransitionToScene(string sceneName)
    {
        hasSpawnPosition = false;
        
        if (ShouldDestroyPlayer(sceneName))
        {
            DestroyPersistentObjects();
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// 플레이어를 제거해야 하는 씬인지 확인
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    /// <returns>제거 여부</returns>
    private bool ShouldDestroyPlayer(string sceneName)
    {
        foreach (string destroyScene in destroyOnLoadScenes)
        {
            if (sceneName.Equals(destroyScene, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 지속성 객체들 제거
    /// </summary>
    public void DestroyPersistentObjects()
    {
        if (debugMode)
        {
            Debug.Log("PlayerPersistenceManager: 지속성 객체들 제거 시작");
        }
        
        // 플레이어 제거
        if (persistentPlayer != null)
        {
            Destroy(persistentPlayer);
            persistentPlayer = null;
            isPlayerPersistent = false;
        }
        
        // GameManager 제거
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }
        
        // PlayerStateManager 제거
        if (PlayerStateManager.Instance != null)
        {
            Destroy(PlayerStateManager.Instance.gameObject);
        }
        
        // TimeManager 제거
        if (TimeManager.Instance != null)
        {
            Destroy(TimeManager.Instance.gameObject);
        }
        
        // SkillManager 제거
        if (SkillManager.Instance != null)
        {
            Destroy(SkillManager.Instance.gameObject);
        }
        
        // CheckpointManager 제거
        if (CheckpointManager.Instance != null)
        {
            Destroy(CheckpointManager.Instance.gameObject);
        }
        
        // 자기 자신 제거
        Destroy(gameObject);
        
        if (debugMode)
        {
            Debug.Log("PlayerPersistenceManager: 지속성 객체들 제거 완료");
        }
    }
    
    /// <summary>
    /// 씬 로드 완료 시 호출
    /// </summary>
    /// <param name="scene">로드된 씬</param>
    /// <param name="mode">로드 모드</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (debugMode)
        {
            Debug.Log($"PlayerPersistenceManager: 씬 로드 완료 - {scene.name}");
        }

        if (persistentPlayer != null)
        {
            persistentPlayer.transform.position = nextSceneSpawnPosition;
        }
        
        // 지속성 해제 대상 씬이면 처리하지 않음
        if (ShouldDestroyPlayer(scene.name))
        {
            return;
        }
        
        StartCoroutine(HandleSceneLoaded(scene));
    }
    
    /// <summary>
    /// 씬 로드 후 처리
    /// </summary>
    /// <param name="scene">로드된 씬</param>
    private IEnumerator HandleSceneLoaded(Scene scene)
    {
        yield return new WaitForSeconds(transitionDelay);
        
        // 플레이어가 지속되고 있고 스폰 위치가 설정되어 있으면 이동
        if (persistentPlayer != null && hasSpawnPosition)
        {
            MovePlayerToSpawnPosition();
        }
        // 플레이어가 없으면 새로 찾기
        else if (persistentPlayer == null)
        {
            FindAndSetupPlayer();
        }
        
        // 플레이어 상태 복원
        if (PlayerStateManager.Instance != null && persistentPlayer != null)
        {
            PlayerStateManager.Instance.RestorePlayerState(persistentPlayer);
        }
        
        // TimeManager 참조 갱신
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.RefreshPlayerReference();
        }
    }
    
    /// <summary>
    /// 플레이어를 스폰 위치로 이동
    /// </summary>
    private void MovePlayerToSpawnPosition()
    {
        if (persistentPlayer != null && hasSpawnPosition)
        {
            persistentPlayer.transform.position = nextSceneSpawnPosition;
            
            // 플레이어 컨트롤러 초기화
            PlayerController playerController = persistentPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ResetOnRespawn();
            }
            
            // 스폰 위치 리셋
            hasSpawnPosition = false;
            
            if (debugMode)
            {
                Debug.Log($"PlayerPersistenceManager: 플레이어를 {nextSceneSpawnPosition}로 이동시켰습니다.");
            }
        }
    }
    
    /// <summary>
    /// 씬 언로드 시 호출
    /// </summary>
    /// <param name="scene">언로드되는 씬</param>
    private void OnSceneUnloaded(Scene scene)
    {
        if (debugMode)
        {
            Debug.Log($"PlayerPersistenceManager: 씬 언로드 - {scene.name}");
        }
        
        // 플레이어 상태 저장
        if (PlayerStateManager.Instance != null && persistentPlayer != null)
        {
            PlayerStateManager.Instance.SavePlayerState(persistentPlayer);
        }
    }
    
    /// <summary>
    /// 현재 플레이어가 지속되고 있는지 확인
    /// </summary>
    /// <returns>지속 여부</returns>
    public bool IsPlayerPersistent()
    {
        return isPlayerPersistent && persistentPlayer != null;
    }
    
    /// <summary>
    /// 지속 중인 플레이어 오브젝트 반환
    /// </summary>
    /// <returns>플레이어 오브젝트</returns>
    public GameObject GetPersistentPlayer()
    {
        return persistentPlayer;
    }
    
    /// <summary>
    /// 강제로 플레이어 지속성 해제
    /// </summary>
    public void ReleasePersistence()
    {
        if (persistentPlayer != null)
        {
            // 씬 내 일반 오브젝트로 되돌리기
            SceneManager.MoveGameObjectToScene(persistentPlayer, SceneManager.GetActiveScene());
            persistentPlayer = null;
            isPlayerPersistent = false;
            
            if (debugMode)
            {
                Debug.Log("PlayerPersistenceManager: 플레이어 지속성 해제");
            }
        }
    }
}
