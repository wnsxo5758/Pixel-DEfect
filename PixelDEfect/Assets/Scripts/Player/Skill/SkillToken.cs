using System.Collections;
using UnityEngine;

public class SkillToken : MonoBehaviour
{
    [Header("스킬 토큰 설정")] 
    [SerializeField] private SkillType skillType = SkillType.None;
    [SerializeField] private SpriteRenderer tokenSprite;
    [SerializeField] private Collider2D tokenCollider;
    
    [Header("시각 효과")]
    [SerializeField] private float floatHeight = 0.3f;
    [SerializeField] private float floatSpeed = 2f;
    
    [Header("획득 효과")]
    [SerializeField] private float acquisitionAnimationDuration = 1f;
    [SerializeField] private AnimationCurve acquisitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float floatUpDistance = 2f;
    
    [Header("사운드")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioSource audioSource;
    
    private Vector3 startPosition;
    private bool isAcquired = false;

    private void Awake()
    {
        // 컴포넌트 자동 할당
        if (tokenSprite == null)
            tokenSprite = GetComponent<SpriteRenderer>();
        
        if (tokenCollider == null)
            tokenCollider = GetComponent<Collider2D>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        // 트리거 설정
        if (tokenCollider != null)
            tokenCollider.isTrigger = true;
        
        // 태그 설정 확인
        if (!gameObject.CompareTag("SkillToken"))
        {
            gameObject.tag = "SkillToken";
        }
    }
    
    
    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (isAcquired) return;

        // 부유 효과
        ApplyFloatingEffect();
    }

    // 스킬 토큰 초기화
    public void Initialize(SkillType skill)
    {
        skillType = skill;
    }

    // 부유 효과
    private void ApplyFloatingEffect()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    // 토클 획득 처리
    public void OnTokenAcquired()
    {
        if (isAcquired) return;

        isAcquired = true;
        
        // 획득 사운드 재생
        
        // 획득 애니메이션 시작
        StartCoroutine(AcquisitionAnimation());
    }
    
    // 획득 애니메이션
    private IEnumerator AcquisitionAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * floatUpDistance;
        Color startColor = tokenSprite.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < acquisitionAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / acquisitionAnimationDuration;
            float curveValue = acquisitionCurve.Evaluate(progress);
            
            // 위치 애니메이션
            transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            
            // 투명도 애니메이션
            if (tokenSprite != null)
            {
                Color currentColor = Color.Lerp(startColor, targetColor, curveValue);
                tokenSprite.color = currentColor;
            }
            
            // 크기 애니메이션 (점점 커지다가 작아짐)
            float scale = 1f + Mathf.Sin(curveValue * Mathf.PI) * 0.5f;
            transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        // 획득 완료 후 파괴
        Destroy(gameObject);
    }

    public SkillType GetSkillType()
    {
        return skillType;
    }

    public bool IsAcquired()
    {
        return isAcquired;
    }
}
