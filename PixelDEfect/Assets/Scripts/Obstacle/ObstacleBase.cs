using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    [SerializeField]
    private bool isInstantDeath = false; // 즉사인지 확인
    [SerializeField]
    private int damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return; // 대상이 플레이어가 아니라면 return

        if (isInstantDeath)
        {

        }

        else // 즉사 장애물이 아니라면 체력 감소
        {
            collision.GetComponent<PlayerHp>().DecreaseHp(damage);
        }
    }
}
