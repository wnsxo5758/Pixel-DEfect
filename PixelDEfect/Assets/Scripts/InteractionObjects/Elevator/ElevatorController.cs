using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorController : InteractableObject
{
    [Header("엘리베이터 기본 설정")]
    [SerializeField]
    private Transform upFloorTransform; // 위층 위치
    [SerializeField]
    private float moveSpeed; // 엘리베이터 속도

    [Header("엘리베이터 내부 버튼")]
    [SerializeField]
    private ElevatorInternalButton internalButton;
 
    private Transform startPos; // 아래층 위치
    private bool isAtUpperFloor = false; //위층인가?
    private bool isMoving = false; // 움직이는 중인가?

    public bool IsAtUpperFloor => isAtUpperFloor;
    public bool IsMoving => isMoving;

    private void Awake()
    {
        startPos = transform;
    }

    public void ActivateElevator()
    {
        if (isMoving) return;

        Vector3 destination = isAtUpperFloor ? startPos.position : upFloorTransform.position;
        StartCoroutine(MoveElevator(destination));
    }

    private IEnumerator MoveElevator(Vector3 destination)
    {
        isMoving = true;
        if (internalButton != null)
            internalButton.SetInteractable(false);

        while (Vector2.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
        isAtUpperFloor = !isAtUpperFloor;
        isMoving = false;
    }

    // 외부 버튼용 Trigger
    public override void Trigger()
    {
        if (isMoving) return;

        // 외부 버튼에서는 엘리베이터가 현재 위치에 없을 때만 작동
        if ((!isAtUpperFloor && transform.position == startPos.position) ||
            (isAtUpperFloor && transform.position == upFloorTransform.position))
        {
            ActivateElevator();
        }

        ActivateElevator();
    }

    // 내부 버튼에서 호출할 메서드
    public void PressInternalButton()
    {
        if (!isMoving)
        {
            ActivateElevator();
        }
    }

}
