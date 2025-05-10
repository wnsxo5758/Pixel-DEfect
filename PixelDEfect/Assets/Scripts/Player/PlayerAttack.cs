using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
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

    [Header("텔레포트 설정")] 
    [SerializeField] private float teleportDelayTime = 0.5f;
    [SerializeField] private float teleportCooldown = 3f;
    
    private WeaponBase currentWeapon;
    private PlayerController playerController;
    private PlayerAnimator playerAnimator;
    private MovementRigidbody2D movement;
    private ThrownWeapon lastThrownWeapon;
    private GameObject attackColliderObject;
    
    private bool isAttacking = false;
    private bool hasWeapon = false;
    private bool canThrow = true;
    private bool canTeleport = true;
    private bool isTeleporting = false;
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        movement = GetComponent<MovementRigidbody2D>();

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
    
    // PlayerController에서 호출되는 근접 공격
    public void PerformMeleeAttack()
    {
        if (CanPlayerAttack())
        {
            isAttacking = true;
            
            MeleeAttackAnimation();
            StartCoroutine(AttackCooldownTimer());
            
            playerController.ChangeState(new PlayerStates.Attack());
        }
    }
    
    // PlayerController에서 호출되는 던지기
    public void PerformThrowWeapon()
    {
        if (hasWeapon && !isAttacking && canThrow && CanPlayerAttack())
        {
            ThrowWeapon();
        }
    }
    
    // PlayerController에서 호출되는 텔레포트
    public void PerformTeleport()
    {
        if (lastThrownWeapon != null && canTeleport && !isTeleporting &&
            playerController.GetCurrentState() is not PlayerStates.Climb and not PlayerStates.Hold and not PlayerStates.Valve)
        {
            // 텔레포트 전 검증
            if (IsTeleportPossible(lastThrownWeapon))
            {
                StartCoroutine(TeleportCoroutine(lastThrownWeapon));
            }
            else
            {
                // 텔레포트 불가
                Debug.Log("텔레포트 불가");
            }
        }
    } 
    
    // 무기 픽업 메소드
    public void ProcessWeaponPickup(WeaponPickup weaponPickup, ThrownWeapon thrownWeapon)
    {
        // 던져진 무기 픽업
        if (thrownWeapon != null && canThrow)
        {
            WeaponBase weaponData = thrownWeapon.GetWeaponData();
            
            // 적에게 박힌 무기라면 추가 데미지 적용
            thrownWeapon.PullOutFromEnemy();

            if (weaponData != null)
            {
                EquipWeapon(weaponData);
                Destroy(thrownWeapon.gameObject);
                return;
            }
        }
        
        // 일반 무기 픽업
        if (weaponPickup != null)
        {
            WeaponBase weaponData = weaponPickup.GetWeaponData();

            if (weaponData != null)
            {
                Debug.Log("Pickup");

                thrownWeaponPrefab = weaponPickup.GetWeaponPrefab();
                EquipWeapon(weaponData);
                Destroy(weaponPickup.gameObject);
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

        lastThrownWeapon = null;
        
        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(true);
        }

        canTeleport = false;
        
        // 무기 장착 상태 애니메이션 변경
        if (playerAnimator != null)
        {
            playerAnimator.SetHasWeapon(true);
        }
    }

    private void UnEquipWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon = null;
            hasWeapon = false;
            
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
            movement.MoveTo(0);
            playerController.ChangeState(new PlayerStates.Idle());
        }
    }

    // 던지기 공격 애니메이션 종료 시 Idle 상태로 변경
    public void FinishedThrowAnim()
    {
        movement.MoveTo(0);
        playerController.ChangeState(new PlayerStates.Idle());
    }

    // 무기 던지기 수행
    private void ExecuteThrow()
    {
        // 던져진 무기 생성
        if (thrownWeaponPrefab != null && currentWeapon != null)
        {
            Vector2 direction = new Vector2(transform.localScale.x, 0).normalized;
            Vector2 spawnPosition = (Vector2)transform.position + attackOffset;
            
            GameObject thrownWeaponObj = Instantiate(thrownWeaponPrefab, spawnPosition, Quaternion.identity);
            ThrownWeapon thrownWeapon = thrownWeaponObj.GetComponent<ThrownWeapon>();

            if (thrownWeapon != null)
            {
                // 던지는 힘 계산 (포물선)
                Vector2 throwForceVector = direction * throwForce + Vector2.up * throwUpwardForce;
                
                thrownWeapon.Initialize(currentWeapon, throwForceVector, direction, transform.position);

                lastThrownWeapon = thrownWeapon;

                canTeleport = true;
                
                UnEquipWeapon();
            }
        }
    }

    public void HandleAttackCollision()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;

        List<Collider2D> results = new List<Collider2D>();
        attackCollider.OverlapCollider(filter, results);

        foreach (Collider2D enemyCollider in results)
        {
            EnemyBT enemy = enemyCollider.GetComponent<EnemyBT>();
            ITimeAffected timeAffected = enemyCollider.GetComponent<ITimeAffected>();
            
            if (enemy != null)
            {
                // 공격 방향 계산
                Vector2 attackDirection = (enemyCollider.transform.position - transform.position).normalized;
                int damage = currentWeapon.Damage;
                
                // 시간이 정지된 상태인지 확인
                if (TimeManager.Instance.IsTimeFrozen() && timeAffected != null)
                {
                    // 시간 정지 중 데미지 적용
                    TimeManager.Instance.ApplyDamageInFrozenTime(timeAffected, damage, attackDirection);
                    
                    // 시각 효과만 표시
                    CameraController.Instance.ShakeScreen(0.1f, 0.05f, 0.05f);
                }
                else
                {
                    enemy.DecreaseHp(damage);
                    CameraController.Instance.ShakeScreen();
                }
            }
        }
    }

    private IEnumerator TeleportCoroutine(ThrownWeapon targetWeapon)
    {
        canTeleport = false;
        isTeleporting = true;
        
        // 텔레포트 시작 이펙트 (구현 예정)
        
        playerController.ChangeState(new PlayerStates.Teleport());

        yield return new WaitForSeconds(teleportDelayTime);
        
        // 무기가 이미 파괴되었는지 확인
        if (targetWeapon == null)
        {
            isTeleporting = false;
            playerController.ChangeState(new PlayerStates.Idle());
            StartCoroutine(TeleportCooldownTimer());
            yield break;
        }

        Vector3 teleportPosition;

        // 텔레포트 위치 계산
        if (targetWeapon.IsStuck())
        {
            Vector2 contactNormal = targetWeapon.GetContactNormal();
            Vector2 teleportDirection;

            Vector2 playerSize = GetComponent<Collider2D>().bounds.size;
            float teleportDistance = playerSize.x * 3f;
        
            float absNormalX = Mathf.Abs(contactNormal.x);
            float absNormalY = Mathf.Abs(contactNormal.y);
        
            if (absNormalX > absNormalY)
            {
                float diagonalX = Mathf.Sign(contactNormal.x);
                teleportDirection = new Vector2(diagonalX, 1f).normalized;
                teleportDistance = playerSize.x * 3f;
            }
            else
            {
                teleportDirection = contactNormal.y < 0 ? Vector2.down : Vector2.up;
                teleportDistance = playerSize.y;
            }
        
            teleportPosition = (Vector2)targetWeapon.transform.position + teleportDirection * teleportDistance;
        }
        else
        {
            teleportPosition = targetWeapon.transform.position;
        }
        
        // 무기가 날아가는 중이면 리지드바디 멈추기
        if (!targetWeapon.IsStuck())
        {
            targetWeapon.StopMovement();
        }
        // 적에게 무기가 박혀있는 경우 
        else
        {
            targetWeapon.PullOutFromEnemy();
        }
        
        // 무기 장착
        WeaponBase weaponData = targetWeapon.GetWeaponData();
        if (weaponData != null)
        {
            EquipWeapon(weaponData);
        }
        
        // 플레이어 위치 이동
        transform.position = teleportPosition;
        
        // 도착 이펙트 (구현 예정)
        
        // 던져진 무기 제거
        Destroy(targetWeapon.gameObject);

        //텔레포트 쿨다운
        StartCoroutine(TeleportCooldownTimer());
        
        isTeleporting = false;
    }

    private bool IsTeleportPossible(ThrownWeapon weapon)
    {
        if (weapon == null) return false;
        
        Vector3 weaponPosition = weapon.transform.position;
        
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return false;
        
        Vector2 playerSize = playerCollider.bounds.size;
        
        // 무기가 박혀있지 않으면 무기 위치 반환
        if (!weapon.IsStuck())
        {
            return true;
        }

        Vector2 contactNormal = weapon.GetContactNormal();
        
        float absNormalX = Mathf.Abs(contactNormal.x);
        float absNormalY = Mathf.Abs(contactNormal.y);

        Vector2 teleportDirection;
        float teleportDistance;

        // 법선 벡터가 수평 방향인 경우
        if (absNormalX > absNormalY)
        {
            float diagonalX = Mathf.Sign(contactNormal.x);
            teleportDirection = new Vector2(diagonalX, 1f).normalized;
            
            // 수평 벽의 경우 더 큰 거리 설정
            teleportDistance = playerSize.x * 3f;
        }
        // 법선 벡터가 수직 방향인 경우
        else
        {
            teleportDirection = contactNormal.y < 0 ? Vector2.down : Vector2.up;

            teleportDistance = playerSize.y;
        }
        
        // 텔레포트 위치 계산
        Vector2 basePosition = (Vector2)weaponPosition + teleportDirection * teleportDistance;

        if (!IsSafeLocation(basePosition, weapon.gameObject))
        {
            return false;
        }
        
        return true;
    }

    private bool IsSafeLocation(Vector2 position, GameObject weaponObj)
    {
        ThrownWeapon weapon = weaponObj.GetComponent<ThrownWeapon>();
        if (weapon == null) return false;
        
        Vector2 weaponPosition = weapon.transform.position;
        Vector2 contactNormal = weapon.GetContactNormal();

        float offsetDistance = 0.2f;
        Vector2 adjustedStartPosition;

        if (weapon.IsStuck() && contactNormal != Vector2.zero)
        {
            adjustedStartPosition = weaponPosition - contactNormal * offsetDistance;
        }
        else
        {
            adjustedStartPosition = weaponPosition;
        }
        
        RaycastHit2D[] pathHits = Physics2D.LinecastAll(adjustedStartPosition, position, weapon.StickLayers);

        foreach (RaycastHit2D hit in pathHits)
        {
            if (hit.collider != null && !IsWeaponOrItsParent(hit.collider.gameObject, weaponObj))
            {
                Debug.Log($"물체 감지: {hit.collider.gameObject}");
                return false;
            }
        }
        
        return true;
    }
    
    private bool IsWeaponOrItsParent(GameObject obj, GameObject weapon)
    {
        if (obj == weapon) return true;

        Transform weaponParent = weapon.transform.parent;
        if (weaponParent != null && obj == weaponParent.gameObject) return true;

        return false;
    }
    
    private IEnumerator TeleportCooldownTimer()
    {
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }
    
    // 플레이어가 공격 가능한 상태인지 확인
    private bool CanPlayerAttack()
    {
        var currentState = playerController.GetCurrentState();

        // 낙하 중에는 공격 불가
        if (currentState is PlayerStates.Idle || currentState is PlayerStates.Run)
        {
            if (!movement.IsGrounded)
            {
                return false;
            }
        }
        
        // 공격 가능한 상태 확인
        if (currentState is PlayerStates.Climb || currentState is PlayerStates.Hold || 
            currentState is PlayerStates.Crawl || currentState is PlayerStates.Roll || 
            currentState is PlayerStates.Teleport || currentState is PlayerStates.Valve)
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

    private void OnDrawGizmosSelected()
    {
        if (lastThrownWeapon != null && lastThrownWeapon.IsStuck())
        {
            Vector2 normal = lastThrownWeapon.GetContactNormal();
            Vector3 weaponPos = lastThrownWeapon.transform.position;
            
            if (normal != Vector2.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(weaponPos, normal);

                float offsetDistance = 0.2f;
                Vector2 adjustedStartPosition = (Vector2)weaponPos - normal * offsetDistance;
                
                Vector2 teleportDirection;
                float teleportDistance;
                Collider2D playerCollider = GetComponent<Collider2D>();
                
                // 계산된 텔레포트 방향 시각화
                float absNormalX = Mathf.Abs(normal.x);
                float absNormalY = Mathf.Abs(normal.y);
                
                Vector2 playerSize = playerCollider.bounds.size;
                
                if (absNormalX > absNormalY)
                {
                    float diagonalX = Mathf.Sign(normal.x);
                    teleportDirection = new Vector2(diagonalX, 1f).normalized;
                    teleportDistance = playerSize.x * 3f;
                    Gizmos.color = Color.magenta;
                }
                else
                {
                    teleportDirection = normal.y < 0 ? Vector2.down : Vector2.up;
                    teleportDistance = playerSize.y;
                    Gizmos.color = Color.green;
                }

                Vector2 teleportPos = (Vector2)weaponPos + teleportDirection * teleportDistance;
                
                Gizmos.DrawLine(adjustedStartPosition, teleportPos);
                Gizmos.DrawWireSphere(teleportPos, 0.4f);
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
