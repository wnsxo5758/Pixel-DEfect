using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV2 : InteractableObject
{

    Animator animator;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }
    public override void Trigger()
    { 
        if(isActive == false)
        {
            isActive = true;
            animator.SetTrigger("isOpen");
        }
    }


}
