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
    [SerializeField] private float knockBackForce = 10f; // 넉백
    
    [Header("회복약 관련")]
    [SerializeField]
    private int maxMedicKit = 3; // 최대 회복약
    private int currentMedicKit; // 현재 회복약
    private int healAmount = 1; //회복약 사용시 회복양
    
    [Header("UI")]
    [SerializeField] private UIPlayerData uiPlayer;
    
    private PlayerController player;
    private PlayerAnimator playerAnimator;
    private SpriteRenderer spriteRenderer; // 피격시 색상 변경을 위한 스프라이트 렌더러
    private Color originColor; //플레이어 초기 색상
    private Rigidbody2D rb;
    private MovementRigidbody2D movement;

    private float currentHitStunDuration; // 현재 적용중인 경직 시간
    private bool isHit = false; // 피격 중인가
    private bool isDead = false;
    private bool isHealing = false; // 회복 중인가
    private bool isInvincible = false; // 무적인가

    public event Action OnPlayerDeath;
    public int CurrentHp => currentHp;
    public bool IsHit { get => isHit; set => isHit = value; }
    public bool IsDead => isDead;
    public int CurrentMedicKit
    {
        set => Mathf.Clamp(value, 0, maxHp);
        get => currentMedicKit;
    }
    
    private void Awake()
    {
        currentMedicKit = maxMedicKit;
        currentHp = maxHp;
        player = GetComponent<PlayerController>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        movement = GetComponent<MovementRigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        originColor = spriteRenderer.color;
    }

    public void DecreaseHp(int damage, bool canDodge = false, bool canFreeze = false)
    {
        if (isDead) return;
        
        bool dodged = false;

        if (canDodge && player != null)
        {
            dodged = player.OnAttackReceived(canFreeze);
        }

        // 무적 or 회피 상태 체크
        if (isInvincible || dodged)
        {
            return;
        }
        
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Debug.Log("플레이어 사망");
            
            currentHp = 0;
            Die();
        }
        else
        {
            Debug.Log("플레이어에게 " + damage + "데미지");
            
            HandleHit();
        }

        uiPlayer.SetHpAll(currentHp); // 여기서 전체 갱신
    }

    private void HandleHit()
    {
        var currentState = player.GetCurrentState();
        bool isSpecialState = currentState is PlayerStates.Climb || currentState is PlayerStates.Hold;
        
        // 경직 시간 설정
        currentHitStunDuration = isSpecialState ? specialStunDuration : hitStunDuration;
        isHit = true;
        
        // 넉백 처리
        // if (!isSpecialState && attacker != null && rb != null)
        // {
        //     Vector2 knockBackDirection = (transform.position - attacker.transform.position).normalized;
        //     rb.velocity = Vector2.zero;
        //     rb.AddForce(knockBackDirection * knockBackForce, ForceMode2D.Impulse);
        // }
        
        // 피격 효과
        OnInvincibility(1.5f);
        
        // 상태 전환 또는 경직 처리
        if (isSpecialState)
        {
            StartCoroutine(SpecialStateStun());
        }
        else
        {
            movement.MoveTo(0);
            player.ChangeState(new PlayerStates.Hit());
        }
    }
    
    public void GetMedicKit()
    {
        if (currentMedicKit >= maxMedicKit) return;
        currentMedicKit++;
        Debug.Log($"구급약 획득, 현재 구급약 {currentMedicKit}");

    }

    public void Die()
    {
        isDead = true;
        currentHp = 0;

        Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        if(rigid != null)
        {
            rigid.velocity = Vector2.zero; // 속도 0으로
            rigid.angularVelocity = 0f;    // 회전속도도 정지
            rigid.constraints = RigidbodyConstraints2D.FreezePositionX; // x위치 고정
        }
        playerAnimator.TriggerDeathAnim();
        OnPlayerDeath?.Invoke();
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

    public float GetCurrentHitStunDuration()
    {
        return currentHitStunDuration;
    }
    
    public void IncreaseHp()  // 체력 회복
    {
        if (currentHp == maxHp || currentMedicKit <= 0 || isHealing)
        {
            Debug.Log($"체력 회복 불가, 현재 체력 : {currentHp} , 현재 구급약 {currentMedicKit} , 회복 중 {isHealing}");
            return; // 체력이 이미 최대치이거나, 회복약이 없거나, 체력 회복 중이라면 return;
        }
        else
        {

            if (currentHp < maxHp) // 체력이 최대체력이 아닐 경우
            {
                StartCoroutine(nameof(PlayerHeal));
            }
        }
    }
    
    private IEnumerator PlayerHeal()
    {
        isHealing = true;

        yield return new WaitForSeconds(healTime);
        currentHp += healAmount;
        currentHp = Mathf.Min(currentHp, maxHp); // 최대 체력 넘지 않도록
        currentMedicKit--;

        Debug.Log($"체력 회복됨 현재 체력 :{currentHp}, 남은 회복약 : {currentMedicKit}");

        uiPlayer.SetHpAll(currentHp); // 전체 갱신
        isHealing = false;
    }
}
