using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMemoryPool : MonoBehaviour
{

    private MemoryPool enemyMemoryPool;
    [SerializeField]
    private float enemySpawnLatency;
    [SerializeField]
    private int numberOfEnemeisSpawnAtOnce; // 동시에 생성되는 적의 숫자

    private IEnumerator SpawnEnemy(GameObject point)
    {
        yield return new WaitForSeconds(enemySpawnLatency);

        GameObject item = enemyMemoryPool.ActivePoolItem();
        item.transform.position = point.transform.position;
    }
}
