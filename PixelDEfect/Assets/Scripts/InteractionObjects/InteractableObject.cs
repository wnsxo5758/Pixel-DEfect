using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    [SerializeField]
    protected bool isActive;
    public abstract void Trigger(); // È°¼ºÈ­
}
