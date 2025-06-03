using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    
    [Header("페이드 효과 설정")]
    [SerializeField] private GameObject fadeCanvasPrefab; // 페이드 효과용 캔버스 프리팹
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float transitionDelay = 0.5f; // 씬 전환 전 대기 시간
    
    [Header("디버그")]
    [SerializeField] private bool debugMode = false;
    
    private bool isTransitioning = false;
    private GameObject currentFadeCanvas;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 씬 전환 (플레이어 위치 지정)
    /// </summary>
    /// <param name="targetScene">대상 씬 이름</param>
    /// <param name="spawnPosition">플레이어 스폰 위치</param>
    /// <param name="useTransition">전환 효과 사용 여부</param>
    public void TransitionToScene(string targetScene, Vector3 spawnPosition, bool useTransition = true)
    {
        if (isTransitioning)
        {
            if (debugMode)
                Debug.LogWarning("SceneTransitionManager: 이미 씬 전환 중입니다.");
            return;
        }
        
        if (useTransition)
        {
            StartCoroutine(TransitionWithFade(targetScene, spawnPosition));
        }
        else
        {
            DirectTransition(targetScene, spawnPosition);
        }
    }
    
    /// <summary>
    /// 씬 전환 (스폰 위치 없음)
    /// </summary>
    /// <param name="targetScene">대상 씬 이름</param>
    /// <param name="useTransition">전환 효과 사용 여부</param>
    public void TransitionToScene(string targetScene, bool useTransition = true)
    {
        if (isTransitioning)
        {
            if (debugMode)
                Debug.LogWarning("SceneTransitionManager: 이미 씬 전환 중입니다.");
            return;
        }
        
        if (useTransition)
        {
            StartCoroutine(TransitionWithFade(targetScene));
        }
        else
        {
            DirectTransition(targetScene);
        }
    }
    
    /// <summary>
    /// 페이드 효과와 함께 씬 전환
    /// </summary>
    /// <param name="targetScene">대상 씬</param>
    /// <param name="spawnPosition">스폰 위치 (optional)</param>
    private IEnumerator TransitionWithFade(string targetScene, Vector3? spawnPosition = null)
    {
        isTransitioning = true;
        
        if (debugMode)
            Debug.Log($"SceneTransitionManager: {targetScene}으로 전환 시작");
        
        // 플레이어 입력 비활성화
        DisablePlayerInput();
        
        // 페이드 인 (화면을 어둡게)
        yield return StartCoroutine(FadeIn());
        
        // 전환 대기 시간
        yield return new WaitForSeconds(transitionDelay);
        
        // PlayerPersistenceManager에 스폰 위치 설정
        if (spawnPosition.HasValue && PlayerPersistenceManager.Instance != null)
        {
            PlayerPersistenceManager.Instance.SetNextSceneSpawnPosition(spawnPosition.Value);
        }
        
        // 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
        
        // 로딩 완료까지 대기
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 약간의 대기 (새 씬 초기화를 위해)
        yield return new WaitForSeconds(0.5f);
        
        // 페이드 아웃 (화면을 밝게)
        yield return StartCoroutine(FadeOut());
        
        // 플레이어 입력 활성화
        EnablePlayerInput();
        
        isTransitioning = false;
        
        if (debugMode)
            Debug.Log($"SceneTransitionManager: {targetScene} 전환 완료");
    }
    
    /// <summary>
    /// 즉시 씬 전환 (페이드 효과 없음)
    /// </summary>
    /// <param name="targetScene">대상 씬</param>
    /// <param name="spawnPosition">스폰 위치 (optional)</param>
    private void DirectTransition(string targetScene, Vector3? spawnPosition = null)
    {
        if (debugMode)
            Debug.Log($"SceneTransitionManager: {targetScene}으로 즉시 전환");
        
        // PlayerPersistenceManager에 스폰 위치 설정
        if (spawnPosition.HasValue && PlayerPersistenceManager.Instance != null)
        {
            PlayerPersistenceManager.Instance.TransitionToScene(targetScene, spawnPosition.Value);
        }
        else if (PlayerPersistenceManager.Instance != null)
        {
            PlayerPersistenceManager.Instance.TransitionToScene(targetScene);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }
    }
    
    /// <summary>
    /// 페이드 인 효과 (어둡게)
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (fadeCanvasPrefab != null)
        {
            currentFadeCanvas = Instantiate(fadeCanvasPrefab);
            DontDestroyOnLoad(currentFadeCanvas);
            
            CanvasGroup canvasGroup = currentFadeCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                
                float elapsedTime = 0f;
                while (elapsedTime < fadeInDuration)
                {
                    elapsedTime += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                    yield return null;
                }
                
                canvasGroup.alpha = 1f;
            }
        }
        else
        {
            yield return new WaitForSeconds(fadeInDuration);
        }
    }
    
    /// <summary>
    /// 페이드 아웃 효과 (밝게)
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (currentFadeCanvas != null)
        {
            CanvasGroup canvasGroup = currentFadeCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                
                float elapsedTime = 0f;
                while (elapsedTime < fadeOutDuration)
                {
                    elapsedTime += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeOutDuration));
                    yield return null;
                }
                
                canvasGroup.alpha = 0f;
            }
            
            Destroy(currentFadeCanvas);
            currentFadeCanvas = null;
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
    }
    
    /// <summary>
    /// 플레이어 입력 비활성화
    /// </summary>
    private void DisablePlayerInput()
    {
        GameObject player = null;
        
        // 지속성 플레이어가 있으면 그것을 사용
        if (PlayerPersistenceManager.Instance != null)
        {
            player = PlayerPersistenceManager.Instance.GetPersistentPlayer();
        }
        
        // 없으면 씬에서 찾기
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// 플레이어 입력 활성화
    /// </summary>
    private void EnablePlayerInput()
    {
        GameObject player = null;
        
        // 지속성 플레이어가 있으면 그것을 사용
        if (PlayerPersistenceManager.Instance != null)
        {
            player = PlayerPersistenceManager.Instance.GetPersistentPlayer();
        }
        
        // 없으면 씬에서 찾기
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
    
    /// <summary>
    /// 게임 재시작 (타이틀로 돌아가기)
    /// </summary>
    /// <param name="titleSceneName">타이틀 씬 이름</param>
    public void RestartGame(string titleSceneName = "TitleScene")
    {
        if (debugMode)
            Debug.Log("SceneTransitionManager: 게임 재시작");
        
        // 모든 지속성 객체 제거
        if (PlayerPersistenceManager.Instance != null)
        {
            PlayerPersistenceManager.Instance.DestroyPersistentObjects();
        }
        
        // 타이틀 씬으로 이동
        SceneManager.LoadScene(titleSceneName);
    }
    
    /// <summary>
    /// 게임 종료 (엔드 씬으로)
    /// </summary>
    /// <param name="endSceneName">엔드 씬 이름</param>
    public void EndGame(string endSceneName = "EndScene")
    {
        if (debugMode)
            Debug.Log("SceneTransitionManager: 게임 종료");
        
        StartCoroutine(EndGameSequence(endSceneName));
    }
    
    /// <summary>
    /// 게임 종료 시퀀스
    /// </summary>
    /// <param name="endSceneName">엔드 씬 이름</param>
    private IEnumerator EndGameSequence(string endSceneName)
    {
        // 페이드 인
        yield return StartCoroutine(FadeIn());
        
        // 플레이어 상태 저장
        if (PlayerStateManager.Instance != null)
        {
            GameObject player = PlayerPersistenceManager.Instance?.GetPersistentPlayer();
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                PlayerStateManager.Instance.SavePlayerState(player);
            }
        }
        
        // 모든 지속성 객체 제거
        if (PlayerPersistenceManager.Instance != null)
        {
            PlayerPersistenceManager.Instance.DestroyPersistentObjects();
        }
        
        // 엔드 씬으로 이동
        SceneManager.LoadScene(endSceneName);
        
        // 페이드 아웃은 엔드 씬에서 처리
    }
    
    /// <summary>
    /// 현재 전환 중인지 확인
    /// </summary>
    /// <returns>전환 중 여부</returns>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}
