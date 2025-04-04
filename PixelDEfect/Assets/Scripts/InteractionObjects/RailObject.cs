using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
            if(isActive)
            {
                moveDir = Vector2.left;
            }
            else
            {
                moveDir = Vector2.right;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //방향전환이 불가능하고, isActive가 false라면
        if (!canChangeDir && !isActive ) return;

        Rigidbody2D rigid = collision.attachedRigidbody;
        if(rigid != null)
        {
            rigid.AddForce(moveDir.normalized * speed, ForceMode2D.Force);
        }
    }
}
