using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    [Header("엘리베이터 설정")]
    [SerializeField] private Transform upFloorTransform;          // 위층 위치
    [SerializeField] private float moveSpeed = 2f;                // 이동 속도
    [SerializeField] private ElevatorButton internalButton; // 내부 버튼 참조

    private Vector3 downFloorPosition; // 아래층 위치
    private bool isAtUpperFloor = false;
    [SerializeField]
    private bool isMoving = false;

    public bool IsAtUpperFloor => isAtUpperFloor;
    public bool IsMoving => isMoving;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        downFloorPosition = transform.position;
    }

    public void RequestMove()
    {
        Debug.Log("이동 명령받음");
        if (isMoving) return;
        Vector3 destination = isAtUpperFloor ? downFloorPosition : upFloorTransform.position;
        StartCoroutine(MoveElevator(destination));
    }

    private IEnumerator MoveElevator(Vector3 destination)
    {
        isMoving = true;
        animator.SetBool("isMove", true);
        // 내부 버튼 비활성화
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

        // 도착 후 내부 버튼 다시 활성화
        if (internalButton != null)
            animator.SetBool("isMove", false);
        internalButton.SetInteractable(true);
    }
}
