using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TurretState { None = -1, Idle = 0, Attack };

public class TurretBase : MonoBehaviour
{
    [Header("포탑 설정")]
    [SerializeField]
    private Transform turret_Head; // 포탑 상체
    [SerializeField]
    private Transform firePos; // 총알 발사 위치
    [SerializeField]
    private GameObject bulletPrefab; // 총알 프리팹
    [SerializeField]
    private TurretState turret_State;

    [Header("기본 설정")]
    [SerializeField]
    private float detectRange; // 포탑 감지 범위
    [SerializeField]
    private float maxCoolTime; // 쿨타임
    private float currentCoolTime; //현재 쿨타임
    private bool isAttack; // 공격 중인가

    [SerializeField]
    private Transform target;

    private void Awake()
    {
        // target으로 플레이어 설정
        if (target == null)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
        currentCoolTime = maxCoolTime;
        ChangeState(TurretState.Idle);
    }

    private void TurretAttack() // 터렛 공격
    {
        if (target == null) return;

        // 플레이어 방향으로 회전
        Vector3 direction = (target.position - turret_Head.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);

        // 부드럽게 회전 (플레이어가 감지 범위 안에 있을 때)
        if (Vector2.Distance(target.position, transform.position) <= detectRange)
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
            Instantiate(bulletPrefab, firePos.position, firePos.rotation);
            currentCoolTime = 0f;
        }

        // 쿨타임 업데이트
        currentCoolTime += Time.deltaTime;

    }

    public void ChangeState(TurretState newState)
    {
        if (turret_State == newState) return;

        StopCoroutine(turret_State.ToString()); // 이전의 행동 정지
        //새로운 상태 설정 후 실행
        turret_State = newState;
        StartCoroutine(turret_State.ToString());
    }
    private void CalculateDistanceToTargetAndSelectState() //플레이어와의 거리 측정후 상태 변경
    {
        if (target == null) return; // 목표가 없으면 리턴

        float distance = Vector2.Distance(target.position, transform.position); // 플레이어와 거리 측정

        if (distance <= detectRange) // 인지 범위에 들어온 경우
        {
            ChangeState(TurretState.Attack);
        }
        else if (distance >= detectRange) // 인지 범위 밖인 경우
        {
            ChangeState(TurretState.Idle);
        }
    }

    private IEnumerator Idle()
    {
        Debug.Log($"{gameObject.name}의 현재 상태 : {turret_State}");
        while (true)
        {
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }

    }

    private IEnumerator Attack()
    {
        Debug.Log($"{gameObject.name}의 현재 상태 : {turret_State}");
        while (true)
        {
            TurretAttack();
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
