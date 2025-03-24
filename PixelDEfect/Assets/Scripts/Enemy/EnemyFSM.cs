using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState { None = -1, Idle = 0, Wander, Pursuit, Attack, Dead }
public abstract class EnemyFSM : MonoBehaviour
{



    [Header("기본 설정")]
    [SerializeField]
    private int currentHp; // 현재 체력
    [SerializeField]
    private int maxHp; // 최대체력
    [SerializeField]
    private float distanceToDetect; //플레이어 인지 거리
    [SerializeField]
    private float pursuitLimitRange; // 추적최대치
    [SerializeField]
    protected float distanceToAttack; // 공격하는 거리
    [SerializeField]
    private float checkWallDistance; // 벽 확인 거리
    [SerializeField]
    private LayerMask wallLayer; // 벽 레이어

    private bool isFacingRight = true; // true인 경우 우측을 보는 중

    [Header("효과음")]
    [SerializeField]
    private AudioClip hitClip; // 피격시 효과음
    [SerializeField]
    private AudioClip deadClip; // 사망시 효과음 
    [SerializeField]
    private AudioClip attackClip; // 공격 효과음

    [Header("사망시 보상")]
    [SerializeField]
    private GameObject coins; // 사망시 드랍하는 코인
    [SerializeField]
    private int amount; // 드랍하는 코인의 최댓값

    protected EnemyState enemyState = EnemyState.None;
    [SerializeField]
    private float waitTime; // Idle시 대기 시간
    [SerializeField]
    private float WanderTime; // 방황하는 시간 

    [SerializeField]
    protected Transform target;

    protected MovementRigidbody2D movement;
    protected EnemyAnimator animator;
    protected AudioSource audio;
    private void Awake()
    {
        movement = GetComponent<MovementRigidbody2D>();
        animator = GetComponentInChildren<EnemyAnimator>();
    }

    private void Start()
    {
        SetUp();
    }

    private Vector2 dir;

    private bool isChange;

    public bool IsFacingRight => isFacingRight;


    public void ChangeFacing()
    {
        if (isChange == true)
        {
            return;
        }

        StartCoroutine(nameof(Changing));

    }

    private IEnumerator Changing()
    {
        isChange = true;
        isFacingRight = !isFacingRight;
        Debug.Log("돌았다!!!");
        yield return new WaitForSeconds(1f);
        isChange = false;
    }
    public bool CheckWall()
    {
        dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, checkWallDistance, wallLayer);
        if (hit.collider != null)
        {
            if (!isChange)
            {
                Debug.Log($"벽 감지 : + {hit.collider.name}");
                ChangeFacing();
            }
            return true;
        }
        return false;
    }


    private void SetUp()
    {
        currentHp = maxHp;
        if (target == null)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
        ChangeState(EnemyState.Idle);
    }

    private void OnDisable() // 비활성화될 경우
    {
        StopCoroutine(enemyState.ToString());
        enemyState = EnemyState.None;
    }
    protected virtual void ChangeState(EnemyState state) // State 변경시 사용
    {
        if (enemyState == state) return;

        StopCoroutine(enemyState.ToString()); // 이전의 행동 정지

        //새로운 상태 설정 후 실행
        enemyState = state;
        StartCoroutine(enemyState.ToString());
    }
    protected virtual IEnumerator Idle() // 정지(휴식)
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
    protected virtual IEnumerator Wander()  // 배회
    {
        float currentTime = 0;
        float maxTime = 5;
        float x = 0.5f;

        while (true)
        {
            currentTime += Time.deltaTime;
            if (IsFacingRight)
            {
                movement.MoveTo(x);
                animator.UpdateAnimation(x);
            }
            else
            {
                movement.MoveTo(-x);
                animator.UpdateAnimation(-x);
            }
            CheckWall();
            if (currentTime >= maxTime)
            {
                Debug.Log($"{gameObject.name}은 조금 쉬기로 했다");
                ChangeState(EnemyState.Idle);
            }
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }
    }


    protected void LookRotationToTarget() //플레이어 감지시 플레이어 방향으로 전환
    {
        Vector2 dirToTarget = (target.position - transform.position).normalized;
        if (dirToTarget.x > 0) // 플레이어가 오른쪽에 있는 경우
        {
            if (IsFacingRight == false) { ChangeFacing(); }
        }
        else if (dirToTarget.x < 0)
        {
            if (IsFacingRight == true) { ChangeFacing(); }
        }
    }
    protected virtual void CalculateDistanceToTargetAndSelectState() //플레이어와의 거리 측정후 상태 변경
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
    protected virtual IEnumerator Pursuit() // 추적
    {
        Debug.Log($"{gameObject.name}은 플레이어를 향해 이동중");

        float speed;

        while (true)
        {
            LookRotationToTarget();
            if (IsFacingRight)
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
    protected virtual IEnumerator Dead() // 사망
    {

        animator.isDead(); // 적 사망 애니메이션 
        yield return null;
    }
    protected abstract IEnumerator Attack(); // 하위 객체에서 공격 구현

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // 목표 인식 거리
        Gizmos.DrawWireSphere(transform.position, distanceToDetect);

        Gizmos.color = Color.blue; // 추적최대치 거리
        Gizmos.DrawWireSphere(transform.position, pursuitLimitRange);


        Gizmos.color = Color.red; // 공격범위
        Gizmos.DrawWireSphere(transform.position, distanceToAttack);

        Gizmos.color = Color.black; // 벽확인용
        Gizmos.DrawRay(transform.position, dir * checkWallDistance);


    }
    public void TakeDamage(int _damage)
    {
        if (currentHp > 0)
        {
            currentHp -= _damage;
            PlaySound(hitClip);
            if (currentHp <= 0)
            {
                Debug.Log($"{gameObject.name}가 사망");
            }
        }
    }
    public void IncreaseHp(int _amount)
    {
        if (currentHp < maxHp)
        {
            currentHp += _amount;
            if (currentHp > maxHp)
            {
                currentHp = maxHp;
            }
        }
    }
    private void PlaySound(AudioClip clip)
    {
        audio.Stop();
        audio.clip = clip;
        audio.Play();
    }

}
