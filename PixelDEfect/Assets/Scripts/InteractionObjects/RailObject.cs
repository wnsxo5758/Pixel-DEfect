using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailObject : InteractableObject
{

    [SerializeField]
    private Vector2 moveDir = Vector2.right; // 기본적으로 오른쪽
    [SerializeField]
    private bool canChangeDir;
    [SerializeField]
    private float speed; // 움직이는 속도

    private Animator animator;

    public override void Trigger()
    {

        if(canChangeDir)
        {
            moveDir *= -1f;
            bool right = moveDir.x > 0f;
            animator.SetBool("isRight", right);
        }
        else
        {
            isActive = !isActive;
            animator.SetBool("isActive", isActive);
            // 방향전환
        }
    }

    private void Awake()
    {

        animator = GetComponentInChildren<Animator>();

        animator.SetBool("isActive", isActive);
        bool right = moveDir.x > 0f;
        animator.SetBool("isRight", right);
    }



    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("레일과 충돌 중 : " + collision.gameObject.name);
        if (!isActive) return;

        if (!collision.collider.CompareTag("Player") && !collision.collider.CompareTag("Enemy") && !collision.collider.CompareTag("Objects")) return;

        Rigidbody2D rigid = collision.rigidbody;
        if (rigid != null)
        {
            rigid.AddForce(moveDir.normalized * speed, ForceMode2D.Impulse);
        }

    }

}
