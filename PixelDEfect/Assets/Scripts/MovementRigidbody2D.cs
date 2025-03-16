using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementRigidbody2D : MonoBehaviour
{
    [Header("레이어 마스크")]
    [SerializeField]
    private LayerMask groundCheckLayer;
    
    [Header("움직임")]
    [SerializeField]
    private float walkSpeed; // 걷기 속도
    [SerializeField]
    private float runSpeed; // 달리기 속도
    [SerializeField]
    private float jumpForce; // 점프력
    [SerializeField]
    private float lowGravityScale; // 약한 중력 (높은 점프시)
    [SerializeField]
    private float highGravityScale; // 강한 중력 (일반 점프시)
   
    private float moveSpeed; // 현재 움직이는 속도
    
    private Vector2 collisionSize; // 
    private Vector2 footPos; // 발 위치

    private Rigidbody2D rigid;
    private Collider2D collider;

    public bool IsLongJump { set; get; } = false;
    public bool IsGrounded { private set; get; } = false;

    public Vector2 Velocity => rigid.velocity;

    private void Awake()
    {
        moveSpeed = walkSpeed;
        rigid = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        UpdateCollision();
        JumpHeight();
    }

    public void MoveTo(float x)
    {
        moveSpeed = Mathf.Abs(x) != 1 ? walkSpeed : runSpeed;
        if (x != 0) x = Mathf.Sign(x);
        rigid.velocity = new Vector2(x * moveSpeed, rigid.velocity.y);
    }

    private void UpdateCollision()
    {
        Bounds bounds = collider.bounds;

        collisionSize = new Vector2((bounds.max.x - bounds.min.x) * 0.5f, 0.1f);
        footPos = new Vector2(bounds.center.x, bounds.min.y);

        IsGrounded = Physics2D.OverlapBox(footPos, collisionSize, 0, groundCheckLayer);
    }

    public void Jump() //점프
    {
        if (IsGrounded)
        {
            rigid.velocity = new Vector2(rigid.velocity.x, jumpForce);
        }
    }

    private void JumpHeight() // 점프시 높이
    {
        if (IsLongJump && rigid.velocity.y > 0)
        {
            rigid.gravityScale = lowGravityScale;
        }
        else
        {
            rigid.gravityScale = highGravityScale;
        }
    }
}
