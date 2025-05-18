using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageDoor : InteractableObject
{
    public GameObject[] targets;        // 비활성화 여부를 확인할 대상들

    private Animator animator;
    private bool doorOpened = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!doorOpened && CheckAllTargetsInactive())
        {
            isActive = true;
            Active();
            doorOpened = true;          // 한 번만 열리게 방지
        }
    }

    private bool CheckAllTargetsInactive()
    {
        foreach (GameObject obj in targets)
        {
            if (obj != null && obj.activeSelf)
            {
                return false;           // 하나라도 활성화되어 있으면 아직 X
            }
        }
        return true;                    // 전부 비활성화됨
    }

    public override void Trigger()
    {
        isActive = !isActive;
        Active();
    }

    private void Active()
    {
        animator.SetBool("isActive", isActive);
    }
}
