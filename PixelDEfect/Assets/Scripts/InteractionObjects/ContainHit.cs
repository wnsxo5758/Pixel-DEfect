using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainHit : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHp playerHp = collision.GetComponent<PlayerHp>();
            if (playerHp != null)
            {
                DeathData deathData = new DeathData(DeathCause.Press);
                playerHp.DecreaseHp(9999, deathData);
            }

        }
    }
}
