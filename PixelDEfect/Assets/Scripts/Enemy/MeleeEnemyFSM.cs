using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyFSM : EnemyFSM
{
    [Header("근접 공격 관련")]
    [SerializeField]
    private int damage;
    [SerializeField]
    private float coolTime; // 근접 공격 쿨타임
    [SerializeField]
    private float currentCoolTime; // 현재 쿨타임
    [SerializeField]
    private Collider2D attackCollider; // 공격할곳

    private bool isAttacking;
    
    
    protected override IEnumerator Attack() 
    {
        //이동을 멈춤
        while (true)
        {
            movement.MoveTo(0);
            animator.isAttack = true;
            animator.UpdateAnimation(0);
            StartCoroutine(nameof(MeleeAttack));
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }
    }
    
    private IEnumerator MeleeAttack()
    {
        if (isAttacking  ||currentCoolTime > 0) yield break;

        LookRotationToTarget();
        if (IsFacingRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), 
                                    transform.localScale.y, transform.localScale.z );
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x),
                                    transform.localScale.y, transform.localScale.z );
        }
        
        isAttacking = true;
        currentCoolTime = coolTime;
        PlaySound(attackClip);
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.3f);
        attackCollider.enabled = false;
        while (currentCoolTime > 0)
        {
            animator.isAttack = false;
            currentCoolTime -= Time.deltaTime;
            yield return null;
        }
        isAttacking = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            PlayerHp playerHp = collision.GetComponent<PlayerHp>();
            if(playerHp != null)
            {
                playerHp.DecreaseHp(damage);
                Debug.Log($"{gameObject.name}의 공격이 플레이어에게 {damage}의 데미지 부여");
            }
        }
    }


}
