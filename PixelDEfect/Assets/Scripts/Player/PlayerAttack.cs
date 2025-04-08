using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("무기 감지")]
    [SerializeField] private float weaponDetectionRadius = 1f;
    [SerializeField] private LayerMask weaponLayer;
    
    private WeaponBase currentWeapon;
    private PlayerController playerController;
    private PlayerAnimator playerAnimator;
    private bool isAttacking;
    private WeaponPickup nearbyWeapon;
    private bool hasWeapon = false;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
    }

    private void Start()
    {
        InputManager.Instance.OnPickupPressed += OnPickupWeapon;
    }

    private void Update()
    {
        DetectNearbyWeapon();
    }
    
    // 무기 감지 메소드
    private void DetectNearbyWeapon()
    {
        Collider2D weaponCollider = Physics2D.OverlapCircle(transform.position, weaponDetectionRadius,
                                                            weaponLayer);

        if (weaponCollider != null)
        {
            WeaponPickup pickup = weaponCollider.GetComponent<WeaponPickup>();

            if (pickup != null)
            {
                nearbyWeapon = pickup;
                InputManager.Instance.SetCanPickup(true);
            }
        }
        else
        {
            nearbyWeapon = null;
            InputManager.Instance.SetCanPickup(false);
        }
    }

    private void OnPickupWeapon()
    {
        if (nearbyWeapon != null)
        {
            WeaponBase weaponData = nearbyWeapon.GetWeaponData();

            if (weaponData != null)
            {
                Debug.Log("Pickup");
                EquipWeapon(weaponData);
                Destroy(nearbyWeapon.gameObject);
            }
        }
    }
    
    public void EquipWeapon(WeaponBase weaponData)
    {
        if (currentWeapon != null)
        {
            UnEquipWeapon();
        }

        currentWeapon = weaponData;
        hasWeapon = true;
        
        InputManager.Instance.OnAttackPressed += OnAttack;

        // 무기 장착 상태 애니메이션 변경
        
        // GUI에 무기 정보 표시
    }

    public void UnEquipWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon = null;
            hasWeapon = false;

            InputManager.Instance.OnAttackPressed -= OnAttack;

            // 무기 해제 상태 애니메이션 변경
        }
    }
    
    // 공격 입력 처리
    private void OnAttack()
    {
        if (currentWeapon != null && !isAttacking)
        {
            if (CanPlayerAttack())
            {
                isAttacking = true;
                
                PlayerAttackAnimation();
                
                PerformAttack();

                StartCoroutine(AttackCooldownTimer());
            }
        }
    }

    // 공격 애니메이션 재생
    private void PlayerAttackAnimation()
    {
        
    }

    // 공격 수행
    private void PerformAttack()
    {
        Vector2 attackDirection = GetAttackDirection();
        
        currentWeapon.Attack(transform.position, attackDirection);
    }
    
    // 공격 쿨타임 코루틴
    private IEnumerator AttackCooldownTimer()
    {
        yield return new WaitForSeconds(currentWeapon.AttackCooldown);
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

    // 무기 소지 여부 (외부 접근용)
    public bool HasWeapon()
    {
        return hasWeapon;
    }

    // 현재 장착된 무기 정보 (외부 접근용)
    public WeaponBase GetCurrentWeapon()
    {
        return currentWeapon;
    }
    
    // 이벤트 구독 해제
    private void OnDestroy()
    {
        // if (InputManager.Instance != null)
        // {
        //     InputManager.Instance.OnAttackPressed -= OnAttack;
        // }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, weaponDetectionRadius);
    }

    void OnGUI()
    {
        if(currentWeapon != null)
            GUI.Label(new Rect(1000, 70, 300, 20),
                currentWeapon.WeaponName);
    }
}
