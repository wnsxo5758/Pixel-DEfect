using UnityEngine;

public class MovingObjectTimeAffected : TimeAffectedEntity
{
    protected Rigidbody2D rb;
    protected Animator animator;
    
    // 저장된 상태
    protected Vector2 originalVelocity;
    protected float originalAngularVelocity;
    protected float originalAnimatorSpeed;
    protected bool wasKinematic;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    public override void OnTimeStop()
    {
        base.OnTimeStop();
        
        // 물리 객체 상태 저장 및 정지
        if (rb != null)
        {
            originalVelocity = rb.velocity;
            originalAngularVelocity = rb.angularVelocity;
            wasKinematic = rb.isKinematic;

            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = true;
        }
        
        // 애니메이션 정지
        if (animator != null && animator.enabled)
        {
            originalAnimatorSpeed = animator.speed;
            animator.speed = 0;
        }
    }

    public override void OnTimeResume()
    {
        base.OnTimeResume();
        
        // 물리 상태 복원
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            if (!wasKinematic)
            {
                rb.velocity = originalVelocity;
                rb.angularVelocity = originalAngularVelocity;
            }
        }
        
        // 애니메이션 복원
        if (animator != null && animator.enabled)
        {
            animator.speed = originalAnimatorSpeed;
        }
    }
}
