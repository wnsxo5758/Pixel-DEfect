using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretFSM : EnemyFSM
{
    [Header("포탑 설정")]
    [SerializeField]
    private Transform turret_Head; // 포탑 상체
    [SerializeField]
    private Transform firePos; // 총알 발사 위치
    [SerializeField]
    private GameObject bulletPrefab; // 총알 프리팹
    [SerializeField]
    private float maxCoolTime; // 쿨타임


    private float currentCoolTime; //현재 쿨타임
    private bool isAttack; // 공격 중인가

    private void TurretAttack() // 터렛 공격
    {
        if (target == null) return;

        // 플레이어 방향으로 회전
        Vector3 direction = (target.position - turret_Head.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);

        // 부드럽게 회전 (플레이어가 감지 범위 안에 있을 때)
        if (Vector2.Distance(target.position, transform.position) <= distanceToAttack)
        {
            turret_Head.rotation = Quaternion.Lerp(turret_Head.rotation, targetRotation, Time.deltaTime * 5f);
        }
        else // 범위를 벗어나면 원래 방향으로 회전
        {
            turret_Head.rotation = Quaternion.Lerp(turret_Head.rotation, Quaternion.identity, Time.deltaTime * 1f);
        }

        // 쿨타임이 다 차면 발사
        if (currentCoolTime >= maxCoolTime)
        {
            audioSource.PlayOneShot(attackClip);

            GameObject bulletObj = Instantiate(bulletPrefab, firePos.position, firePos.rotation);
            TurretBullet bullet = bulletObj.GetComponent<TurretBullet>();
            if(bullet != null)
            {
                bullet.SetUp(target.position);
            }
            currentCoolTime = 0f;
        }

        // 쿨타임 업데이트
        currentCoolTime += Time.deltaTime;

    }
    protected override void CalculateDistanceToTargetAndSelectState() //플레이어와의 거리 측정후 상태 변경
    {
        if (target == null) return; // 목표가 없으면 리턴

        float distance = Vector2.Distance(target.position, transform.position); // 플레이어와 거리 측정

        if (distance <= distanceToAttack) // 인지 범위에 들어온 경우
        {
            ChangeState(EnemyState.Attack);
        }
        else if (distance >= distanceToAttack) // 인지 범위 밖인 경우
        {
            ChangeState(EnemyState.Idle);
        }
    }

    protected override IEnumerator Idle() // 
    {
        Debug.Log($"{gameObject.name}의 현재 상태 : {enemyState}");
        while (true)
        {
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }

    }

    protected override IEnumerator Wander() // 포탑은 배회 X 
    {
        return null;
    }

    protected override IEnumerator Attack()
    {
        Debug.Log($"{gameObject.name}의 현재 상태 : {enemyState}");
        while (true)
        {
            TurretAttack();
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }
    }

}
