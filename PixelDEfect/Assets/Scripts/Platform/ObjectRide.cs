using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRide : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 extraVelocity = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 아래쪽 발판에 닿아있을 때만
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) // 발 아래에 닿은 거라면
            {
                Rigidbody2D platformRb = collision.rigidbody;

                if (platformRb != null)
                {
                    extraVelocity = platformRb.velocity;
                    return;
                }
            }
        }

        extraVelocity = Vector2.zero;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        extraVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        // 추가 속도만큼 움직여주기 (플레이어 입력 외에 보정용)
        if (extraVelocity != Vector2.zero)
        {
            rb.position += extraVelocity * Time.fixedDeltaTime;
        }
    }
}
