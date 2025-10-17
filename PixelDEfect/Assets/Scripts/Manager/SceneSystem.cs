using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환을 전담하는 싱글톤 시스템
/// 책임: 씬 로드/언로드, 페이드 효과, 전환 이벤트 발행
/// </summary>
public class SceneSystem : Singleton<SceneSystem>
{
    [Header("씬 이름 설정")]
    [SerializeField] private string titleScene = "Title";
    [SerializeField] private string stage1Scene = "Stage1";
    [SerializeField] private string stage2Scene = "Stage2";
    [SerializeField] private string endScene = "EndScene";

    [Header("로딩 UI 설정")]
    [SerializeField] private float loadingDuration = 1f;

    // 상태
    private bool isTransitioning = false;
    private string currentSceneName;

    // 이벤트
    public event Action<string> OnSceneLoadStart;      // 씬 로드 시작 (씬 이름)
    public event Action<string> OnSceneLoadComplete;   // 씬 로드 완료 (씬 이름)
    public event Action OnTransitionStart;             // 전환 시작
    public event Action OnTransitionComplete;          // 전환 완료

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake(); // Singleton<T>의 Awake 호출 (중복 제거 + DontDestroyOnLoad)

        // Unity 씬 이벤트 구독
        SceneManager.sceneLoaded += OnUnitySceneLoaded;

        // 현재 씬 저장
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy(); // Singleton<T>의 OnDestroy 호출 (인스턴스 정리)

        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnUnitySceneLoaded;
    }

    #endregion

    #region Public Methods - Scene Loading

    /// <summary>
    /// 씬 로드
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("SceneSystem: 이미 씬 전환 중입니다.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneSystem: 씬 이름이 비어있습니다.");
            return;
        }

        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    /// <summary>
    /// 타이틀 씬으로 이동
    /// </summary>
    public void LoadTitleScene()
    {
        LoadScene(titleScene);
    }

    /// <summary>
    /// Stage1로 이동
    /// </summary>
    public void LoadStage1()
    {
        LoadScene(stage1Scene);
    }

    /// <summary>
    /// Stage2로 이동
    /// </summary>
    public void LoadStage2()
    {
        LoadScene(stage2Scene);
    }

    /// <summary>
    /// 엔딩 씬으로 이동
    /// </summary>
    public void LoadEndScene()
    {
        LoadScene(endScene);
    }

    /// <summary>
    /// 현재 씬 재시작
    /// </summary>
    public void RestartCurrentScene()
    {
        LoadScene(currentSceneName);
    }

    /// <summary>
    /// 전환 중인지 확인
    /// </summary>
    public bool IsTransitioning() => isTransitioning;

    /// <summary>
    /// 현재 씬 이름 반환
    /// </summary>
    public string GetCurrentSceneName() => currentSceneName;

    #endregion

    #region Private Methods - Scene Loading

    /// <summary>
    /// 씬 로드 코루틴
    /// </summary>
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        isTransitioning = true;
        OnTransitionStart?.Invoke();
        OnSceneLoadStart?.Invoke(sceneName);

        // Stage2 → EndScene은 로딩 없음
        bool useLoading = !(currentSceneName == stage2Scene && sceneName == endScene);

        if (useLoading)
        {
            // LoadingUI 활성화
            GameObject loadingUI = FindLoadingUI();
            if (loadingUI != null)
            {
                loadingUI.SetActive(true);
            }

            // 1초 대기
            yield return new WaitForSeconds(loadingDuration);
        }

        // 씬 비동기 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 로드 완료 대기
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 전환 완료
        isTransitioning = false;
        OnTransitionComplete?.Invoke();
    }

    /// <summary>
    /// Unity 씬 로드 완료 콜백
    /// </summary>
    private void OnUnitySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        OnSceneLoadComplete?.Invoke(scene.name);
    }

    /// <summary>
    /// Canvas의 자식에서 LoadingUI 찾기
    /// </summary>
    private GameObject FindLoadingUI()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in canvases)
        {
            Transform loadingUITransform = canvas.transform.Find("LoadingUI");
            if (loadingUITransform != null)
            {
                return loadingUITransform.gameObject;
            }
        }

        return null;
    }

    #endregion

    #region Scene Type Checks

    /// <summary>
    /// 게임 플레이 씬인지 확인 (Stage1, Stage2)
    /// </summary>
    public bool IsGameplayScene(string sceneName)
    {
        return sceneName == stage1Scene || sceneName == stage2Scene;
    }

    /// <summary>
    /// 타이틀 씬인지 확인
    /// </summary>
    public bool IsTitleScene(string sceneName)
    {
        return sceneName == titleScene;
    }

    /// <summary>
    /// 엔딩 씬인지 확인
    /// </summary>
    public bool IsEndScene(string sceneName)
    {
        return sceneName == endScene;
    }

    /// <summary>
    /// 플레이어가 필요한 씬인지 확인
    /// </summary>
    public bool IsPlayerRequired(string sceneName)
    {
        return IsGameplayScene(sceneName);
    }

    #endregion
}
