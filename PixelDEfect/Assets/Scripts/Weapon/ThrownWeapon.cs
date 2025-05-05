using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ThrownWeapon : MonoBehaviour
{
    [Header("Weapon Settings")] 
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float stuckDuration = 0.5f;
    [SerializeField] private float extraDamageMultiplier = 1.5f;
    [SerializeField] private LayerMask stickLayers;
    [SerializeField] private float hitStunDuration = 3f; // 던진 무기 피격 시간
    
    private WeaponBase weaponData;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;
    
    private bool isStuck = false;
    private bool canDealDamage = true;
    private Transform stuckTarget;
    private Vector2 contactNormal; // 충돌 표면의 법선 벡터
    private Vector2 throwDirection; // 던지는 방향 저장
    private Vector2 playerPositionOnThrow; // 던진 시점의 플레이어 위치
    public LayerMask StickLayers => stickLayers;

    private static readonly Vector2[] possibleNormals =
    {
        Vector2.right,
        Vector2.left,
        Vector2.up,
        Vector2.down,
    };
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
    }

    public void Initialize(WeaponBase weaponData, Vector2 throwForce, Vector2 direction, Vector2 playerPosition)
    {
        this.weaponData = weaponData;
        
        throwDirection = direction.normalized;

        playerPositionOnThrow = playerPosition;
        
        // 무기 데이터 적용
        if (spriteRenderer != null && weaponData.GetComponent<SpriteRenderer>() != null)
        {
            spriteRenderer.sprite = weaponData.GetComponent<SpriteRenderer>().sprite;
            spriteRenderer.flipX = direction.x < 0;
            
            // 콜라이더 크기 설정
            float radius = Mathf.Max(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y) * 0.4f;
            circleCollider.radius = radius;
            circleCollider.offset = Vector2.zero;
        }
        
        // Rigidbody 설정
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.angularDrag = 0.1f;
            rb.gravityScale = 1f;
            rb.velocity = Vector2.zero;
            rb.AddForce(throwForce, ForceMode2D.Impulse);
            rb.AddTorque(direction.x > 0 ? -rotationSpeed : rotationSpeed);
        }

        gameObject.layer = LayerMask.NameToLayer("Weapon");
        
        canDealDamage = true;
        isStuck = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isStuck) return;

        bool canStick = ((1 << collision.gameObject.layer) & stickLayers) != 0;

        if (canStick)
        {
            StickTo(collision);
        }
        
        // 적 감지 및 데미지 처리
        if (canDealDamage && collision.transform.CompareTag("Enemy"))
        {
            EnemyBT enemy = collision.transform.GetComponent<EnemyBT>();
            if (enemy != null)
            {
                float damage = weaponData.Damage;
                enemy.DecreaseHp((int)damage, true);
                Debug.Log($"Hit enemy with thrown weapon for {damage} damage");
                
                canDealDamage = false;
            }
        }
    }

    private void StickTo(Collision2D collision)
    {
        if (isStuck) return;
        
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 impactPoint = contact.point;
        Vector2 impactNormal = contact.normal;

        // 법선 벡터 저장
        contactNormal = impactNormal;
        
        if (throwDirection != Vector2.zero && Mathf.Abs(impactNormal.x) > Mathf.Abs(impactNormal.y))
        {
            Vector2 inversedThrowDir = -throwDirection;

            float angleWithPhysicsNormal = Vector2.Angle(impactNormal, inversedThrowDir);

            if (angleWithPhysicsNormal < 100f)
            {
                contactNormal = GetClosestCardinalDirection(inversedThrowDir);
            }
        }
        
        ValidateNormalWithPlayerPosition(impactPoint);
        
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.isKinematic = true;
        
        circleCollider.isTrigger = true;
        
        // 위치 & 회전 보정
        transform.position = impactPoint;
        
        float angle = Mathf.Atan2(impactNormal.y, impactNormal.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // 충돌 대상에 부착
        if (collision.rigidbody != null)
        {
            transform.SetParent(collision.transform);
            stuckTarget = collision.transform;
        }
        
        isStuck = true;
    }

    private void ValidateNormalWithPlayerPosition(Vector2 impactPoint)
    {
        if (Mathf.Abs(contactNormal.y) > Mathf.Abs(contactNormal.x))
        {
            return;
        }

        // 플레이어와 무기의 X 위치 차이
        float xDifference = impactPoint.x - playerPositionOnThrow.x;
        
        // 법선 벡터의 예상 X 방향
        float expectedNormalX = Mathf.Sign(xDifference);
        
        // 법선 벡터의 현재 X 방향
        float currentNormalX = contactNormal.x;

        // 만약 법선 벡터의 X 방향이 예상과 다르면 반전
        if (expectedNormalX * currentNormalX > 0)
        {
            contactNormal = new Vector2(-contactNormal.x, contactNormal.y);
        }
    }
    
    private Vector2 GetClosestCardinalDirection(Vector2 inputVector)
    {
        Vector2 normalizedInput = inputVector.normalized;
        
        float maxDot = float.MinValue;
        Vector2 closestDirection = Vector2.right;
        
        foreach (Vector2 direction in possibleNormals)
        {
            float dot = Vector2.Dot(normalizedInput, direction);
            if (dot > maxDot)
            {
                maxDot = dot;
                closestDirection = direction;
            }
        }
        
        return closestDirection;
    }
    
    public Vector2 GetContactNormal()
    {
        return contactNormal;
    }
    
    public bool IsStuck()
    {
        return isStuck;
    }

    public WeaponBase GetWeaponData()
    {
        return weaponData;
    }

    public void PullOutFromEnemy()
    {
        if (isStuck && stuckTarget != null && stuckTarget.CompareTag("Enemy"))
        {
            EnemyBT enemy = stuckTarget.GetComponent<EnemyBT>();
            if (enemy != null)
            {
                float extraDamage = weaponData.Damage * extraDamageMultiplier;
                
                enemy.DecreaseHp((int)extraDamage);
                Debug.Log($"Extra damage dealt for {extraDamage} damage");
            }
        }
    }

    public void DetachFromEnemy(Vector2 position)
    {
        if (isStuck && stuckTarget != null)
        {
            transform.SetParent(null);
            transform.position = position;
            
            circleCollider.isTrigger = false;
            circleCollider.enabled = true;

            rb.isKinematic = false;
            rb.gravityScale = 1f;
            rb.velocity = Vector2.zero;
            rb.AddTorque(rotationSpeed);
            rb.velocity = new Vector2(Random.Range(-1f, 1f), 5f);
            
            isStuck = false;
            stuckTarget = null;
            canDealDamage = true;
        }
    }

    public void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (isStuck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
    }
}
