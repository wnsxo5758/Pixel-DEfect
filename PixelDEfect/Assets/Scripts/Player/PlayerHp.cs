using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHp : MonoBehaviour
{
    [SerializeField]
    private int maxHp = 3; // 최대 체력
    private int currentHp; // 현재 체력
    private bool isDead;
    [Header("회복약 관련")]
    [SerializeField]
    private int maxMedickit = 3; // 최대 회복약
    private int currentMedickit; // 현재 회복약
    private int healAmount = 1; //회복약 사용시 회복양


    public int CurrentMedicKit
    {
        set
        {
            Mathf.Clamp(value, 0, maxHp);
        }
        get => currentMedickit;
    }
    [SerializeField]
    private float healTime = 0; // 회복 시간
    private bool isHealing = false; // 회복 중인가?

    [SerializeField]
    private float invincibilityTime = 0; // 무적시간
    private bool isInvincibility = false; // 무적인가

    private SpriteRenderer spriteRenderer; // 피격시 색상 변경을 위한 스프라이트 렌더러
    private Color originColor; //플레이어 초기 색상
    [SerializeField]
    private UIPlayerData uiPlayer;


    private void Awake()
    {
        currentMedickit = maxMedickit;
        currentHp = maxHp;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originColor = spriteRenderer.color;
    }

    public void DecreaseHp(int damage) // 체력 피해
    {
        if (isInvincibility == true || isDead) return; // 무적이라면 데미지X
        currentHp -= damage;
        Debug.Log($"플레이어가 {damage}만큼의 데미지를 받아서 현재 체력 {currentHp}");
        OnInvincibility(1.5f);
        uiPlayer.SetHp(currentHp, false);
        CheckDead();

    }

    public void GetMedicKit()
    {
        if (currentMedickit >= maxMedickit) return;
        currentMedickit++;
        Debug.Log($"구급약 흭득, 현재 구급약 {currentMedickit}");

    }
    private void CheckDead() // 사망 확인
    {
        if (currentHp <= 0)
        {
            Debug.Log("플레이어 사망");
            currentHp = 0;
            isDead = true;
            GameManager manager = FindObjectOfType<GameManager>();
            manager.RestartGame();
        }
    }
    public void IncreaseHp()  // 체력 회복
    {
        if (currentHp == maxHp || currentMedickit <= 0 || isHealing == true)
        {
            Debug.Log($"체력 회복 불가, 현재 체력 : {currentHp} , 현재 구급약 {currentMedickit} , 회복 중 {isHealing}");
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
        currentMedickit--;
        Debug.Log($"체력 회복됨 현재 체력 :{currentHp}, 남은 회복약 : {currentMedickit}");
        isHealing = false;
        uiPlayer.SetHp(currentHp - 1, true);

    }


    public void OnInvincibility(float time) // 무적상태
    {
        if (isInvincibility == true)
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
        isInvincibility = true;
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
        isInvincibility = false;
    }


}
