using System.Collections;
using UnityEngine;

/// <summary>
/// 씬 전환 포털
/// 플레이어가 트리거하면 지정된 씬으로 전환
/// </summary>
public class ScenePortal : MonoBehaviour
{
    [Header("포털 설정")]
    [SerializeField] private string targetScene = "Stage2";
    [SerializeField] private Vector3 spawnPos = Vector3.zero;
    [SerializeField] private bool useSpawnPos = true;

    [Header("전환 조건")]
    [SerializeField] private bool requireInteraction = false; // 상호작용 필요 여부
    [SerializeField] private bool autoTransition = true;      // 자동 전환 여부
    [SerializeField] private float transitionDelay = 0f;      // 전환 전 지연 시간

    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;

    [Header("포털 제한")]
    [SerializeField] private bool oneTimeUse = false; // 한 번만 사용 가능

    [Header("디버그")]
    [SerializeField] private bool debugMode = false;

    private bool isPlayerInRange = false;
    private bool hasBeenUsed = false;
    private BoxCollider2D portalCollider;

    #region Unity Lifecycle

    private void Awake()
    {
        // 콜라이더 설정
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
    }

    private void Update()
    {
        // 상호작용 모드일 때 입력 체크
        if (requireInteraction && isPlayerInRange && !hasBeenUsed)
        {
            // E키 또는 상호작용 키 입력 시 전환
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
            {
                TriggerTransition();
            }
        }
    }

    #endregion

    #region Trigger Events

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = true;

        if (debugMode)
        {
            Debug.Log($"[ScenePortal] 플레이어 포털 진입 - 포털 위치: {transform.position}");
        }

        // 상호작용 프롬프트 표시
        if (requireInteraction && interactionPrompt != null && !hasBeenUsed)
        {
            interactionPrompt.SetActive(true);
        }

        // 자동 전환 모드일 때 즉시 전환 시도
        if (autoTransition && !requireInteraction && !hasBeenUsed)
        {
            if (transitionDelay > 0)
            {
                if (debugMode)
                {
                    Debug.Log($"[ScenePortal] {transitionDelay}초 후 전환 시작");
                }
                StartCoroutine(TriggerTransitionDelayed());
            }
            else
            {
                TriggerTransition();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = false;

        // 상호작용 프롬프트 숨김
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // 지연된 전환 취소
        StopAllCoroutines();
    }

    #endregion

    #region Transition

    /// <summary>
    /// 씬 전환 트리거
    /// </summary>
    private void TriggerTransition()
    {
        if (debugMode)
        {
            Debug.Log($"[ScenePortal] ===== 씬 전환 트리거 =====");
            Debug.Log($"[ScenePortal] 현재 씬: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            Debug.Log($"[ScenePortal] 목표 씬: {targetScene}");
            Debug.Log($"[ScenePortal] 스폰 위치: {(useSpawnPos ? spawnPos.ToString() : "설정 안됨")}");
        }

        // 이미 사용되었거나 전환 중이면 무시
        if (hasBeenUsed && oneTimeUse)
        {
            if (debugMode) Debug.Log($"[ScenePortal] 이미 사용된 일회용 포털 - 무시");
            return;
        }
        if (SceneSystem.Instance == null)
        {
            Debug.LogError($"[ScenePortal] SceneSystem.Instance가 null입니다!");
            return;
        }
        if (SceneSystem.Instance.IsTransitioning())
        {
            if (debugMode) Debug.Log($"[ScenePortal] 이미 씬 전환 중 - 무시");
            return;
        }

        // 플레이어 입력 비활성화
        if (PlayerPersistenceSystem.Instance != null)
        {
            PlayerPersistenceSystem.Instance.DisablePlayerInput();
            if (debugMode) Debug.Log($"[ScenePortal] 플레이어 입력 비활성화 완료");
        }

        // 스폰 위치 설정
        if (useSpawnPos && PlayerPersistenceSystem.Instance != null)
        {
            PlayerPersistenceSystem.Instance.SetNextSpawnPosition(spawnPos);
            if (debugMode) Debug.Log($"[ScenePortal] 스폰 위치 설정 완료: {spawnPos}");
        }

        // 씬 전환
        if (debugMode) Debug.Log($"[ScenePortal] SceneSystem.LoadScene(\"{targetScene}\") 호출");
        SceneSystem.Instance.LoadScene(targetScene);

        // 일회용 포털이면 사용 완료 표시
        if (oneTimeUse)
        {
            hasBeenUsed = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            if (debugMode) Debug.Log($"[ScenePortal] 일회용 포털 사용 완료");
        }
    }

    /// <summary>
    /// 지연된 씬 전환
    /// </summary>
    private IEnumerator TriggerTransitionDelayed()
    {
        yield return new WaitForSeconds(transitionDelay);
        TriggerTransition();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 포털 목적지 설정
    /// </summary>
    public void SetDestination(string scene, Vector3 spawn)
    {
        targetScene = scene;
        spawnPos = spawn;
        useSpawnPos = true;
    }

    /// <summary>
    /// 포털 활성화/비활성화
    /// </summary>
    public void SetPortalActive(bool active)
    {
        portalCollider.enabled = active;
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        // 포털 위치 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, transform.localScale);

        // 스폰 위치 표시
        if (useSpawnPos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPos, 0.5f);
            Gizmos.DrawLine(transform.position, spawnPos);
        }
    }

    #endregion
}
