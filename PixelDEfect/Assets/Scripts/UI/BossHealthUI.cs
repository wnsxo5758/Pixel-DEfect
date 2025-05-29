using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Slider hpSlider;

    [SerializeField] private TextMeshProUGUI bossText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private GameObject stunCountIndicator;
    [SerializeField] private Image[] stunCountDots;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private AnimationCurve damageCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float damageAnimationDuration = 0.5f;

    [Header("색상 설정")] 
    [SerializeField] private Color normalHpColor = Color.green;
    [SerializeField] private Color lowHpColor = Color.red;
    [SerializeField] private Color stunColor = Color.yellow;
    [SerializeField] private float lowHpThreshold = 0.3f;

    private BossBT currentBoss;
    private float targetHpRatio;
    private float currentHpRatio;
    private bool isAnimating = false;
    private Coroutine damageAnimationCoroutine;
    
    
    // Start is called before the first frame update
    void Start()
    {
        // 초기에는 숨김
        gameObject.SetActive(false);
        
        // 보스 자동 탐지
        FindAndConnectBoss();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentBoss != null)
        {
            if (!currentBoss.gameObject.activeInHierarchy || currentBoss == null)
            {
                DisconnectBoss();
                return;
            }

            UpdateUI();
        }
        else
        {
            FindAndConnectBoss();
        }
    }

    private void FindAndConnectBoss()
    {
        BossBT boss = FindObjectOfType<BossBT>();
        if (boss != null)
        {
            ConnectToBoss(boss);
        }
    }

    public void ConnectToBoss(BossBT boss)
    {
        currentBoss = boss;
        gameObject.SetActive(true);
        
        // 보스 이름 설정
        if (bossText != null)
        {
            bossText.text = "관리자 로봇";
        }
        
        // 초기 체력 설정
        if (hpSlider != null)
        {
            hpSlider.maxValue = 1f;
            targetHpRatio = (float)currentBoss.CurrentHp / currentBoss.MaxHp;
            currentHpRatio = targetHpRatio;
            hpSlider.value = currentHpRatio;
            UpdateHpColor();
        }

        UpdateStunCount();
    }

    public void DisconnectBoss()
    {
        currentBoss = null;
        gameObject.SetActive(false);

        if (damageAnimationCoroutine != null)
        {
            StopCoroutine(damageAnimationCoroutine);
            damageAnimationCoroutine = null;
        }
    }
    
    private void UpdateUI()
    {
        if (currentBoss == null || hpSlider == null) return;
        
        // 목표 체력 비율 계산
        float newTargetRatio = (float)currentBoss.CurrentHp / currentBoss.MaxHp;
        
        // 체력이 변경되었으면 애니메이션 시작
        if (Mathf.Abs(newTargetRatio - targetHpRatio) > 0.01f)
        {
            targetHpRatio = newTargetRatio;
            
            // 데미지 애니메이션 시작
            if (damageAnimationCoroutine != null)
            {
                StopCoroutine(damageAnimationCoroutine);
            }
            damageAnimationCoroutine = StartCoroutine(AnimateHpChange());
        }
        
        // 체력 텍스트 업데이트
        if (hpText != null)
        {
            hpText.text = $"{currentBoss.CurrentHp} / {currentBoss.MaxHp}";
        }

        // 기절 카운트 업데이트
        UpdateStunCount();
        
        UpdateHpColor();
    }

    private System.Collections.IEnumerator AnimateHpChange()
    {
        isAnimating = true;
        float startValue = currentHpRatio;
        float elapsedTime = 0f;

        while (elapsedTime < damageAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / damageAnimationDuration;
            float curveValue = damageCurve.Evaluate(progress);

            currentHpRatio = Mathf.Lerp(startValue, targetHpRatio, curveValue);
            hpSlider.value = currentHpRatio;

            yield return null;
        }
        
        currentHpRatio = targetHpRatio;
        hpSlider.value = currentHpRatio;
        isAnimating = false;
        damageAnimationCoroutine = null;
    }
    
    private void UpdateHpColor()
    {
        if (hpSlider == null) return;
        
        Image fillImage = hpSlider.fillRect.GetComponent<Image>();
        if (fillImage == null) return;
        
        // 기절 상태면 노란색
        if (currentBoss != null && currentBoss.IsStunned())
        {
            fillImage.color = stunColor;
        }
        // 낮은 체력이면 빨간색
        else if (currentHpRatio <= lowHpThreshold)
        {
            fillImage.color = lowHpColor;
        }
        // 정상 체력이면 초록색
        else
        {
            fillImage.color = normalHpColor;
        }
    }
    
    private void UpdateStunCount()
    {
        if (currentBoss == null || stunCountIndicator == null) return;
        
        int stunCount = currentBoss.GetStunCount();
        int stunThreshold = 3; // BossBT에서 stunThreshold 가져오기
        
        // 기절 카운트가 있을 때만 표시
        bool shouldShow = stunCount > 0 && !currentBoss.IsStunned();
        stunCountIndicator.SetActive(shouldShow);
        
        if (shouldShow && stunCountDots != null)
        {
            for (int i = 0; i < stunCountDots.Length; i++)
            {
                if (stunCountDots[i] != null)
                {
                    stunCountDots[i].gameObject.SetActive(i < stunCount);
                    
                    // 점 색상 설정 (임계값에 가까울수록 빨갛게)
                    if (i < stunCount)
                    {
                        float intensity = (float)(i + 1) / stunThreshold;
                        stunCountDots[i].color = Color.Lerp(Color.white, Color.red, intensity);
                    }
                }
            }
        }
    }
}
