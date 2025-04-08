using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDoor : InteractableObject
{

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }
    public override void Trigger()
    {
        isActive = !isActive;
        Active();
    }

    private void Active()
    {
        if(isActive == true) 
        {
            animator.SetBool("isActive", true);
        }
        else if (isActive == false)
        {
            animator.SetBool("isActive", false);
        }
    }
}
