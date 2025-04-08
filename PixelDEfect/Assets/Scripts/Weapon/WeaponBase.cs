using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")] 
    [SerializeField] protected string weaponName;
    [SerializeField] protected float damage;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float attackCooldown;

    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    // 공격 메서드 (자식 클래스에서 구현)
    public abstract void Attack(Vector2 origin, Vector2 direction);
    
    public string WeaponName => weaponName;
    public float Damage => damage;
    public float AttackSpeed => attackSpeed;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
}
