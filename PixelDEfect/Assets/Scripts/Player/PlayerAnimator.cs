using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{

    private Animator animator; // 애니메이션 
    private MovementRigidbody2D movement; // 움직임
    private PlayerAttack attack; // 플레이어 공격

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<MovementRigidbody2D>();
        attack = GetComponentInParent<PlayerAttack>();
    }

    public void UpdateAnimation(float x)
    {
        if (x != 0)
        {
            SpriteFlipX(x);
        }
        animator.SetBool("isJump", !movement.IsGrounded); // 땅에 닿은 상태가 아닌 경우

        if (movement.IsGrounded)
        {
            animator.SetFloat("velocityX", Mathf.Abs(x)); // X 값에 따라 변경
        }
        else
        {
            animator.SetFloat("velocityY", movement.Velocity.y); // Y 값에 따라 변경 -> y가 작으면 공중에서 내려가는 모션, 높으면 올라가는 모션
        }

        if (attack.IsMelee)
        {
            animator.SetBool("isAttack", attack.IsMelee); // 공격 중인지에 따라 애니메이션 변경
        }
    }
    private void SpriteFlipX(float x)
    {
        transform.parent.localScale = new Vector3((x < 0 ? -1 : 1), 1, 1);
    }
}
