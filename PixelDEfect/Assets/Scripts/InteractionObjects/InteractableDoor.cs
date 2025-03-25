using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDoor : InteractableObject
{
    public override void Trigger()
    {
        isActive = !isActive;
        Active();
    }

    private void Active()
    {
        if(isActive == true) 
        {
            gameObject.SetActive(false);
        }
        else if (isActive == false)
        {
            gameObject.SetActive(true);
        }
    }
}
