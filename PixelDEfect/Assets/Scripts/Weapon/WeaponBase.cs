using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")] 
    [SerializeField] protected string weaponName;
    [SerializeField] protected int damage;
    [SerializeField] protected float attackCooldown;

    // audioSource 추가 예정
    
    public string WeaponName => weaponName;
    public int Damage => damage;
    public float AttackCooldown => attackCooldown;
}
