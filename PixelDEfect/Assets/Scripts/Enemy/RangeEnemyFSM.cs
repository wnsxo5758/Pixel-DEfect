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
    [SerializeField]
    private GameObject bulletPrefab; // 총알 프리팹
    [SerializeField]
    private Transform firePos; // 공격위치
    [SerializeField]
    private float bulletSpeed;

    protected override IEnumerator Attack()
    {
        Debug.Log("플레이어에 대한 공격!");
        movement.MoveTo(0);
        animator.isAttack = true;
        animator.UpdateAnimation(0);
        while (true)
        {
            if(currentCoolTime <= 0)// 쿨타임이 0보다 작으면
            {
                Shooting(); // 총알 발사
                currentCoolTime = coolTime; // 쿨타임 초기화
            }
            else
            {
                currentCoolTime -= Time.deltaTime; // 쿨타임 
            }

            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }

    }

    private void Shooting()
    {
        //아무것도 없으면 return
        if (bulletPrefab == null || firePos == null) return;

        //
        GameObject bullet = Instantiate(bulletPrefab, firePos.position, Quaternion.identity);

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        if (rigid != null)
        {
            Vector2 direction = (target.position - firePos.position).normalized; // 플레이어 방향 계산
            rigid.velocity = direction * bulletSpeed; // 총알 이동
        }
    }
}
