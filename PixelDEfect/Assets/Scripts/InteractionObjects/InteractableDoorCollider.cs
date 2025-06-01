using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDoorCollider : InteractableObject
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isActive = true;
            Active();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isActive = false;
            Active();
        }
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
