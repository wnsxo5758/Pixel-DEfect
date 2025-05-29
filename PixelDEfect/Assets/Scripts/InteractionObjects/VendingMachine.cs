using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    [Header("자판기 설정")] 
    [SerializeField] private int healAmount = 8;    // 총 회복량
    [SerializeField] private float healRate = 2f;      // 초당 회복량
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private bool isUnlimited = false; // 무제한 사용 가능 여부

    [Header("UI 표시")] 
    [SerializeField] private GameObject usagePrompt;    // 사용 안내 UI
    [SerializeField] private GameObject cooldownPrompt; // 쿨다운 안내 UI

    private bool isInCooldown = false;
    private float cooldownTimer = 0f;
    
    private void Awake()
    {
        
    }

    private void Update()
    {
        if (isInCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                isInCooldown = false;
                cooldownTimer = 0f;
                
                // 저장
                
            }
        }
    }
    
    public bool TryUseVendingMachine(GameObject player)
    {
        // 쿨다운 중이면 사용 불가
        if (isInCooldown && !isUnlimited)
        {
            return false;
        }
        
        PlayerHp playerHp = player.GetComponent<PlayerHp>();

        if (playerHp == null)
        {
            Debug.LogError("PlayerHp is null");
            return false;
        }
        
        // 이미 체력이 최대인지 확인
        if (playerHp.GetCurrentHp() >= playerHp.GetMaxHp())
        {
            Debug.Log("체력이 이미 최대입니다.");
            return false;
        }
        
        // 쿨다운 시작
        if (!isUnlimited)
        {
            StartCooldown();
        }

        StartHealing(player);

        return true;
    }

    private void StartHealing(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            VendingMachineHealContext context = new VendingMachineHealContext
            {
                healAmount = healAmount,
                healRate = healRate,
                vendingMachine = this
            };
            
            Debug.Log("회복 자판기 상호작용 시작");
            controller.ChangeState(new PlayerStates.VendingMachineHeal(context));
        }
    }

    private void StartCooldown()
    {
        isInCooldown = true;
        cooldownTimer = cooldownTime;
    }
}
