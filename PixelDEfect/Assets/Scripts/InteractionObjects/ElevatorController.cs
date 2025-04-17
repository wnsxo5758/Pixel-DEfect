using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorController : InteractableObject
{
    [Header("엘리베이터 기본 설정")]
    [SerializeField] private Transform floorTransform; // 위층 위치
    [SerializeField] private float moveSpeed = 2f;

    [Header("엘리베이터 내부 버튼 설정")]
    [SerializeField] private SpriteRenderer internalButtonSprite;
    [SerializeField] private Collider2D internalButtonCollider;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite inactiveSprite;

    private Vector3 startPosition; // 아래층 위치
    private bool isAtUpperFloor = false;
    private bool isMoving = false;

    public bool IsAtUpperFloor => isAtUpperFloor;
    public bool IsMoving => isMoving;

    private void Awake()
    {
        startPosition = transform.position;
        UpdateInternalButtonState(true); // 시작 시 내부 버튼 활성화
    }

    public void ActivateElevator()
    {
        if (isMoving) return;

        Transform destination = isAtUpperFloor ? GetStartTransform() : floorTransform;
        StartCoroutine(MoveElevator(destination));
    }

    private IEnumerator MoveElevator(Transform destination)
    {
        isMoving = true;
        UpdateInternalButtonState(false); // 이동 중엔 내부 버튼 비활성화

        while (Vector2.Distance(transform.position, destination.position) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, destination.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination.position;
        isAtUpperFloor = !isAtUpperFloor;
        isMoving = false;

        UpdateInternalButtonState(true); // 도착 후 다시 활성화
    }

    // 외부 버튼용 Trigger
    public override void Trigger()
    {
        if (isMoving) return;

        // 외부 버튼에서는 엘리베이터가 현재 위치에 없을 때만 작동
        if ((!isAtUpperFloor && transform.position != startPosition) ||
            (isAtUpperFloor && transform.position != floorTransform.position))
        {
            return;
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

    // 내부 버튼 상태를 스프라이트 및 콜라이더로 제어
    private void UpdateInternalButtonState(bool active)
    {
        if (internalButtonSprite != null)
        {
            internalButtonSprite.sprite = active ? activeSprite : inactiveSprite;
        }

        if (internalButtonCollider != null)
        {
            internalButtonCollider.enabled = active;
        }
    }

    // 아래층 위치 Transform 반환용 (캐싱 또는 생성)
    private Transform GetStartTransform()
    {
        GameObject temp = new GameObject("StartPositionTemp");
        temp.transform.position = startPosition;
        return temp.transform;
    }
}
