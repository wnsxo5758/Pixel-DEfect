using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    [Header("자판기 설정")] 
    [SerializeField] private int healAmount = 8;        // 총 회복량
    [SerializeField] private float healRate = 2f;       // 초당 회복량
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private bool isUnlimited = false;  // 무제한 사용 가능 여부

    [Header("플레이어 이동 설정")] 
    [SerializeField] private Transform playerPosition;  // 플레이어가 이동할 위치
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float positionTolerance = 0.1f;
    
    [Header("UI 표시")] 
    [SerializeField] private GameObject usagePrompt;    // 사용 안내 UI
    [SerializeField] private GameObject cooldownPrompt; // 쿨다운 안내 UI

    private bool isInCooldown = false;
    private float cooldownTimer = 0f;
    
    private void Awake()
    {
        // 플레이어 위치가 설정되지 않았으면 자판기 앞쪽으로 설정
        if (playerPosition == null)
        {
            GameObject positionObj = new GameObject("PlayerPosition");
            positionObj.transform.SetParent(transform);
            positionObj.transform.localPosition = Vector3.zero; // 자판기 중앙
            playerPosition = positionObj.transform;
        }
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

        StartVendingMachineSequence(player);

        return true;
    }

    private void StartVendingMachineSequence(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            Vector3 targetPos = playerPosition != null ? playerPosition.position : transform.position;

            VendingMachineHealContext context = new VendingMachineHealContext(
                this,
                targetPos,
                moveSpeed,
                positionTolerance,
                healAmount,
                healRate);
            
            Debug.Log("회복 자판기 상호작용 시작");
            
            controller.ChangeState(new PlayerStates.VendingMachineMove(context));
        }
    }

    private void StartCooldown()
    {
        isInCooldown = true;
        cooldownTimer = cooldownTime;
    }
}
