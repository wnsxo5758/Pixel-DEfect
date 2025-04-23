using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBT : MonoBehaviour
{
    private BehaviorTree behaviorTree;

    [Header("AI 설정")]
    [SerializeField] private float detectionRange; // 플레이어 인지 거리
    [SerializeField] private float pursuitLimitRange; // 추적 최대 거리
    [SerializeField] protected float attackRange; // 공격 사거리
    [SerializeField] private Transform target; // 플레이어 타깃

    private MovementRigidbody2D movement;
    private EnemyAnimator animator;

    private void Awake()
    {
        movement = GetComponent<MovementRigidbody2D>();
        animator = GetComponentInChildren<EnemyAnimator>();
        
        // Behavior Tree 구성
        SetupBehaviorTree();
    }

    private void SetupBehaviorTree()
    {
        // 루트 노드 (셀렉터)
        Selector rootSelector = new Selector();
        
        // 공격 시퀸스
        Sequence attackSequence = new Sequence();
        ConditionNode isInAttackRange = new ConditionNode(() => IsInAttackRange());
        ActionNode attackAction = new ActionNode(() => PerformAttack());
        attackSequence.AddChild(isInAttackRange);
        attackSequence.AddChild(attackAction);
        
        // 추적 시퀸스
        Sequence chaseSequence = new Sequence();
        ConditionNode isTargetVisible = new ConditionNode(() => IsTargetVisible());
        ActionNode chaseAction = new ActionNode(() => ChaseTarget());
        chaseSequence.AddChild(isTargetVisible);
        chaseSequence.AddChild(chaseAction);
        
        // 순찰 액션
        
        // 루트에 노드 추가 (우선순위 순서대로)
        rootSelector.AddChild(attackSequence);
        
        
        
        // Behavior Tree 생성
        behaviorTree = new BehaviorTree(rootSelector);
    }

    private void Update()
    {
        // Behavior Tree 평가
        behaviorTree.Evaluate();
    }
    
    // AI 액션 구현
    private bool IsInAttackRange()
    {
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.position) <= attackRange;
    }

    private bool IsTargetVisible()
    {
        if (target == null) return false;

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= detectionRange)
        {
            
        }

        return false;
    }

    private NodeState PerformAttack()
    {

        return NodeState.Success;
    }

    private NodeState ChaseTarget()
    {
        if (target == null) return NodeState.Failure;
        
        // 타겟을 향해 이동
        Vector2 direction = (target.position - transform.position).normalized;

        return NodeState.Running; // 계속 추적
    }

    private NodeState Patrol()
    {
        // 순찰 로직 구현

        return NodeState.Running;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, pursuitLimitRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
