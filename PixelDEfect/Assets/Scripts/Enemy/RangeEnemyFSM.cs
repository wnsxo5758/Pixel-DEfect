using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEnemy : EnemyFSM
{
    [Header("근접 공격 관련")]
    [SerializeField]
    private int damage;
    [SerializeField]
    private float coolTime; // 근접 공격 쿨탕미
    [SerializeField]
    private float currentCoolTime; // 현재 쿨타임


    protected override IEnumerator Attack()
    {
        //이동을 멈춤

        Debug.Log("플레이어에 대한 공격!");
        while (true)
        {
            movement.MoveTo(0);
            animator.isAttack = true;
            animator.UpdateAnimation(0);

            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }

    }
}
