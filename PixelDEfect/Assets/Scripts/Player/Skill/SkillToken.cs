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
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private float glowSpeed = 3f;
    [SerializeField] private Color glowColor = Color.yellow;
    
    [Header("사운드")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioSource audioSource;
    
    private Vector3 startPosition;
    private bool isPlayerNearby = false;
    private bool isAcquired = false;
    private Color originalColor;
    private ParticleSystem particles;

    private void Awake()
    {
        // 컴포넌트 자동 할당
        if (tokenSprite == null)
            tokenSprite = GetComponent<SpriteRenderer>();
        
        if (tokenCollider == null)
            tokenCollider = GetComponent<Collider2D>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (tokenSprite != null)
            originalColor = tokenSprite.color;
        
        // 트리거 설정
        if (tokenCollider != null)
            tokenCollider.isTrigger = true;
    }
    
    
    private void Start()
    {
        startPosition = transform.position;
        
        // 파티클 시스템 설정
        SetupParticleSystem();
    }

    private void Update()
    {
        if (isAcquired) return;

        // 부유 효과
        ApplyFloatingEffect();
        
        // 글로우 효과
        if (enableGlow)
            ApplyGlowEffect();
    }

    // 스킬 토큰 초기화
    public void Initialize(SkillType skill)
    {
        skillType = skill;
        
        // 스킬에 따른 스프라이트 설정 (나중에 구현)
        SetTokenAppearance();
    }

    // 토큰 외형 설정
    private void SetTokenAppearance()
    {
        if (tokenSprite == null) return;

        // 스킬 타입에 따른 색상 설정
        switch (skillType)
        {
            case SkillType.Teleport:
                tokenSprite.color = Color.cyan;
                glowColor = Color.cyan;
                break;
            case SkillType.TimeStop:
                tokenSprite.color = Color.yellow;
                glowColor = Color.yellow;
                break;
            default:
                tokenSprite.color = Color.white;
                glowColor = Color.white;
                break;
        }
        
        originalColor = tokenSprite.color;
    }

    // 파티클 시스템 설정
    private void SetupParticleSystem()
    {
        particles = GetComponentInChildren<ParticleSystem>();
        if (particles == null)
        {
            // 간단한 파티클 이펙트 생성
            GameObject particleObj = new GameObject("SkillTokenParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;
            
            particles = particleObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = glowColor;
            main.startSize = 0.1f;
            main.startSpeed = 1f;
            main.maxParticles = 10;
            
            var emission = particles.emission;
            emission.rateOverTime = 5f;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
        }
    }

    // 부유 효과
    private void ApplyFloatingEffect()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    // 글로우 효과
    private void ApplyGlowEffect()
    {
        if (tokenSprite == null) return;

        float glow = (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f; // 0~1 사이 값
        Color currentColor = Color.Lerp(originalColor, glowColor, glow * 0.5f);
        tokenSprite.color = currentColor;
    }

    // 플레이어 트리거 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isAcquired)
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

}
