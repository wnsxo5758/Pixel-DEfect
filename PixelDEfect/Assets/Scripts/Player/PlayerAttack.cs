using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("무기 감지")]
    [SerializeField] private float weaponDetectionRadius = 1f;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private LayerMask weaponLayer;

    [Header("근접 공격 설정")] 
    [SerializeField] private Vector2 attackOffset = new Vector2(0, 0.6f);
    [SerializeField] private PolygonCollider2D attackCollider;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector2[] attackPolygonPoints = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(0, 1f),
        new Vector2(1f, 0.5f),
        new Vector2(1f, -0.5f)
    };

    [Header("원거리 공격 설정")] 
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwUpwardForce = 5f;
    [SerializeField] private GameObject thrownWeaponPrefab;
    [SerializeField] private float throwCooldown = 1f;
    
    private WeaponBase currentWeapon;
    private PlayerController playerController;
    private PlayerAnimator playerAnimator;
    private WeaponPickup nearbyWeapon;
    private ThrownWeapon nearbyThrownWeapon;
    private GameObject attackColliderObject;
    
    private bool isAttacking = false;
    private bool hasWeapon = false;
    private bool canThrow = true;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();

        // 공격 범위 초기화
        InitializeAttackCollider();
    }

    // 공격 콜라이더 초기화
    private void InitializeAttackCollider()
    {
        if (attackCollider == null)
        {
            attackColliderObject = new GameObject("AttackCollider")
            {
                transform =
                {
                    parent = transform,
                    localPosition = attackOffset
                }
            };
            attackCollider = attackColliderObject.AddComponent<PolygonCollider2D>();
            attackCollider.isTrigger = true;
            
            attackCollider.SetPath(0, attackPolygonPoints);
        }
        else
        {
            attackColliderObject = attackCollider.gameObject;
            attackColliderObject.transform.localPosition = attackOffset;
        }

        attackColliderObject.SetActive(false);
    }

    private void Start()
    {
        InputManager.Instance.OnPickupPressed += OnPickupWeapon;
    }

    private void Update()
    {
        DetectNearbyWeapon();
        DetectNearbyThrownWeapon();
    }
    
    // 무기 감지 메소드
    private void DetectNearbyWeapon()
    {
        Collider2D pickupCollider = Physics2D.OverlapCircle(transform.position + (Vector3)attackOffset, 
            weaponDetectionRadius, pickupLayer);

        if (pickupCollider != null)
        {
            WeaponPickup pickup = pickupCollider.GetComponent<WeaponPickup>();

            if (pickup != null)
            {
                nearbyWeapon = pickup;
                InputManager.Instance.SetCanPickup(true);
                InputManager.Instance.SetCanHold(false);
            }
        }
        else
        {
            nearbyWeapon = null;

            if (nearbyThrownWeapon == null)
            {
                InputManager.Instance.SetCanPickup(false);
                InputManager.Instance.SetCanHold(true);
            }
        }
    }

    // 던져진 무기 감지 메소드
    private void DetectNearbyThrownWeapon()
    {
        Collider2D weaponCollider = Physics2D.OverlapCircle(transform.position + (Vector3)attackOffset,
            weaponDetectionRadius, weaponLayer);

        if (weaponCollider != null)
        {
            ThrownWeapon thrownWeapon = weaponCollider.GetComponent<ThrownWeapon>();

            Debug.Log(thrownWeapon.IsStuck());
            if (thrownWeapon != null && thrownWeapon.IsStuck())
            {
                nearbyThrownWeapon = thrownWeapon;
                InputManager.Instance.SetCanPickup(true);
                InputManager.Instance.SetCanHold(false);
            }
        }
        else
        {
            nearbyThrownWeapon = null;

            if (nearbyWeapon == null)
            {
                InputManager.Instance.SetCanPickup(false);
                InputManager.Instance.SetCanHold(true);
            }
        }
    }
    
    // 무기 픽업 메소드
    private void OnPickupWeapon()
    {
        // 던져진 무기 픽업
        if (nearbyThrownWeapon != null)
        {
            WeaponBase weaponData = nearbyThrownWeapon.GetWeaponData();
            
            // 적에게 박힌 무기라면 추가 데미지 적용
            float extraDamage = nearbyThrownWeapon.PullOutFromEnemy();

            if (weaponData != null)
            {
                EquipWeapon(weaponData);
                Destroy(nearbyThrownWeapon.gameObject);
                return;
            }
        }
        
        // 일반 무기 픽업
        if (nearbyWeapon != null)
        {
            WeaponBase weaponData = nearbyWeapon.GetWeaponData();

            if (weaponData != null)
            {
                Debug.Log("Pickup");

                thrownWeaponPrefab = nearbyWeapon.GetWeaponPrefab();
                EquipWeapon(weaponData);
                Destroy(nearbyWeapon.gameObject);
            }
        }
    }
    
    private void EquipWeapon(WeaponBase weaponData)
    {
        if (currentWeapon != null)
        {
            UnEquipWeapon();
        }

        currentWeapon = weaponData;
        hasWeapon = true;

        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(true);
        }
        
        InputManager.Instance.OnMeleeAttackPressed += OnMeleeAttack;
        InputManager.Instance.OnThrowWeaponPressed += OnThrowWeapon;

        // 무기 장착 상태 애니메이션 변경
        if (playerAnimator != null)
        {
            playerAnimator.SetHasWeapon(true);
        }
        // GUI에 무기 정보 표시
    }

    private void UnEquipWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon = null;
            hasWeapon = false;

            InputManager.Instance.OnMeleeAttackPressed -= OnMeleeAttack;
            InputManager.Instance.OnThrowWeaponPressed -= OnThrowWeapon;

            // 무기 해제 상태 애니메이션 변경
            if (playerAnimator != null)
            {
                playerAnimator.SetHasWeapon(false);
            }
        }
    }
    
    // 공격 쿨타임 코루틴
    private IEnumerator AttackCooldownTimer()
    {
        yield return new WaitForSeconds(currentWeapon.AttackCooldown);
        isAttacking = false;
    }
    
    // 던지기 쿨타임 코루틴
    private IEnumerator ThrowCooldownTimer()
    {
        canThrow = false;
        yield return new WaitForSeconds(throwCooldown);
        canThrow = true;
    }
    
    // 공격 입력 처리
    private void OnMeleeAttack()
    {
        if (currentWeapon != null && !isAttacking)
        {
            if (CanPlayerAttack())
            {
                isAttacking = true;
                
                MeleeAttackAnimation();
                StartCoroutine(AttackCooldownTimer());
                
                playerController.ChangeState(new PlayerStates.Attack());
            }
        }
    }

    // 무기 던지기 입력 처리
    private void OnThrowWeapon()
    {
        if (hasWeapon && !isAttacking && canThrow && CanPlayerAttack())
        {
            ThrowWeapon();
        }
    }

    // 무기 던지기 메소드
    private void ThrowWeapon()
    {
        if (currentWeapon == null || !hasWeapon) return;
        
        playerController.ChangeState(new PlayerStates.Attack());
        
        // 던지기 애니메이션 재생
        if (playerAnimator != null)
        {
            playerAnimator.TriggerThrowAnim();
        }
        
        ExecuteThrow();
        
        StartCoroutine(ThrowCooldownTimer());
    }
    
    // 근접 공격 애니메이션 재생
    private void MeleeAttackAnimation()
    {
        if (playerAnimator != null)
        {
            playerAnimator.TriggerAttackAnim();
        }
    }

    // 근접 공격 애니메이션 종료 시 Idle 상태로 변경
    public void FinishedMeleeAttackAnim()
    {
        if (currentWeapon != null && isAttacking)
        {
            playerController.ChangeState(new PlayerStates.Idle());
        }
    }

    // 던지기 공격 애니메이션 종료 시 Idle 상태로 변경
    public void FinishedThrowAnim()
    {
        playerController.ChangeState(new PlayerStates.Idle());
    }

    // 근접 공격 수행
    public void PerformMeleeAttack()
    {
        HandleAttackCollision();
        
        Debug.Log("Melee Attack");
    }

    // 무기 던지기 수행
    private void ExecuteThrow()
    {
        // 던져진 무기 생성
        if (thrownWeaponPrefab != null && currentWeapon != null)
        {
            Vector2 direction = new Vector2(transform.localScale.x, 0).normalized;
            Vector2 spawnPosition = (Vector2)transform.position + attackOffset + direction * 1f;
            
            GameObject thrownWeaponObj = Instantiate(thrownWeaponPrefab, spawnPosition, Quaternion.identity);
            ThrownWeapon thrownWeapon = thrownWeaponObj.GetComponent<ThrownWeapon>();

            if (thrownWeapon != null)
            {
                // 던지는 힘 계산 (포물선)
                Vector2 throwForceVector = direction * throwForce + Vector2.up * throwUpwardForce;
                
                thrownWeapon.Initialize(currentWeapon, throwForceVector, direction);
                
                UnEquipWeapon();
                
                Debug.Log("Weapon thrown");
            }
        }
    }

    private void HandleAttackCollision()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;

        List<Collider2D> results = new List<Collider2D>();
        attackCollider.OverlapCollider(filter, results);

        foreach (Collider2D enemyCollider in results)
        {
            EnemyFSM enemy = enemyCollider.GetComponent<EnemyFSM>();
            if (enemy != null)
            {
                enemy.DecreaseHp((int)currentWeapon.Damage);
                Debug.Log("Hit enemy");
                // 히트 이펙트
            }
        }
    }
    
    // 플레이어가 공격 가능한 상태인지 확인
    private bool CanPlayerAttack()
    {
        var currentState = playerController.GetCurrentState();

        if (currentState is PlayerStates.Climb || currentState is PlayerStates.Hold || 
            currentState is PlayerStates.Crawl || currentState is PlayerStates.Jump)
        {
            return false;
        }

        return true;
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
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPickupPressed -= OnPickupWeapon;
            InputManager.Instance.OnMeleeAttackPressed -= OnMeleeAttack;
            InputManager.Instance.OnThrowWeaponPressed -= OnThrowWeapon;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 무기 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + (Vector3)attackOffset, weaponDetectionRadius);

        if (attackCollider != null)
        {
            Gizmos.color = Color.magenta;
            Bounds bounds = attackCollider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

    void OnGUI()
    {
        if(currentWeapon != null)
            GUI.Label(new Rect(1000, 70, 300, 20),
                currentWeapon.WeaponName);
    }
}
