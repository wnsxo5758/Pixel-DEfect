using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private GameObject weaponPrefab; // if 픽업 아이템 =/ 무기 아이템

    private void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerAttack playerAttack = collision.GetComponent<PlayerAttack>();

            if (playerAttack != null && weaponPrefab != null)
            {
                GameObject weaponInstance = Instantiate(weaponPrefab);
                WeaponBase weapon = weaponInstance.GetComponent<WeaponBase>();

                if (weapon != null)
                {
                    playerAttack.EquipWeapon(weapon);
                }
                
                Destroy(gameObject);
            }
        }
    }
    
}
