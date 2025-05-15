using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("시간 정지 설정")] 
    [SerializeField] private float timeFreezeRadius = 20f;
    [SerializeField] private float timeFreezeDuration = 5f;
    [SerializeField] private float timeFreezeCooldown = 15f;
    [SerializeField] private LayerMask timeAffectedLayers;
    [SerializeField] private bool hasTimeStopAbility = false;
    
    [Header("시각 효과")] 
    [SerializeField] private GameObject timeFreezeVFXPrefab;
    [SerializeField] private GameObject screenOverlayPrefab;
    
    // 상태 변수
    private bool isTimeFrozen = false;
    private float timeFreezeTimer = 0f;
    private float cooldownTimer = 0f;
    private List<ITimeAffected> affectedEntities = new List<ITimeAffected>();
    private GameObject currentVFX;
    private GameObject currentOverlay;
    
    
    // 이벤트
    public System.Action OnTimeStopBegin;
    public System.Action OnTimeStopEnd;
    public System.Action<float> OnCooldownUpdate; // 쿨다운 업데이트 0~1
    
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
    }

    private void Update()
    {
        // 쿨다운 업데이트
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            OnCooldownUpdate?.Invoke(1 - (cooldownTimer / timeFreezeCooldown));
        }
        
        // 시간 정지 타이머 업데이트
        if (isTimeFrozen)
        {
            timeFreezeTimer -= Time.deltaTime;
            if (timeFreezeTimer <= 0)
            {
                ResumeTime();
            }
        }
    }
    
    // 시간 정지 스킬 획득
    public void UnlockTimeStopAbility()
    {
        hasTimeStopAbility = true;
    }
    
    // 플레이어의 회피 성공 시 호출될 메서드
    public void TriggerTimeStopOnDodge(Vector3 position)
    {
        if (!hasTimeStopAbility || isTimeFrozen || cooldownTimer > 0)
            return;

        FreezeTime(position);
    }
    
    // 시간 정지 실행
    public void FreezeTime(Vector3 originPosition)
    {
        if (isTimeFrozen || cooldownTimer > 0)
            return;

        isTimeFrozen = true;
        timeFreezeTimer = timeFreezeDuration;
        
        // 영향 범위 내 모든 개체 찾기
        FindAndRegisterTimeAffectedEntities(originPosition);
        
        // 시각 효과 생성
        CreateVisualEffects(originPosition);
        
        // 이벤트 호출
        OnTimeStopBegin?.Invoke();
    }
    
    // 시간 재개
    public void ResumeTime()
    {
        if (!isTimeFrozen)
            return;

        isTimeFrozen = false;
        cooldownTimer = timeFreezeCooldown;
        
        // 모든 개체 시간 재개
        foreach (var entity in affectedEntities)
        {
            entity.OnTimeResume();
        }
        
        // 시각 효과 제거
        RemoveVisualEffects();
        
        // 목록 초기화
        affectedEntities.Clear();
        
        // 이벤트 호출
        OnTimeStopEnd?.Invoke();
    }
    
    // 시간 정지 중 데미지 적용
    public void ApplyDamageInFrozenTime(ITimeAffected target, int damage, Vector2 direction)
    {
        if (!isTimeFrozen || !affectedEntities.Contains(target))
            return;
        
        target.ReceiveDamageInFrozenTime(damage, direction);
    }
    
    // 영향 받는 개체 찾기 및 등록
    private void FindAndRegisterTimeAffectedEntities(Vector3 originPosition)
    {
        affectedEntities.Clear();
        
        // 레이어 마스크를 이용한 효율적인 개체 검색
        Collider2D[] colliders = 
            Physics2D.OverlapCircleAll(originPosition, timeFreezeRadius, timeAffectedLayers);

        foreach (var collider in colliders)
        {
            // ITimeAffected 인터페이스를 구현한 컴포넌트 검색
            ITimeAffected[] entities = collider.GetComponents<ITimeAffected>();

            foreach (var entity in entities)
            {
                if (entity.IsInTimeFreezeRange(originPosition, timeFreezeRadius))
                {
                    affectedEntities.Add(entity);
                    entity.OnTimeStop();
                }
            }
        }
    }
    
    // 시각 효과 생성
    private void CreateVisualEffects(Vector3 position)
    {
        
    }
    
    // 시각 효과 제거
    private void RemoveVisualEffects()
    {
        
    }
    
    // 상태 확인 메서드들
    public bool IsTimeFrozen() => isTimeFrozen;
    public bool HasTimeStopAbility() => hasTimeStopAbility;
    public float GetCooldownPercentage() => cooldownTimer > 0 ? cooldownTimer / timeFreezeCooldown : 0f;
    public float GetRemainingFreezeDuration() => timeFreezeTimer;
}
    
