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
    
    private WeaponBase weaponData;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    
    private bool isStuck = false;
    private bool canDealDamage = true;
    private Transform stuckTarget;
    private Vector2 stuckLocalPosition;
    private Vector3 stuckLocalRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public void Initialize(WeaponBase weaponData, Vector2 throwForce, Vector2 direction)
    {
        this.weaponData = weaponData;
        
        // 무기 데이터 적용
        if (spriteRenderer != null && weaponData.GetComponent<SpriteRenderer>() != null)
        {
            spriteRenderer.sprite = weaponData.GetComponent<SpriteRenderer>().sprite;
            spriteRenderer.flipX = direction.x < 0;
            
            // 콜라이더 크기 설정
            boxCollider.size = spriteRenderer.bounds.size * 0.8f;
            boxCollider.offset = Vector2.zero;
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
            EnemyFSM enemy = collision.transform.GetComponent<EnemyFSM>();
            if (enemy != null)
            {
                float damage = weaponData.Damage;
                enemy.DecreaseHp((int)damage);
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

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.isKinematic = true;
        
        boxCollider.isTrigger = true;
        
        // 위치 & 회전 보정
        transform.position = impactPoint;
        float angle = Mathf.Atan2(impactNormal.y, impactNormal.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // 충돌 대상에 부착
        if (collision.rigidbody != null)
        {
            transform.SetParent(collision.transform);
            stuckTarget = collision.transform;
            stuckLocalPosition = transform.localPosition;
            stuckLocalRotation = transform.localEulerAngles;
        }
        
        isStuck = true;
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
            EnemyFSM enemy = stuckTarget.GetComponent<EnemyFSM>();
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
            
            boxCollider.isTrigger = false;
            boxCollider.enabled = true;

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
