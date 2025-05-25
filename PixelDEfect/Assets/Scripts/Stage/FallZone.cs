using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallZone : MonoBehaviour
{
    [Header("낙사 설정")] 
    [SerializeField] private int fallDamage = 1;
    [SerializeField] private float teleportDelay = 0.5f;

    private bool isProcessingFall = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isProcessingFall)
        {
            StartCoroutine(ProcessPlayerFall(collision.gameObject));
        }
        else if (collision.CompareTag("Weapon") || collision.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            ThrownWeapon thrownWeapon = collision.GetComponent<ThrownWeapon>();
            if (thrownWeapon != null)
            {
                StartCoroutine(ProcessWeaponFall(thrownWeapon));
            }
        } 
    }

    private IEnumerator ProcessPlayerFall(GameObject player)
    {
        isProcessingFall = true;
        
        yield return new WaitForSeconds(teleportDelay);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessPlayerFall(player, fallDamage);
        }

        isProcessingFall = false;
    }

    private IEnumerator ProcessWeaponFall(ThrownWeapon weapon)
    {
        if (weapon == null) yield break;

        weapon.StopMovement();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
            if (playerAttack != null)
            {
                playerAttack.RecallWeaponFromFall(weapon);
            }
        }

        yield break;
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isProcessingFall = false;
        }
    }
}
