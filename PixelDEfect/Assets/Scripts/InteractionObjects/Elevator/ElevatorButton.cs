using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorButton : ButtonBase
{
    [SerializeField]
    private ElevatorController elevator;
    [SerializeField] 
    private bool isUpperFloorButton; // true = 위층 버튼, false = 아래층 버튼
    [SerializeField] 
    private bool isInsideButton;     // true = 내부 버튼

    public override void ButtonTrigger()
    {
        Debug.Log("엘리베이터 버튼이 눌림");
        if (elevator == null || elevator.IsMoving)
            return;
        if (isInsideButton)
        {
            // 내부 버튼은 현재 층과 반대로 이동
            StartCoroutine(ButtonActive());
        }
        else
        {
            // 외부 버튼은 버튼 위치와 엘리베이터 위치가 다를 때만
            bool isAtThisFloor = isUpperFloorButton == elevator.IsAtUpperFloor;
            if (isAtThisFloor) return;

            StartCoroutine(ButtonActive());
        }
    }

    protected override IEnumerator ButtonActive()
    {
        isActiving = true;
        isActive = true;
        UpdateSprite();
        audioSoruce.Play();

        elevator.RequestMove();

        yield return new WaitForSeconds(1.0f);
        isActiving = false;
    }

    public void SetInteractable(bool active)
    {
        isActive = active;
        UpdateSprite();
        GetComponent<Collider2D>().enabled = active;
    }
}
