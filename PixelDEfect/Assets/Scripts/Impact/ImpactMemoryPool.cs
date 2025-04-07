using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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

    public void SpawnImpact()
    {

    }


    public void OnSpawnImpact(ImpactType type, Vector2 pos, Quaternion rotation)
    {
        GameObject item = memoryPools[(int)type].ActivePoolItem();
        item.transform.position = pos;
        item.transform.rotation = rotation;
        item.GetComponent<Impact>().SetUp(memoryPools[(int)type]);
    }
}
