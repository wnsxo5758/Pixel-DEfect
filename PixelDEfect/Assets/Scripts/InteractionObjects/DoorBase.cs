using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBase : InteractableObject
{
    public override void Trigger()
    {
        isActive = !isActive;
        DoorActive();
    }

    private void DoorActive()
    {
        if(isActive == true)
        {
            Debug.Log("πÆ¿Ã ø≠∏≤");
        }
        else
        {
            Debug.Log("πÆ¿Ã ¥›»˚"); 
        }

    }
}
