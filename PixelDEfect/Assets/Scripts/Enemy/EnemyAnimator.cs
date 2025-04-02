using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;
    private MovementRigidbody2D movement; // 움직임
    public bool isAttack;
    private bool isDeath; // 사망시
    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<MovementRigidbody2D>();
    }

    public void UpdateAnimation(float x)
    {
        if (isDeath == true) return;  // 사망시 애니메이션 작동 X

        if (x != 0)
        {
            SpriteFlipX(x);
        }
        if(isAttack)
        {
            animator.SetBool("isAttack", true);
        }
        else if(!isAttack)
        {
            animator.SetBool("isAttack", false);
        }
        animator.SetBool("isJump", !movement.IsGrounded); // 땅에 닿은 상태가 아닌 경우
        if (movement.IsGrounded) // 땅에 있는 경우
        {
            animator.SetFloat("velocityX", Mathf.Abs(x)); // X값에 따라 이동 상태 변환
        }
        else // 땅에 없는 경우
        {
            animator.SetFloat("velocityY", movement.Velocity.y);  // Y 값에 따라 상태 변경
        }
    }
    public void Attack()
    {
        animator.SetBool("isAttack", true);
    }

    public void Death()
    {
        animator.SetTrigger("isDead"); // 사망시 값 설정
        isDeath = true;
    }

    private void SpriteFlipX(float x) // 방향전환
    {
        transform.parent.localScale = new Vector3((x < 0 ? -1 : 1), 1, 1);
    }

}
