using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")] 
    [SerializeField] protected string weaponName;
    [SerializeField] protected float damage;
    [SerializeField] protected float attackCooldown;

    // audioSource 추가 예정
    
    public string WeaponName => weaponName;
    public float Damage => damage;
    public float AttackCooldown => attackCooldown;
}
