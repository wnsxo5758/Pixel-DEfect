using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeAnimator : MonoBehaviour
{

    public bool isAttack;
    public float DeathAnimLength;

    Animator animator;
    MovementRigidbody2D movement;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<MovementRigidbody2D>();
    }
    public void UpdateAnimation(float x)
    {
        animator.SetBool("isAttack", isAttack);
        if (x != 0)
        {
            SpriteFlipX(x);
        }

        if(movement.IsGrounded)
        {
            animator.SetFloat("velocityX", Mathf.Abs(x));
        }
    }

    public void Death()
    {
        Debug.Log("¹ìÀº ¾ÈµÚÁ®");
    }
    private void SpriteFlipX(float x)
    {
        transform.parent.localScale = new Vector3((x < 0 ? -1 : 1), 1, 1);
    }
}
