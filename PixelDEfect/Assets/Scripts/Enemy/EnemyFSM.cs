using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState { None = -1, Idle = 0, Wander, Pursuit, Attack, Dead }
public class EnemyFSM : MonoBehaviour
{
    [SerializeField]
    private EnemyState enemyState = EnemyState.None;
    [SerializeField]
    private float waitTime; // Idle시 대기 시간
    [SerializeField]
    private float WanderTime; // 방황하는 시간 

    [SerializeField]
    private float distanceToDetect; //플레이어 인지 거리
    [SerializeField]
    private float pursuitLimitRange; // 추적최대치

    [SerializeField]
    private float distanceToAttack; // 공격하는 거리
    [SerializeField]
    private Transform target;


    private EnemyBase enemyBase;
    private MovementRigidbody2D movement;
    private EnemyAnimator animator;
    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        movement = GetComponent<MovementRigidbody2D>();
        animator = GetComponentInChildren<EnemyAnimator>();
        if (target == null)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
    }

    private void Start()
    {
        ChangeState(EnemyState.Idle);
    }
    public void SetUp(Transform _target)
    {
        this.target = _target;
    }
    private void OnDisable()
    {
        StopCoroutine(enemyState.ToString());
        enemyState = EnemyState.None;
    }
    public void ChangeState(EnemyState state) // State 변경시 사용
    {
        if (enemyState == state) return;

        StopCoroutine(enemyState.ToString()); // 이전의 행동 정지

        //새로운 상태 설정 후 실행
        enemyState = state;
        StartCoroutine(enemyState.ToString());
    }
    private IEnumerator Idle() // 정지(휴식)
    {
        movement.MoveTo(0);
        animator.UpdateAnimation(0);

        StartCoroutine(nameof(AutoChangeFromIdleToWander));
        while (true)
        {
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }

    }
    private IEnumerator Wander()  // 배회
    {
        float currentTime = 0;
        float maxTime = 5;
        float x = 0.5f;

        while (true)
        {
            currentTime += Time.deltaTime;
            if (enemyBase.IsFacingRight)
            {
                movement.MoveTo(x);
                animator.UpdateAnimation(x);
            }
            else
            {
                movement.MoveTo(-x);
                animator.UpdateAnimation(-x);
            }
            enemyBase.CheckWall();
            if (currentTime >= maxTime)
            {
                Debug.Log($"{gameObject.name}은 조금 쉬기로 했다");
                ChangeState(EnemyState.Idle);
            }
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }
    }


    private void LookRotationToTarget() //플레이어 감지시 플레이어 방향으로 전환
    {
        Vector2 dirToTarget = (target.position - transform.position).normalized;
        if (dirToTarget.x > 0) // 플레이어가 오른쪽에 있는 경우
        {
            if (enemyBase.IsFacingRight == false) { enemyBase.ChangeFacing(); }
        }
        else if (dirToTarget.x < 0)
        {
            if (enemyBase.IsFacingRight == true) { enemyBase.ChangeFacing(); }
        }
    }
    private void CalculateDistanceToTargetAndSelectState() //플레이어와의 거리 측정후 상태 변경
    {
        if (target == null) return; // 목표가 없으면 리턴

        float distance = Vector2.Distance(target.position, transform.position); // 플레이어와 거리 측정

        if (distance <= distanceToAttack) // 공격 범위에 들어온 경우
        {
            ChangeState(EnemyState.Attack);
        }
        else if (distance <= distanceToDetect) // 감지 범위에 들어온 경우
        {
            Debug.Log($"{gameObject.name}은 플레이어 감지했다! : {enemyState}");
            ChangeState(EnemyState.Pursuit);

        }
        else if (distance >= pursuitLimitRange) // 추적 범위에서 벗어난 경우
        {
            ChangeState(EnemyState.Wander);
        }
    }

    private IEnumerator AutoChangeFromIdleToWander() // 일정 시간이 지난 뒤, 배회상태로 변경
    {

        int changeTime = Random.Range(1, 5);

        yield return new WaitForSeconds(changeTime);

        ChangeState(EnemyState.Wander);
    }




    private IEnumerator Pursuit() // 추적
    {
        Debug.Log($"{gameObject.name}은 플레이어를 향해 이동중");

        float speed;

        while (true)
        {
            LookRotationToTarget();
            if (enemyBase.IsFacingRight)
            {
                speed = 1;
            }
            else
            {
                speed = -1;
            }
            movement.MoveTo(speed);
            animator.UpdateAnimation(speed);
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }
    }

    private IEnumerator Attack()
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // 목표 인식 거리
        Gizmos.DrawWireSphere(transform.position, distanceToDetect);

        Gizmos.color = Color.blue; // 추적최대치 거리
        Gizmos.DrawWireSphere(transform.position, pursuitLimitRange);


        Gizmos.color = Color.red; // 공격범위
        Gizmos.DrawWireSphere(transform.position, distanceToAttack);

    }

}
