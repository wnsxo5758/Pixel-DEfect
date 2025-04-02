using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    [SerializeField]
    private bool isInstantDeath = false; // 즉사인지 확인
    [SerializeField]
    private bool continuousDamage; // 지속적인 데미지인가?
    [SerializeField]
    private int damage;
    [SerializeField]
    private bool canDestory = false; // 플레이어와 접촉 후, 파괴되는지

    private Coroutine damageCoroutine;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return; // 대상이 플레이어가 아니라면 return

        if (isInstantDeath)
        {
            collision.GetComponent<PlayerInteraction>().MoveToSpawnPoint();
            collision.GetComponent<PlayerHp>().DecreaseHp(damage);
        }
        else if (continuousDamage)
        {
            return;
        }
        else // 즉사 장애물이 아니라면 체력 감소
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
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // 애니메이션 종료까지 대기
        }
        Destroy(gameObject);

    }
}
