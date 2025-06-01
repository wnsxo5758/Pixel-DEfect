using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;        // 스폰할 프리팹
    public Transform spawnPoint;            // 스폰 위치
    public string targetTag = "Player";     // A 오브젝트의 태그 

    private bool isTouching = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            isTouching = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            isTouching = false;
            Spawn();
        }
    }

    private void Spawn()
    {
        Instantiate(objectToSpawn, spawnPoint.position, Quaternion.identity);
    }
}
