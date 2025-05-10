using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    [SerializeField]
    private bool isInstantDeath = false; // ������� Ȯ��
    [SerializeField]
    private bool continuousDamage; // �������� �������ΰ�?
    [SerializeField]
    private int damage;
    [SerializeField]
    private bool canDestory = false; // �÷��̾�� ���� ��, �ı��Ǵ���

    private Coroutine damageCoroutine;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return; // ����� �÷��̾ �ƴ϶�� return

        if (isInstantDeath)
        {
            // collision.GetComponent<PlayerInteraction>().MoveToSpawnPoint();
            collision.GetComponent<PlayerHp>().DecreaseHp(damage);
        }
        else if (continuousDamage)
        {
            return;
        }
        else // ��� ��ֹ��� �ƴ϶�� ü�� ����
        {
            collision.GetComponent<PlayerHp>().DecreaseHp(damage);
            if(canDestory)
            {
                StartCoroutine(nameof(ObstacleDestory));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(continuousDamage)
        {
            if (damageCoroutine != null)
            {
                damageCoroutine = null;
                StopAllCoroutines();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if(damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(ApplyContinuousDamage(collision.GetComponent<PlayerHp>()));
            }
        }
        else if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyFSM>().DecreaseHp(damage);
        }
    }

    private IEnumerator ApplyContinuousDamage(PlayerHp hp)
    {
        while(true)
        {
            hp.DecreaseHp(damage);
            yield return new WaitForSeconds(1f);
        }
    }

    private  IEnumerator ObstacleDestory()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if(animator != null)
        {
            animator.SetTrigger("isDeath");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // �ִϸ��̼� ������� ���
        }
        Destroy(gameObject);

    }
}
