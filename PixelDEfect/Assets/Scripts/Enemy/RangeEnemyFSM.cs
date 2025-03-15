using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEnemy : EnemyFSM
{
    [Header("원거리 공격 관련")]
    [SerializeField]
    private int damage;
    [SerializeField]
    private float coolTime; // 공격 쿨타임
    [SerializeField]
    private float currentCoolTime; // 현재 쿨타임


    protected override IEnumerator Attack()
    {
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
