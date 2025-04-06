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

    public override void Trigger()
    {
        isActive = !isActive;
        // 방향전환
        if(canChangeDir)
        {
            moveDir *= -1f;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("충돌 중: " + collision.gameObject.name);
        if (!isActive) return;

        if (!collision.collider.CompareTag("Player") && !collision.collider.CompareTag("Enemy")) return;

        Rigidbody2D rigid = collision.rigidbody;
        if (rigid != null)
        {
            Vector2 newVelocity = rigid.velocity;
            newVelocity.x = moveDir.normalized.x * speed;
            rigid.velocity = newVelocity;
        }

    }

}
