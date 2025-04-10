using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("무기 감지")]
    [SerializeField] private float weaponDetectionRadius = 1f;
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
    [SerializeField] private GameObject thrownWeaponPrefab;
    
    private WeaponBase currentWeapon;
    private PlayerController playerController;
    private PlayerAnimator playerAnimator;
    private WeaponPickup nearbyWeapon;
    private GameObject attackColliderObject;
    
    private bool isAttacking = false;
    private bool hasWeapon = false;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();

        // 공격 범위 초기화
        InitializeAttackCollider();
    }

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

        if (attackColliderObject != null && attackColliderObject.activeSelf)
        {
            UpdateAttackColliderDirection(transform.localScale.x);
        }
    }
    
    // 무기 감지 메소드
    private void DetectNearbyWeapon()
    {
        Collider2D weaponCollider = Physics2D.OverlapCircle(transform.position + (Vector3)attackOffset, 
            weaponDetectionRadius, weaponLayer);

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

    // 무기 픽업 메소드
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

        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(true);
            UpdateAttackColliderDirection(transform.localScale.x);
        }
        
        InputManager.Instance.OnAttackPressed += OnAttack;

        // 무기 장착 상태 애니메이션 변경
        if (playerAnimator != null)
        {
            playerAnimator.SetHasWeapon(true);
        }
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
    
    // 공격 입력 처리
    private void OnAttack()
    {
        if (currentWeapon != null && !isAttacking)
        {
            if (CanPlayerAttack())
            {
                isAttacking = true;
                
                MeleeAttackAnimation();

                StartCoroutine(AttackCooldownTimer());
            }
        }
    }

    // 공격 애니메이션 재생
    private void MeleeAttackAnimation()
    {
        if (playerAnimator != null)
        {
            playerAnimator.TriggerAttackAnim();
        }
    }

    // 근접 공격 수행
    public void PerformMeleeAttack()
    {
        UpdateAttackColliderDirection(transform.localScale.x);
        
        HandleAttackCollision();
        
        Debug.Log("Melee Attack");
    }

    // 공격 콜라이더 방향 조정
    private void UpdateAttackColliderDirection(float direction)
    {
        Vector2[] flippedPoints = new Vector2[attackPolygonPoints.Length];

        for (int i = 0; i < attackPolygonPoints.Length; i++)
        {
            flippedPoints[i] = new Vector2(
                attackPolygonPoints[i].x * direction,
                attackPolygonPoints[i].y);
        }
        
        attackCollider.SetPath(0, flippedPoints);
    }

    // 적 감지 및 공격 처리
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
            currentState is PlayerStates.Crawl)
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
            InputManager.Instance.OnAttackPressed -= OnAttack;
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

            if (Application.isPlaying && attackColliderObject.activeSelf)
            {
                Vector2[] points = attackCollider.GetPath(0);
                for (int i = 0; i < points.Length; i++)
                {
                    Vector2 start = attackCollider.transform.position + (Vector3)points[i];
                    Vector2 end = attackCollider.transform.position + (Vector3)points[(i + 1) % points.Length];
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }

    void OnGUI()
    {
        if(currentWeapon != null)
            GUI.Label(new Rect(1000, 70, 300, 20),
                currentWeapon.WeaponName);
    }
}
