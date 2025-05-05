using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("시간 정지 설정")] 
    [SerializeField] private float timeStopDuration = 4f;
    [SerializeField] private float timeScale = 0f;

    [Header("이펙트 설정")] 
    [SerializeField] private GameObject timeStopEffectPrefab;
    [SerializeField] private GameObject screenFilterObject;

    // 이벤트 콜백
    public UnityEvent onTimeStop = new UnityEvent();
    public UnityEvent onTimeResume = new UnityEvent();

    private bool isTimeStopped = false;
    private Coroutine timeStopCoroutine;
    private List<Rigidbody2D> frozenRigidbodies = new List<Rigidbody2D>();
    private Dictionary<Rigidbody2D, Vector2> savedVelocities = new Dictionary<Rigidbody2D, Vector2>();
    private Dictionary<Rigidbody2D, float> savedAngularVelocities = new Dictionary<Rigidbody2D, float>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // 화면 필터 초기화
        if (screenFilterObject != null)
        {
            screenFilterObject.SetActive(false);
        }
    }

    public void StopTime()
    {
        if (isTimeStopped) return;

        isTimeStopped = true;

        // 기존 코루틴 종료
        if (timeStopCoroutine != null)
        {
            StopCoroutine(timeStopCoroutine);
        }
        
        // 새 코루틴 시작
        timeStopCoroutine = StartCoroutine(TimeStopRoutine());
        
        // 이벤트 호출
        onTimeStop.Invoke();
        
        // 시간 정지 사운드 재생
    }

    public void ResumeTime()
    {
        if (!isTimeStopped) return;

        isTimeStopped = false;
        
        // 코루틴 종료
        if (timeStopCoroutine != null)
        {
            StopCoroutine(timeStopCoroutine);
            timeStopCoroutine = null;
        }
        
        // 시간 스케일 복원
        Time.timeScale = 1f;
        
        // 저장된 물리 상태 복원
        RestorePhysics();
        
        // 이펙트 제거
        DisableTimeStopEffects();
        
        // 이벤트 호출
        onTimeResume.Invoke();
        
        // 시간 복원 사운드 재생
    }

    private IEnumerator TimeStopRoutine()
    {
        // 물리 객체 상태 저장
        FreezePhysics();
        
        // 시간 정지 이펙트 활성화
        EnableTimeStopEffects();
        
        // 시간 정지
        Time.timeScale = timeScale;
        
        // 정지 시간만큼 대기
        float elapsedRealTime = 0f;
        while (elapsedRealTime < timeStopDuration)
        {
            elapsedRealTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // 시간 복원
        ResumeTime();
    }
    
    // 모든 물리 객체 정지
    private void FreezePhysics()
    {
        // 모든 Rigidbody2D 찾기
        Rigidbody2D[] allRigidbodies = FindObjectsOfType<Rigidbody2D>();

        frozenRigidbodies.Clear();
        savedVelocities.Clear();
        savedAngularVelocities.Clear();

        foreach (Rigidbody2D rb in allRigidbodies)
        {
            // 플레이어 제외
            if (rb.CompareTag("Player")) continue;
            
            // 현재 상태 저장
            savedVelocities[rb] = rb.velocity;
            savedAngularVelocities[rb] = rb.angularVelocity;
            
            // 모든 움직임 정지
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.Sleep();
            
            // 리스트에 추가
            frozenRigidbodies.Add(rb);
        }
        
        // 모든 적 애니메이션 정지
        foreach (var enemy in FindObjectsOfType<EnemyBT>())
        {
            enemy.PauseEnemy(true);
        }
    }
    
    // 물리 객체 상태 복원
    private void RestorePhysics()
    {
        foreach (Rigidbody2D rb in frozenRigidbodies)
        {
            if (rb == null) continue;
            
            // 속도 복원
            if (savedVelocities.ContainsKey(rb))
            {
                rb.velocity = savedVelocities[rb];
            }
            
            // 각속도 복원
            if (savedAngularVelocities.ContainsKey(rb))
            {
                rb.angularVelocity = savedAngularVelocities[rb];
            }
            
            rb.WakeUp();
        }
        
        // 모든 적 애니메이션 재개
        foreach (var enemy in FindObjectsOfType<EnemyBT>())
        {
            enemy.PauseEnemy(false);
        }
        
        frozenRigidbodies.Clear();
        savedVelocities.Clear();
        savedAngularVelocities.Clear();
    }
    
    // 시간 정지 이펙트 활성화
    private void EnableTimeStopEffects()
    {
        
    }
    
    // 시간 정지 이펙트 비활성화
    private void DisableTimeStopEffects()
    {
        
    }

    public bool IsTimeStopped()
    {
        return isTimeStopped;
    }
}
    
