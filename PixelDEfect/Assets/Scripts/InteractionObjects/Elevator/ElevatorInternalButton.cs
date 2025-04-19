using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorInternalButton : ButtonBase
{

    [SerializeField]
    private ElevatorController elevator; // 연결된 엘리베이터

    public override void ButtonTrigger()
    {
        if (elevator == null) return;
        if (elevator.IsMoving) return;

        elevator.ActivateElevator();
    }

    public void SetInteractable(bool active)
    {
        isActive = active;
        UpdateSprite();
        GetComponent<Collider2D>().enabled = active;
    }
}
