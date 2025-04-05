using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")] 
    [SerializeField] protected string weaponName;
    [SerializeField] protected float damage;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float attackCooldown;
    [SerializeField] protected WeaponType weaponType;

    protected bool canAttack = true;
    protected Transform weaponHolder;
    protected AudioSource audioSource;
    
    public enum WeaponType
    {
        Melee,
        Ranged
    }

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    // 무기를 플레이어에게 장착할 때 호출
    public virtual void Equip(Transform holder)
    {
        weaponHolder = holder;
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        gameObject.SetActive(true);
    }
    
    // 무기를 해제할 때 호출
    public virtual void UnEquip()
    {
        transform.SetParent(null);
        gameObject.SetActive(false);
    }

    // 공격 메서드 (자식 클래스에서 구현)
    public abstract void Attack(Vector2 direction);

    // 공격 쿨다운 코루틴
    protected IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public string WeaponName => weaponName;
    public float Damage => damage;
    public float AttackSpeed => attackSpeed;
    public WeaponType Type => weaponType;
    public bool CanAttack => canAttack;
}
