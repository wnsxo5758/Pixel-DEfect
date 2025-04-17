using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorController : InteractableObject
{
    [Header("엘리베이터 기본 설정")]
    [SerializeField]
    private Transform movePos; // 엘리베이터가 갈 위치
    [SerializeField]
    private float moveSpeed; // 엘리베이터 속도

    [Header("엘리베이터 버튼")]
    [SerializeField]
    private Transform internalInteractionZone; //

    private Vector3 startPos; // 시작지점

    private bool isMoving = false;

    private void Awake()
    {
        startPos = transform.position;
    }

    public void ActivateElevator()
    {
        if (isMoving) return;

    }

    private IEnumerator MoveElevator(Transform destination)
    {
        isMoving = true;
        while(Vector2.Distance(transform.position,destination.position) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, destination.position, moveSpeed);
            yield return null;
        }
        transform.position = destination.position; 
        isMoving = false;
    }

    public override void Trigger()
    {

    }

    public void Move()
    {
        StartCoroutine(MoveElevator(movePos));
    }
}
