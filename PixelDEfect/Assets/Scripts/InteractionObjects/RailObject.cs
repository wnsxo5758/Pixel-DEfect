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
        isActive = !isActive;
        animator.SetBool("isActive", isActive);
        // 방향전환
        if(canChangeDir)
        {
            moveDir *= -1f;
        }
    }

    private void Awake()
    {
        animator.SetBool("isActive", isActive);
        animator = GetComponentInChildren<Animator>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("레일과 충돌 중 : " + collision.gameObject.name);
        if (!isActive) return;

        if (!collision.collider.CompareTag("Player") && !collision.collider.CompareTag("Enemy")) return;

        Rigidbody2D rigid = collision.rigidbody;
        if (rigid != null)
        {
            rigid.AddForce(moveDir.normalized * speed, ForceMode2D.Impulse);
        }

    }

}
