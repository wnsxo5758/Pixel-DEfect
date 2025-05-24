using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerBT : ManaBT
{
    [Header("�ظ� �� ���� ����")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 2;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    [SerializeField] private Vector2 attackBoxOffset = new Vector2(0.8f, 0);
    [SerializeField] private LayerMask playerLayer;

    private float attackTimer = 0f;
    private bool canAttack = true;
    private bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();
        blackboard.SetValue("AttackRange", attackRange);
        blackboard.SetValue("CanAttack", true);
        blackboard.SetValue("IsAttacking", false);
    }

    protected override void Update()
    {
        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                canAttack = true;
                attackTimer = 0f;
                blackboard.SetValue("CanAttack", true);
            }
        }

        base.Update();
    }

    protected override Node CreateAttackSequence()
    {
        Sequence attackSequence = new Sequence();

        attackSequence.AddChild(new ConditionNode(() => !blackboard.GetValue<bool>("IsHit")));
        attackSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("PlayerDetected")));
        attackSequence.AddChild(new ConditionNode(IsInAttackRange));
        attackSequence.AddChild(new ConditionNode(() => blackboard.GetValue<bool>("CanAttack")));
        attackSequence.AddChild(new ConditionNode(() => !blackboard.GetValue<bool>("IsAttacking")));
        attackSequence.AddChild(new ActionNode(PerformAttack));

        return attackSequence;
    }

    private bool IsInAttackRange()
    {
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.position) <= attackRange;
    }

    protected override bool IsTargetInAttackRange() => IsInAttackRange();
    protected override float GetAttackRange() => attackRange;

    private NodeState PerformAttack()
    {
        if (isHit || isDead) return NodeState.Failure;

        canAttack = false;
        isAttacking = true;
        attackTimer = 0f;

        blackboard.SetValue("CanAttack", false);
        blackboard.SetValue("IsAttacking", true);

        Transform target = blackboard.GetValue<Transform>("Target");
        float dir = Mathf.Sign(target.position.x - transform.position.x);
        SetDirection(dir);

        movement?.MoveTo(0);

        animator?.SetMovementAnim(0);
        animator?.SetChasingState(true);
        animator?.TriggerAttackAnim();

        return NodeState.Running;
    }

    public override void OnAttackAnimationEvent()
    {
        if (isHit) return;
        DealDamage();
    }

    public override void OnAttackAnimationFinished()
    {
        isAttacking = false;
        blackboard.SetValue("IsAttacking", false);

        if (IsInAttackRange())
        {
            Transform target = blackboard.GetValue<Transform>("Target");
            if (target != null)
            {
                float dir = Mathf.Sign(target.position.x - transform.position.x);
                SetDirection(dir);
                movement?.MoveTo(0);
            }
        }
    }

    private void DealDamage()
    {
        float dir = GetDirection();
        Vector2 attackPos = attackPoint != null
            ? attackPoint.position
            : (Vector2)transform.position + new Vector2(attackBoxOffset.x * dir, attackBoxOffset.y);

        Collider2D hitPlayer = Physics2D.OverlapBox(attackPos, attackBoxSize, 0, playerLayer);

        if (hitPlayer != null)
        {
            PlayerHp hp = hitPlayer.GetComponent<PlayerHp>();
            if (hp != null)
            {
                DeathData deathData = new DeathData(DeathCause.MeleeAttack, dir);
                hp.DecreaseHp(attackDamage, deathData,true, true);
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        float dir = Application.isPlaying ? GetDirection() : Mathf.Sign(transform.localScale.x);
        Vector2 attackPos = attackPoint != null
            ? attackPoint.position
            : (Vector2)transform.position + new Vector2(attackBoxOffset.x * dir, attackBoxOffset.y);
        Gizmos.DrawWireCube(attackPos, attackBoxSize);
    }
}
