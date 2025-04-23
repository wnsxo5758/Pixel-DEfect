using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ImpactType { Normal = 0, Obstacle, }
public class ImpactMemoryPool : MonoBehaviour
{
    [Header("레이어 설정")]
    [SerializeField]
    private LayerMask normalLayer; // 기본 레이어
    [SerializeField]
    private LayerMask obstacleLayer; // 장애물 레이어, 기본 레이어 말고 다른 이펙트 프리팹을 사용하고자 하는 경우
    [SerializeField]
    private GameObject[] impactPrefab; // 각 타입에 대응하느 프리팹
    private MemoryPool[] memoryPools; // 메모리풀

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
        ImpactType? type = GetImpactTypeFromLayer(hit.transform.gameObject.layer);
        if (type.HasValue)
        {
            OnSpawnImpact(type.Value, hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
        }
    }
    public GameObject SpawnImpactAndReturn(RaycastHit2D hit)
    {
        ImpactType? type = GetImpactTypeFromLayer(hit.transform.gameObject.layer);
        if (!type.HasValue) return null;

        GameObject item = memoryPools[(int)type.Value].ActivePoolItem();
        if (item != null)
        {
            // 충돌 지점에 바로 생성
            item.transform.position = hit.point;

            //회전만 유지 (방향은 유지하고 위치만 정확히)
            item.transform.rotation = Quaternion.LookRotation(Vector3.forward, hit.normal);

            item.GetComponent<Impact>().SetUp(memoryPools[(int)type.Value]);
            item.SetActive(true);
        }

        return item;
    }


    public void OnSpawnImpact(ImpactType type, Vector2 pos, Quaternion rotation)
    {
        GameObject item = memoryPools[(int)type].ActivePoolItem();
        item.transform.position = pos;
        item.transform.rotation = rotation;
        item.GetComponent<Impact>().SetUp(memoryPools[(int)type]);
        item.SetActive(true);
    }

    private ImpactType? GetImpactTypeFromLayer(int layer)
    {
        if (IsInLayerMask(layer, normalLayer))
            return ImpactType.Normal;
        if (IsInLayerMask(layer, obstacleLayer))
            return ImpactType.Obstacle;
        return null; // 해당되지 않으면 이펙트 없음
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

}
