using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorButton : ButtonBase
{
    [SerializeField] private ElevatorController elevator; // 연결된 엘리베이터
    [SerializeField] private bool isUpperFloorButton;     // 위층 버튼 여부

    public override void ButtonTrigger()
    {
        // 이미 작동 중이거나 엘리베이터가 목적 위치에 있으면 작동 안 함
        if (elevator == null || elevator.IsMoving) return;

        bool isAtThisFloor = isUpperFloorButton == elevator.IsAtUpperFloor;
        if (isAtThisFloor) return;

        StartCoroutine(ButtonActive()); // 공통 작동 루틴
    }

    protected override IEnumerator ButtonActive()
    {
        isActiving = true;
        isActive = true;
        UpdateSprite();
        audioSoruce.Play();

        // 엘리베이터 작동
        elevator.ActivateElevator();

        yield return new WaitForSeconds(1.0f);
        isActiving = false;
    }
}
