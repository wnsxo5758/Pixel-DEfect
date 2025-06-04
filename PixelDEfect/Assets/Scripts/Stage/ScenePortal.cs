using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("씬 전환 설정")]
    [SerializeField] private string targetSceneName; // 이동할 씬 이름
    [SerializeField] private Vector3 spawnPosition = Vector3.zero; // 다음 씬에서의 스폰 위치
    [SerializeField] private bool useSpawnPosition = true; // 스폰 위치 사용 여부
    
    [Header("전환 조건")]
    [SerializeField] private bool requireInteraction = false; // 상호작용 필요 여부
    [SerializeField] private bool automaticTransition = true; // 자동 전환 여부
    [SerializeField] private float transitionDelay = 0f; // 전환 전 지연 시간
    
    [Header("시각적 효과")]
    [SerializeField] private bool useFadeTransition = true; // 페이드 효과 사용
    [SerializeField] private GameObject interactionPrompt; // 상호작용 프롬프트 UI
    
    [Header("포털 제한")]
    [SerializeField] private bool oneTimeUse = false; // 한 번만 사용 가능
    [SerializeField] private bool requireKeyItem = false; // 키 아이템 필요
    [SerializeField] private string requiredKeyItemName = ""; // 필요한 키 아이템 이름
    
    [Header("디버그")]
    [SerializeField] private bool debugMode = false;
    
    private bool isPlayerInRange = false;
    private bool hasBeenUsed = false;
    private BoxCollider2D portalCollider;
    
    private void Awake()
    {
        portalCollider = GetComponent<BoxCollider2D>();
        if (portalCollider == null)
        {
            portalCollider = gameObject.AddComponent<BoxCollider2D>();
            portalCollider.isTrigger = true;
        }
        
        // 상호작용 프롬프트 초기 비활성화
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        targetSceneName = "Stage2";
    }
    
    private void Update()
    {
        // 상호작용 모드일 때 입력 처리
        if (requireInteraction && isPlayerInRange && !hasBeenUsed)
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E))
            {
                AttemptTransition();
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        isPlayerInRange = true;
        
        if (debugMode)
            Debug.Log("ScenePortal: 플레이어가 포털 범위에 진입");
        
        // 상호작용 프롬프트 표시
        if (requireInteraction && interactionPrompt != null && !hasBeenUsed)
        {
            interactionPrompt.SetActive(true);
        }
        
        // 자동 전환 모드일 때 즉시 전환 시도
        if (automaticTransition && !requireInteraction && !hasBeenUsed)
        {
            if (transitionDelay > 0)
            {
                Invoke(nameof(AttemptTransition), transitionDelay);
            }
            else
            {
                AttemptTransition();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        isPlayerInRange = false;
        
        if (debugMode)
            Debug.Log("ScenePortal: 플레이어가 포털 범위에서 이탈");
        
        // 상호작용 프롬프트 숨김
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // 지연된 전환 취소
        CancelInvoke(nameof(AttemptTransition));
    }
    
    /// <summary>
    /// 씬 전환 시도
    /// </summary>
    private void AttemptTransition()
    {
        // 이미 사용되었거나 전환 중이면 무시
        if (hasBeenUsed && oneTimeUse)
        {
            if (debugMode)
                Debug.LogWarning("ScenePortal: 이미 사용된 포털입니다.");
            return;
        }
        
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning())
        {
            if (debugMode)
                Debug.LogWarning("ScenePortal: 이미 씬 전환 중입니다.");
            return;
        }
        
        // 키 아이템 확인
        if (requireKeyItem && !CheckKeyItem())
        {
            if (debugMode)
                Debug.LogWarning($"ScenePortal: 키 아이템 '{requiredKeyItemName}'이 필요합니다.");
            return;
        }
        
        // 대상 씬 이름 확인
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("ScenePortal: 대상 씬 이름이 설정되지 않았습니다.");
            return;
        }
        
        // 플레이어 상태 저장
        SavePlayerState();
        
        // 씬 전환 실행
        ExecuteTransition();
        
        // 한 번만 사용 가능하면 사용됨 표시
        if (oneTimeUse)
        {
            hasBeenUsed = true;
            
            // 프롬프트 숨김
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 키 아이템 확인
    /// </summary>
    /// <returns>키 아이템 보유 여부</returns>
    private bool CheckKeyItem()
    {
        // TODO: 인벤토리 시스템이 있다면 여기서 키 아이템 확인
        // 현재는 항상 true 반환
        return true;
    }
    
    /// <summary>
    /// 플레이어 상태 저장
    /// </summary>
    private void SavePlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.SavePlayerState(player);
            
            if (debugMode)
                Debug.Log("ScenePortal: 플레이어 상태 저장 완료");
        }
    }
    
    /// <summary>
    /// 씬 전환 실행
    /// </summary>
    private void ExecuteTransition()
    {
        if (debugMode)
            Debug.Log($"ScenePortal: {targetSceneName}으로 전환 시작");
        
        if (SceneTransitionManager.Instance != null)
        {
            if (useSpawnPosition)
            {
                SceneTransitionManager.Instance.TransitionToScene(targetSceneName, spawnPosition, useFadeTransition);
            }
            else
            {
                SceneTransitionManager.Instance.TransitionToScene(targetSceneName, useFadeTransition);
            }
        }
        else
        {
            Debug.LogWarning("ScenePortal: SceneTransitionManager를 찾을 수 없습니다. 직접 씬 로드를 시도합니다.");
            
            // 백업 방법: PlayerPersistenceManager 사용
            if (PlayerPersistenceManager.Instance != null)
            {
                if (useSpawnPosition)
                {
                    PlayerPersistenceManager.Instance.TransitionToScene(targetSceneName, spawnPosition);
                }
                else
                {
                    PlayerPersistenceManager.Instance.TransitionToScene(targetSceneName);
                }
            }
            else
            {
                // 최후의 수단: 직접 씬 로드
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
        }
    }
    
    /// <summary>
    /// 포털 수동 활성화 (스크립트에서 호출용)
    /// </summary>
    public void ActivatePortal()
    {
        if (debugMode)
            Debug.Log("ScenePortal: 포털 수동 활성화");
        
        AttemptTransition();
    }
    
    /// <summary>
    /// 포털 설정 변경 (런타임에서 사용)
    /// </summary>
    /// <param name="sceneName">대상 씬 이름</param>
    /// <param name="spawn">스폰 위치</param>
    public void SetPortalDestination(string sceneName, Vector3 spawn)
    {
        targetSceneName = sceneName;
        spawnPosition = spawn;
        useSpawnPosition = true;
        
        if (debugMode)
            Debug.Log($"ScenePortal: 목적지 설정 - 씬: {sceneName}, 위치: {spawn}");
    }
    
    /// <summary>
    /// 포털 활성화/비활성화
    /// </summary>
    /// <param name="active">활성화 여부</param>
    public void SetPortalActive(bool active)
    {
        portalCollider.enabled = active;
        
        if (!active && interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        if (debugMode)
            Debug.Log($"ScenePortal: 포털 {(active ? "활성화" : "비활성화")}");
    }
    
    /// <summary>
    /// 포털 사용 가능 여부 확인
    /// </summary>
    /// <returns>사용 가능 여부</returns>
    public bool CanUsePortal()
    {
        if (oneTimeUse && hasBeenUsed) return false;
        if (requireKeyItem && !CheckKeyItem()) return false;
        if (string.IsNullOrEmpty(targetSceneName)) return false;
        
        return true;
    }
    
    private void OnDrawGizmosSelected()
    {
        // 포털 범위 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // 스폰 위치 표시 (useSpawnPosition이 true일 때)
        if (useSpawnPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPosition, 1f);
            Gizmos.DrawLine(transform.position, spawnPosition);
        }
    }
}
