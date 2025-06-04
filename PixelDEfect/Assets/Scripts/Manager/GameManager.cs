using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private float respawnInvulnerabilityTime = 2f;
    
    [Header("씬 관리")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private string endSceneName = "EndScene";
    [SerializeField] private bool destroyOnTitleScene = true;
    [SerializeField] private bool destroyOnEndScene = true;
    
    [Header("투비 컨티뉴")]
    [SerializeField]
    private GameObject toBeCon;
    private GameObject player;
    
    [Header("디버그")]
    [SerializeField] private bool debugMode = false;
    
    private bool isPlayerDead = false;
    private bool isInitialized = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (ShouldDestroyOnScene(currentSceneName))
            {
                // 타이틀이나 엔드 씬에서는 DontDestroyOnLoad 적용하지 않음
                if (debugMode)
                    Debug.Log($"GameManager: {currentSceneName}에서는 지속성을 적용하지 않습니다.");
            }
            else
            {
                // DontDestroyOnLoad(gameObject);
                if (debugMode)
                    Debug.Log("GameManager: DontDestroyOnLoad 적용");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void ProcessPlayerFall(int fallDamage, bool water)
    {
        if (player == null) return;
        
        StartCoroutine(FallRespawnProcess(fallDamage, water));
    }

    private IEnumerator FallRespawnProcess(int fallDamage, bool water)
    {
        PlayerHp playerHp = player.GetComponent<PlayerHp>();
        if (playerHp != null)
        {
            playerHp.TakeFallDamage(fallDamage, water);

            if (playerHp.GetCurrentHp() <= 0)
            {
                yield break;
            }
        }

        // 낙사 체크포인트로 이동
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.MoveToFallbackCheckpoint(player);
        }
    }
    
    public void PlayerDied()
    {
        isPlayerDead = true;
        DisableTimeEvents();
    }

    // 타임 매니저 이벤트 중지
    private void DisableTimeEvents()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.IsTimeFrozen())
        {
            TimeManager.Instance.ResumeTime();
        }
    }
    
    // 수동 부활 메서드
    public void RespawnPlayer()
    {
        if (!isPlayerDead) return;

        StartCoroutine(ManualRespawnProcess());
    }

    private IEnumerator ManualRespawnProcess()
    {
        // 딜레이 
        yield return new WaitForSeconds(respawnDelay);
        
        // 체크포인트 매니저가 있는지 확인
        if (CheckpointManager.Instance != null && player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.ResetOnRespawn();
            }
            
            CheckpointManager.Instance.RespawnAtCheckpoint(player);
            
            isPlayerDead = false;
        }
        else
        {
            RestartGame();
        }
    }
    
    public void RestartGame()
    {
        StartCoroutine(RestartTimer());
    }

    private IEnumerator RestartTimer()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        
        yield return new WaitForSeconds(respawnDelay);
        
        CheckpointManager.Instance.ResetCheckpoints();
        SceneManager.LoadScene(currentScene.name);
    }
    
    /// <summary>
    /// 지연된 초기화
    /// </summary>
    private IEnumerator InitializeDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        Initialize();
    }
    
    /// <summary>
    /// GameManager 초기화
    /// </summary>
    private void Initialize()
    {
        if (isInitialized) return;
        
        FindPlayer();
        
        // PlayerPersistenceManager가 없으면 생성
        if (PlayerPersistenceManager.Instance == null)
        {
            GameObject persistenceManagerObj = new GameObject("PlayerPersistenceManager");
            persistenceManagerObj.AddComponent<PlayerPersistenceManager>();
            
            if (debugMode)
                Debug.Log("GameManager: PlayerPersistenceManager 생성");
        }
        
        // SceneTransitionManager가 없으면 생성
        if (SceneTransitionManager.Instance == null)
        {
            GameObject transitionManagerObj = new GameObject("SceneTransitionManager");
            transitionManagerObj.AddComponent<SceneTransitionManager>();
            
            if (debugMode)
                Debug.Log("GameManager: SceneTransitionManager 생성");
        }
        
        isInitialized = true;
        
        if (debugMode)
            Debug.Log("GameManager: 초기화 완료");
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (debugMode)
            Debug.Log($"GameManager: 씬 로드됨 - {scene.name}");
        
        // 특정 씬에서는 GameManager 제거
        if (ShouldDestroyOnScene(scene.name))
        {
            if (debugMode)
                Debug.Log($"GameManager: {scene.name}에서 제거됨");
            
            Destroy(gameObject);
            return;
        }
        
        StartCoroutine(OnSceneLoadedDelayed(scene));
    }
    
    private IEnumerator OnSceneLoadedDelayed(Scene scene)
    {
        yield return new WaitForSeconds(0.1f);
        
        // 플레이어 재검색
        FindPlayer();
        
        // PlayerPersistenceManager에 플레이어 설정
        if (player != null && PlayerPersistenceManager.Instance != null)
        {
            PlayerPersistenceManager.Instance.SetupPlayerPersistence(player);
        }
        
        isPlayerDead = false;
    }
    
    /// <summary>
    /// 플레이어 찾기
    /// </summary>
    private void FindPlayer()
    {
        // 지속성 플레이어 우선 확인
        if (PlayerPersistenceManager.Instance != null)
        {
            GameObject persistentPlayer = PlayerPersistenceManager.Instance.GetPersistentPlayer();
            if (persistentPlayer != null)
            {
                player = persistentPlayer;
                if (debugMode)
                    Debug.Log("GameManager: 지속성 플레이어 찾음");
                return;
            }
        }
        
        // 씬에서 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (debugMode)
                Debug.Log("GameManager: 씬에서 플레이어 찾음");
        }
        else
        {
            if (debugMode)
                Debug.LogWarning("GameManager: 플레이어를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 특정 씬에서 GameManager를 제거해야 하는지 확인
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    /// <returns>제거 여부</returns>
    private bool ShouldDestroyOnScene(string sceneName)
    {
        if (destroyOnTitleScene && sceneName.Equals(titleSceneName, System.StringComparison.OrdinalIgnoreCase))
            return true;
        
        if (destroyOnEndScene && sceneName.Equals(endSceneName, System.StringComparison.OrdinalIgnoreCase))
            return true;
        
        return false;
    }
    
    /// <summary>
    /// 게임 종료 (엔드 씬으로)
    /// </summary>
    public void EndGame()
    {
        if (debugMode)
            Debug.Log("GameManager: 게임 종료");
        
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.EndGame(endSceneName);
        }
        else
        {
            // 백업: 직접 엔드 씬으로 이동
            if (PlayerPersistenceManager.Instance != null)
            {
                PlayerPersistenceManager.Instance.DestroyPersistentObjects();
            }
            
            SceneManager.LoadScene(endSceneName);
        }
    }
    
    /// <summary>
    /// 타이틀로 돌아가기
    /// </summary>
    public void ReturnToTitle()
    {
        if (debugMode)
            Debug.Log("GameManager: 타이틀로 돌아가기");
        
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.RestartGame(titleSceneName);
        }
        else
        {
            // 백업: 직접 타이틀 씬으로 이동
            if (PlayerPersistenceManager.Instance != null)
            {
                PlayerPersistenceManager.Instance.DestroyPersistentObjects();
            }
            
            SceneManager.LoadScene(titleSceneName);
        }
    }
    
    public void ToBe()
    {
        toBeCon.SetActive(true);
    }
}
