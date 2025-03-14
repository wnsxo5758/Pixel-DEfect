using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    [SerializeField]
    private float aliveTimeAfterSpawn = 5f; // 
    public void SetUp()
    {
        StartCoroutine(nameof(SpawnItemProcess));
    }

    private IEnumerator SpawnItemProcess()
    {
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UpdateCollision(collision.transform);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            UpdateCollision(collision.transform);
        }
    }

    public abstract void UpdateCollision(Transform target);
}
