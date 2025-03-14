using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlatformBase : MonoBehaviour
{
    public bool IsHit { protected set; get; } = false;

    public abstract void UpdateCollision(GameObject other);
}
