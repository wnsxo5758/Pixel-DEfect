using System;
using System.Collections;
using UnityEngine;

public class PlayerHp : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private int maxHp = 3; // 최대 체력
    [SerializeField] private int currentHp; // 현재 체력
    [SerializeField] private float healTime = 0; // 회복 시간
    [SerializeField] private float invincibilityTime = 0; // 무적시간

    [Header("피격 설정")] 
    [SerializeField] private float hitStunDuration = 0.5f; // 일반 경직 시간
    [SerializeField] private float specialStunDuration = 0.25f; // 특수 상태 경직 시간
    
    [Header("UI")]
    [SerializeField] private UIPlayerData uiPlayer;
    
    private PlayerController player;
    private PlayerAnimator playerAnimator;
    private SpriteRenderer spriteRenderer; // 피격시 색상 변경을 위한 스프라이트 렌더러
    private Color originColor; //플레이어 초기 색상
    private Rigidbody2D rb;
    private MovementRigidbody2D movement;

    private PlayerSound playerSound;

    private float currentHitStunDuration; // 현재 적용중인 경직 시간
    private bool isHit; // 피격 중인가
    private bool isDead;
    private bool isHealing; // 회복 중인가
    private bool isInvincible; // 무적인가

    private DeathCause lastDeathCause = DeathCause.None;
    private DeathData currentDeathData;

    public System.Action<int, int> OnHpChanged;
    public event Action OnPlayerDeath;
    public event Action<DeathCause> OnPlayerDeathWithCause;
    public bool IsHit { get => isHit; set => isHit = value; }
    public bool IsDead => isDead;
    public DeathCause LastDeathCause => lastDeathCause;
    
    private void Awake()
    {
        currentHp = maxHp;
        playerSound = GetComponentInChildren<PlayerSound>();
        player = GetComponent<PlayerController>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        movement = GetComponent<MovementRigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        originColor = spriteRenderer.color;
    }

    // 일반적인 데미지 처리 (경직만 있음)
    public void DecreaseHp(int damage, DeathData deathData, bool canDodge = false, bool canFreeze = false)
    {
        if (isDead) return;
        
        bool dodged = false;

        if (canDodge && player != null)
        {
            dodged = player.OnAttackReceived(canFreeze);
        }

        // 무적 or 회피 상태 체크
        if (isInvincible || dodged) return;
        
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Debug.Log("플레이어 사망");
            currentHp = 0;
            currentDeathData = deathData;
            lastDeathCause = deathData.cause;
            Die();
        }
        else
        {
            Debug.Log("플레이어에게 " + damage + "데미지");
            playerSound.HitSound();
            HandleHit(Vector2.zero);
        }

        uiPlayer.SetHpAll(currentHp); // 여기서 전체 갱신
    }

    // 넉백이 포함된 데미지 처리
    public void DecreaseHp(int damage, Vector2 knockBack, DeathData deathData, bool canDodge = false, bool freeze = false)
    {
        if (isDead) return;
        
        bool dodged = false;

        if (canDodge && player != null)
        {
            dodged = player.OnAttackReceived(freeze);
        }
        
        if (isInvincible || dodged) return;
        
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Debug.Log("플레이어 사망");
            
            currentHp = 0;
            currentDeathData = deathData;
            lastDeathCause = deathData.cause;
            Die();
        }
        else
        {
            playerSound.HitSound();
            Debug.Log("플레이어에게 " + damage + "데미지");
            
            HandleHit(knockBack);
        }

        uiPlayer.SetHpAll(currentHp);
    }
    
    private void HandleHit(Vector2 knockBack)
    {
        var currentState = player.GetCurrentState();
        bool isSpecialState = currentState is not PlayerStates.Idle and not PlayerStates.Run 
            and not PlayerStates.Jump and not PlayerStates.Crawl;
        
        // 경직 시간 설정
        currentHitStunDuration = isSpecialState ? specialStunDuration : hitStunDuration;
        isHit = true;
        
        //넉백 처리
        if (!isSpecialState && knockBack != Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(knockBack, ForceMode2D.Impulse);
        }
        
        // 피격 효과
        OnInvincibility(2f);
        
        // 상태 전환 또는 경직 처리
        if (isSpecialState)
        {
            StartCoroutine(SpecialStateStun());
        }
        else
        {
            if (knockBack == Vector2.zero)
            {
                player.UpdateMove(0);
            }
            player.ChangeState(new PlayerStates.Hit());
        }
    }
    
    public void Die()
    {
        isDead = true;
        currentHp = 0;
        playerSound.DeadthSound();
        ApplyDeathPhysics();
        playerAnimator.TriggerDeathAnim(currentDeathData);
        OnPlayerDeath?.Invoke();
        OnPlayerDeathWithCause?.Invoke(lastDeathCause);
        uiPlayer.SetDeathUI();
    }
    
    public void OnInvincibility(float time) // 무적상태
    {
        if (isInvincible)
        {
            invincibilityTime += time;
        }
        else
        {
            invincibilityTime = time;
            StartCoroutine(nameof(Invincibility));
        }
    }

    private IEnumerator Invincibility() // 무적상태, 캐릭터 깜빡이는 효과
    {
        isInvincible = true;
        float blinkSpeed = 10;

        while (invincibilityTime > 0)
        {
            invincibilityTime -= Time.deltaTime;
            Color color = spriteRenderer.color;
            color.a = Mathf.SmoothStep(0, 1, Mathf.PingPong(Time.time * blinkSpeed, 1));
            spriteRenderer.color = color;

            yield return null;
        }

        spriteRenderer.color = originColor;
        isInvincible = false;
    }

    private IEnumerator SpecialStateStun()
    {
        float timer = currentHitStunDuration;

        movement.MoveTo(0);

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        
        isHit = false;
    }

    private void ApplyDeathPhysics()
    {
        switch (currentDeathData.cause)
        {
            case DeathCause.Press:
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.isKinematic = true;
                break;
            
            case DeathCause.Drowning:
                rb.gravityScale = 0.2f;
                break;
            
            default:
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                break;
        }
    }
    
    public void ResetDeathState()
    {
        isDead = false;
        isHit = false;
        lastDeathCause = DeathCause.None;
        currentDeathData = new DeathData();
        
        // 체력을 최대 체력으로 설정
        SetHp(GetMaxHp());
        
        // 무적 상태 설정
        uiPlayer.ResetDeathUI();
        uiPlayer.SetHpAll(currentHp);
    }

    public void TakeFallDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);
        
        if (uiPlayer != null)
        {
            uiPlayer.SetHpAll(currentHp);
        }
    }
    
    public float GetCurrentHitStunDuration()
    {
        return currentHitStunDuration;
    }

    public int GetMaxHp()
    {
        return maxHp;
    }
    
    public int GetCurrentHp()
    {
        return currentHp;
    }

    public void SetHp(int newHp)
    {
        currentHp = newHp;
    }
    
    public int IncreaseHp(int amount)  // 체력 회복
    {
        if (amount <= 0) return 0;

        int previousHp = currentHp;
        currentHp = Mathf.Min(currentHp + amount, maxHp);
        
        int actualHealed = currentHp - previousHp;

        OnHpChanged?.Invoke(currentHp, maxHp);
        
        Debug.Log($"체력 회복: {actualHealed} (현재: {currentHp}/{maxHp})");
        uiPlayer.SetHpAll(currentHp);
        return actualHealed;
    }

    public void TriggerDebugDeath()
    {
        if (isDead) return;

        DeathData deathData = new DeathData(DeathCause.Environmental);
        currentDeathData = deathData;
        lastDeathCause = deathData.cause;
        Die();
    }
}
