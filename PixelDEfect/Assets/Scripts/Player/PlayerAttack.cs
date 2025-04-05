using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private Transform weaponHolder;
    
    private WeaponBase currentWeapon;
    private PlayerController playerController;
    private bool isAttacking;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (weaponHolder == null)
        {
            GameObject holder = new GameObject("WeaponHolder");
            holder.transform.SetParent(transform);
            holder.transform.localPosition = new Vector3(0.5f, 0, 0); // 손 위치 조정 필요
            weaponHolder = holder.transform;
        }
    }

    private void Start()
    {
        
    }

    // 이벤트 구독 해제
    private void OnDestroy()
    {
        // if (InputManager.Instance != null)
        // {
        //     InputManager.Instance.OnAttackPressed -= OnAttack;
        // }
    }

    public void EquipWeapon(WeaponBase weapon)
    {
        if (currentWeapon != null)
        {
            UnEquipWeapon();
        }

        currentWeapon = weapon;
        currentWeapon.Equip(weaponHolder);

        InputManager.Instance.OnAttackPressed += OnAttack;

        // 플레이어 애니메이션 변경
    }

    public void UnEquipWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.UnEquip();
            currentWeapon = null;
            
            // 플레이어 애니메이션 복원
        }
    }
    
    // 공격 입력 처리
    private void OnAttack()
    {
        if (currentWeapon != null && currentWeapon.CanAttack && !isAttacking)
        {
            if (CanPlayerAttack())
            {
                Vector2 attackDirection = GetAttackDirection();
                currentWeapon.Attack(attackDirection);
            }
        }
    }

    // 공격 플래그 리셋
    private void ResetAttackFlag()
    {
        isAttacking = false;
    }
    
    // 플레이어가 공격 가능한 상태인지 확인
    private bool CanPlayerAttack()
    {
        var currentState = playerController.GetCurrentState();

        if (currentState is PlayerStates.Climb || currentState is PlayerStates.Hold || 
            currentState is PlayerStates.Crawl)
        {
            return false;
        }

        return true;
    }

    // 공격 방향 계산
    private Vector2 GetAttackDirection()
    {
        Vector2 direction = new Vector2(transform.localScale.x, 0);

        return direction;
    }
}
