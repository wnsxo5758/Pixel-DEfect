using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileLoop : MonoBehaviour
{
    [Header("위치 설정")]
    public Transform pointA;       // 이동할 위치 A
    public Transform pointB;       // 순간이동 위치 B

    [Header("이동 속도")]
    public float moveSpeed = 2f;   // A까지 이동 속도

    private bool movingToA = true;
    private Vector3 targetPos;

    void Start()
    {
        if (pointA != null)
        {
            targetPos = pointA.position;
        }
    }

    void Update()
    {
        if (movingToA)
        {
            // A 위치로 천천히 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            // A에 도착하면
            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                // 순간이동 → B 위치
                if (pointB != null)
                {
                    transform.position = pointB.position;
                }

                // 다시 A로 향하도록 설정
                targetPos = pointA.position;
            }
        }
    }
}
