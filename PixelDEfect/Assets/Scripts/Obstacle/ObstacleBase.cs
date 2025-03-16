using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    [SerializeField]
    private bool isInstantDeath = false; // 즉사인지 확인
    [SerializeField]
    private int damage;
    [SerializeField]
    private bool canDestory = false; // 플레이어와 접촉 후, 파괴되는지
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return; // 대상이 플레이어가 아니라면 return

        if (isInstantDeath)
        {

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
