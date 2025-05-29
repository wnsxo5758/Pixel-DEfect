using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMedicKit : ItemBase
{

    public override void UpdateCollision(Transform target)
    {
        Destroy(gameObject);
    }
}
