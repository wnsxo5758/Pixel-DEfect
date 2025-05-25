using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBT : EnemyBT
{
    [Header("점프 관련")]
    [SerializeField] protected float jumpForce = 7f;
    [SerializeField] protected float maxJumpObstacleHeight = 5f;
    [SerializeField] protected float maxJumpGapWidth = 4f;
    [SerializeField] protected float gapcheckDistance = 3f;
    [SerializeField] protected LayerMask obstacleLayer;

    [Header("스킬 관련")]
    [SerializeField] protected float skillRange = 4f; // 스킬 사용 거리
    [SerializeField] protected float skillCooldown = 5f; // 

    protected bool canUseSkill = true;
    protected float skillTimer = 0f;



    ManaAnimator manaAnimator;

    protected override void Awake()
    {
        base.Awake();
        manaAnimator = GetComponentInChildren<ManaAnimator>();
    }
    protected override void Update()
    {
        base.Update();

        if(!canUseSkill)
        {
            skillTimer += Time.deltaTime;
            if(skillTimer >= skillCooldown)
            {
                skillTimer = 0f;
                canUseSkill = true;
            }
        }
    }

    protected virtual bool IsTargetInSkillRange()
    {
        Transform target = blackboard.GetValue<Transform>("Target");
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.position) <= skillRange;
    }

    protected override NodeState Patrol()
    {
        if (isHit || isDead) return NodeState.Failure;

        manaAnimator?.SetChasing(false);

        float direction = blackboard.GetValue<float>("PatrolDirection");

        if (float.IsNaN(direction) || direction == 0f)
        {
            direction = 1f;
            blackboard.SetValue("PatrolDirection", direction);
        }

        if (!IsGroundAhead(direction))
        {
            if (CanGapJump(direction) && movement.IsGrounded)
            {
                Debug.Log("순찰 중 낭떠러지 점프");
                movement.Jump();
            }
            else
            {
                direction *= -1;
                blackboard.SetValue("PatrolDirection", direction);
                SetDirection(direction);
                movement.MoveTo(0);
                return NodeState.Running;
            }
        }
        else if (CanClimbJump(direction) && movement.IsGrounded)
        {
            Debug.Log("순찰 중 장애물 점프");
            movement.Jump();
        }
        else if (CheckWall(direction))
        {
            direction *= -1;
            blackboard.SetValue("PatrolDirection", direction);
            SetDirection(direction);
        }

        movement.MoveTo(direction);
        manaAnimator?.SetMovement(Mathf.Abs(direction));
        return NodeState.Running;
    }


    protected override NodeState ChaseTarget()
    {
        if (isHit || isDead) return NodeState.Failure;

        Transform currentTarget = blackboard.GetValue<Transform>("Target");
        if (currentTarget == null) return NodeState.Failure;

        float direction = Mathf.Sign((currentTarget.position - transform.position).x);
        SetDirection(direction);

        // 낭떠러지
        if (!IsGroundAhead(direction))
        {
            if (CanGapJump(direction) && movement.IsGrounded)
            {
                Debug.Log("낭떠러지 점프");
                movement.Jump();
            }
            else
            {
                movement.MoveTo(0);
            }
            return NodeState.Running;
        }

        // 벽
        if (CanClimbJump(direction) && movement.IsGrounded)
        {
            Debug.Log("장애물 점프");
            movement.Jump();
            return NodeState.Running;
        }

        movement.MoveToFast(direction);
        return NodeState.Running;
    }

    protected virtual bool CanClimbJump(float direction)
    {
        Vector2 wallOrigin = (Vector2)transform.position + new Vector2(wallCheckOffset.x * direction, wallCheckOffset.y);
        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, Vector2.right * direction, wallCheckDistance, wallLayer);
        RaycastHit2D topHit = Physics2D.Raycast(wallOrigin, Vector2.up, maxJumpObstacleHeight, obstacleLayer);

        Debug.DrawRay(wallOrigin, Vector2.right * direction * wallCheckDistance, wallHit ? Color.red : Color.green);
        Debug.DrawRay(wallOrigin, Vector2.up * maxJumpObstacleHeight, topHit ? Color.red : Color.green);

        return wallHit.collider != null && topHit.collider == null;
    }

    protected virtual bool CanGapJump(float direction)
    {
        Vector2 gapStart = (Vector2)transform.position + new Vector2(gapcheckDistance * direction, 0) + groundCheckOffset;
        RaycastHit2D frontGround = Physics2D.Raycast(gapStart, Vector2.down, wallCheckDistance, groundLayer);

        Debug.DrawRay(gapStart, Vector2.down * wallCheckDistance, frontGround ? Color.green : Color.red);

        return frontGround.collider != null;
    }

    protected virtual bool IsGroundAhead(float direction)
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(groundCheckOffset.x * direction, groundCheckOffset.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, wallCheckDistance, groundLayer);

        Debug.DrawRay(origin, Vector2.down * wallCheckDistance, hit ? Color.green : Color.red);

        return hit.collider != null;
    }

    protected override void DetectTarget()
    {
        if (isHit || isDead) return;

        if (ShouldIgnoreTarget())
        {
            blackboard.SetValue("PlayerDetected", false);
            return;
        }

        base.DetectTarget();
    }

    protected virtual bool ShouldIgnoreTarget()
    {
        if (target == null) return false;

        // TODO: Player 상태를 체크하는 로직 삽입 예정
        return false;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        float dir = Application.isPlaying ? GetDirection() : 1f;

        Vector2 wallStart = (Vector2)transform.position + new Vector2(wallCheckOffset.x * dir, wallCheckOffset.y);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(wallStart, wallStart + Vector2.right * wallCheckDistance * dir);
        Gizmos.DrawLine(wallStart, wallStart + Vector2.up * maxJumpObstacleHeight);

        Vector2 gapStart = (Vector2)transform.position + new Vector2(gapcheckDistance * dir, 0) + groundCheckOffset;
        Vector2 landing = (Vector2)transform.position + new Vector2(maxJumpGapWidth * dir, groundCheckOffset.y);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(gapStart, gapStart + Vector2.down * wallCheckDistance);
        Gizmos.DrawLine(landing, landing + Vector2.down * wallCheckDistance);
    }

}