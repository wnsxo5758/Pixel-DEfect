using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState { None = -1, Idle = 0, PhaseTransition, Attack , Dead}
public abstract class BossFSM : MonoBehaviour
{
    [Header("보스 기본 설정")]
    [SerializeField]
    private int maxHp; // 최대체력
    [SerializeField]
    private int currentHp; // 현재 체력
    [SerializeField]
    private int phaseThreashod; // 페이즈 변경시 체력 비율
    [SerializeField]
    private int damage; // 보스의 공격력
    [SerializeField]
    protected Transform target; // 타겟(플레이어)

    protected BossState bossState = BossState.None;
    protected bool isPhaseTransiting = false;


    protected AudioSource audio;
    protected MovementRigidbody2D movement;

    protected abstract IEnumerator Idle();
    protected abstract IEnumerator Attack();
    protected abstract IEnumerator PhaseTransition();
    protected abstract IEnumerator Dead();

    protected virtual void Awake()
    {
        audio = GetComponent<AudioSource>();
        movement = GetComponent<MovementRigidbody2D>();
    }

    protected virtual void SetUp()
    {
        currentHp = maxHp;
        if (target == null)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
        ChangeState(BossState.Idle);
    }
    protected virtual void Start()
    {
        SetUp();
    }

    protected virtual void ChangeState(BossState newState)
    {
        if (bossState == newState) return;
        StopAllCoroutines();
        bossState = newState;
        StartCoroutine(newState.ToString());
    }

    public void TakeDamage(int _damage)
    {
        if (currentHp <= 0) return;
        currentHp -= _damage;

        if (currentHp <=0)
        {
            ChangeState(BossState.Dead);
        }
        else if (!isPhaseTransiting && (float )currentHp / maxHp <= phaseThreashod)
        {
            isPhaseTransiting=true;
            ChangeState(BossState.PhaseTransition);
        }
    }
}
