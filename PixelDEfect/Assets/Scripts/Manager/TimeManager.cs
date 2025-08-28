using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{

    [SerializeField]
    private SkillCoolUI skillCoolUI;
    [Header("시간 정지 설정")] 
    [SerializeField] private float timeFreezeRadius = 20f;
    [SerializeField] private float timeFreezeDuration = 5f;
    [SerializeField] private float timeFreezeCooldown = 15f;
    [SerializeField] private LayerMask timeAffectedLayers;

    [Header("시간 정지 이펙트 설정")] 
    [SerializeField] private float effectExpandDuration = 2f;
    [SerializeField] private float effectShrinkDuration = 2f;
    [SerializeField] private float maxEffectScale = 10f;
    [SerializeField] private AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    // 상태 변수
    private bool isTimeFrozen = false;
    private bool effectInProgress = false;
    private float timeFreezeTimer = 0f;
    private float cooldownTimer = 0f;
    private List<ITimeAffected> affectedEntities = new List<ITimeAffected>();
    
    // 시간 정지 이펙트 변수
    private Transform playerTransform;
    private GameObject timeEffectFrame;
    private GameObject timeEffectMaterial;
    private Vector3 originalEffectScale;
    private Coroutine effectCoroutine;
    
    // 이벤트
    public System.Action OnTimeStopBegin;
    public System.Action OnTimeStopEnd;
    public System.Action<float> OnCooldownUpdate; // 쿨다운 업데이트 0~1

    public float TimeFreezeCoolDown => timeFreezeCooldown;


    protected override void Awake()
    {
        base.Awake();
        FindPlayerAndEffects();
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
        if (isTimeFrozen && !effectInProgress)
        {
            timeFreezeTimer -= Time.deltaTime;
            if (timeFreezeTimer <= 0)
            {
                ResumeTime();
            }
        }
    }
    
    // 플레이어의 회피 성공 시 호출될 메서드
    public void TriggerTimeStopOnDodge(Vector3 position)
    {
        if (SkillManager.Instance == null || !SkillManager.Instance.HasSkill(SkillType.TimeStop))
        {
            Debug.Log("시간정지 스킬을 보유하고 있지 않습니다.");
            return;
        }

        if (!isTimeFrozen && cooldownTimer <= 0)
        {
            FreezeTime(position);
        }
    }
    
    // 시간 정지 실행
    public void FreezeTime(Vector3 originPosition)
    {
        if (isTimeFrozen || cooldownTimer > 0)
            return;

        isTimeFrozen = true;
        timeFreezeTimer = timeFreezeDuration;
        skillCoolUI.StartCooldown();
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

        effectInProgress = true;
        
        // 시각 효과 제거
        RemoveVisualEffects();
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
        // 플레이어나 이펙트 오브젝트가 없으면 반환
        if (playerTransform == null || timeEffectFrame == null || timeEffectMaterial == null)
        {
            Debug.LogWarning("TimeManager: 시간 정지 이펙트를 위한 오브젝트가 설정되지 않았습니다.");
            return;
        }
        
        // 이펙트 활성화
        timeEffectFrame.SetActive(true);
        
        // 이벤트 확장 코루틴 시작
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }

        effectCoroutine = StartCoroutine(ExpandEffect());
    }
    
    // 시각 효과 제거
    private void RemoveVisualEffects()
    {
        if (timeEffectFrame == null || timeEffectMaterial == null)
        {
            CompleteTimeResume();
            return;
        }
        
        // 이펙트 축소 코루틴 시작
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }

        effectCoroutine = StartCoroutine(ShrinkEffect());
    }

    // 이펙트 확장 코루틴
    private IEnumerator ExpandEffect()
    {
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = originalEffectScale * maxEffectScale;
        
        // 시작 스케일을 0으로 설정
        timeEffectMaterial.transform.localScale = startScale;
        
        while (elapsedTime < effectExpandDuration)
        {
            elapsedTime += Time.deltaTime;
            
            float progress = elapsedTime / effectExpandDuration;
            float curveValue = expandCurve.Evaluate(progress);
            
            Vector3 currentScale = Vector3.Lerp(startScale, targetScale, curveValue);
            timeEffectMaterial.transform.localScale = currentScale;

            yield return null;
        }
        
        // 최종 스케일 설정
        timeEffectMaterial.transform.localScale = targetScale;
        effectCoroutine = null;
    }

    private void CompleteTimeResume()
    {
        isTimeFrozen = false;
        cooldownTimer = timeFreezeCooldown;
        effectInProgress = false;
        
        // 모든 개체 시간 재개
        foreach (var entity in affectedEntities)
        {
            entity.OnTimeResume();
        }
        
        // 목록 초기화
        affectedEntities.Clear();
        
        // 이벤트 호출
        OnTimeStopEnd?.Invoke();
    }
    
    // 이펙트 축소 코루틴
    private IEnumerator ShrinkEffect()
    {
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = timeEffectMaterial.transform.localScale;

        while (elapsedTime < effectShrinkDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / effectShrinkDuration;
            float curveValue = shrinkCurve.Evaluate(progress);
            
            Vector3 currentScale = Vector3.Lerp(startScale, targetScale, curveValue);
            timeEffectMaterial.transform.localScale = currentScale;

            yield return null;
        }
        
        // 최종 스케일 설정 및 비활성화
        timeEffectMaterial.transform.localScale = targetScale;
        timeEffectFrame.SetActive(false);
        
        // 원래 스케일로 복원
        timeEffectMaterial.transform.localScale = originalEffectScale;
        
        effectCoroutine = null;
        
        CompleteTimeResume();
    }
    
    // 플레이어와 이펙트 오브젝트 찾기
    private void FindPlayerAndEffects()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;

            Transform frameTransform = playerTransform.Find("Frame");
            if (frameTransform != null)
            {
                timeEffectFrame = frameTransform.gameObject;
                
                // Material 오브젝트 찾기
                Transform materialTransform = frameTransform.Find("Material");
                if (materialTransform != null)
                {
                    timeEffectMaterial = materialTransform.gameObject;
                    originalEffectScale = timeEffectMaterial.transform.localScale;
                    
                    // 초기에는 비활성화
                    timeEffectFrame.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("TimeManager: Frame 하위에 Material 오브젝트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("TimeManager: Player 하위에 Frame 오브젝트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("TimeManager: Player 오브젝트를 찾을 수 없습니다.");
        }
    }
    
    // 씬 변경 시 플레이어와 이펙트 재설정
    public void RefreshPlayerReference()
    {
        FindPlayerAndEffects();
    }
    
    // 상태 확인 메서드들
    public bool IsTimeFrozen() => isTimeFrozen;

    public bool HasTimeStopAbility()
    {
        return SkillManager.Instance != null && SkillManager.Instance.HasSkill(SkillType.TimeStop);
    }
    public float GetCooldownPercentage() => cooldownTimer > 0 ? cooldownTimer / timeFreezeCooldown : 0f;
    public float GetRemainingFreezeDuration() => timeFreezeTimer;
}
    
