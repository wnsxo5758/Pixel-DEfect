using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    [SerializeField]
    private GameObject spawnEnemy; // 스폰될 wjr
    [SerializeField]
    private bool isActive;
    [SerializeField]
    private float delayTime; // 딜레이 시간

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            StartCoroutine(nameof(SpawnDelay));
        }
    }


    private IEnumerator SpawnDelay()
    {
        if(isActive == false)
        {
            isActive = true;
            yield return new WaitForSeconds(delayTime);
            Instantiate(spawnEnemy, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
