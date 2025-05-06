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
    
    private bool isDead = false;
    private bool isHealing = false; // 회복 중인가?
    private bool isInvincible = false; // 무적인가

    public event Action OnPlayerDeath;
    public int CurrentHp => currentHp;
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
        originColor = spriteRenderer.color;
    }

    public void DecreaseHp(int damage, GameObject damageSource = null)
    {
        if (isInvincible || isDead || !player.CanTakeDamage(damageSource)) return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0); // 음수 방지

        if (currentHp <= 0)
        {
            Debug.Log("플레이어 사망");

            Die();
        }
        else
        {
            Debug.Log("플레이어에게 " + damage + "데미지");
            
            OnInvincibility(1.5f);
        }

        uiPlayer.SetHpAll(currentHp); // 여기서 전체 갱신
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
