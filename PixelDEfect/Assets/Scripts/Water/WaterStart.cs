using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterStart : MonoBehaviour
{
    [SerializeField]
    private WaterFlow waterFlow;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if(waterFlow != null)
            {
                waterFlow.StartFlow();
            }
        }
    }
}
