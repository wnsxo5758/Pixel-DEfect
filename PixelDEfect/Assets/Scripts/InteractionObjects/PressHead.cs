using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressHead : MonoBehaviour
{
    [SerializeField]
    private int damage;
    [SerializeField]
    private bool isPressing = false;

    public void SetPressing(bool pressing)
    {
        isPressing = pressing;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isPressing) return;
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHp>().DecreaseHp(damage);
        }
        else if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyFSM>().DecreaseHp(damage);
        }
    }
}
