using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : EnemyFSM
{
    [SerializeField]
    private Vector2 originalPos; // ó�� ��ġ

    private void Start()
    {
        originalPos = transform.position;
    }

    protected override IEnumerator Attack()
    {
        Debug.Log($"{gameObject.name}�� �÷��̾ ����!");
        movement.MoveTo(0);
        // animator.UpdateAnimation(0);
        yield return new WaitForSeconds(1f);
        CalculateDistanceToTargetAndSelectState();
    }

    protected override IEnumerator Pursuit()
    {
        while(true)
        {
            if(target == null) yield break;

            Vector2 dir = (target.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, target.position, movement.RunSpeed * Time.deltaTime);
            LookRotationToTarget();
            // animator.UpdateAnimation(1);
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }
    }

    protected override IEnumerator Dead()
    {
        Debug.Log($"{gameObject.name}�� ���");
        // animator.Death();
        movement.MoveTo(0);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

}
