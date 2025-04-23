using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ImpactType { Normal = 0, Obstacle, }
public class ImpactMemoryPool : MonoBehaviour
{

    [SerializeField]
    private GameObject[] impactPrefab;
    private MemoryPool[] memoryPools;

    private void Awake()
    {
        memoryPools = new MemoryPool[impactPrefab.Length];
        for(int i =0; i < impactPrefab.Length; ++i)
        {
            memoryPools[i] = new MemoryPool(impactPrefab[i]);
        }
    }

    public void SpawnImpact(RaycastHit2D hit)
    {
        if(hit.transform.CompareTag("ImpactNormal"))
        {
            OnSpawnImpact(ImpactType.Normal, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }



    public void OnSpawnImpact(ImpactType type, Vector2 pos, Quaternion rotation)
    {
        GameObject item = memoryPools[(int)type].ActivePoolItem();
        item.transform.position = pos;
        item.transform.rotation = rotation;
        item.GetComponent<Impact>().SetUp(memoryPools[(int)type]);
    }
}
