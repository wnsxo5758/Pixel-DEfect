using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Header("레이저 빔설정")]
    [SerializeField]
    private int damage = 1;// 데미지
    [SerializeField]
    private Transform beamTransform; // 레이저의 실제 Transforem 

    [SerializeField]
    private LayerMask groundLayer;

    private ImpactMemoryPool impactPool;


    private void Awake()
    {
        impactPool = GetComponent<ImpactMemoryPool>();
    }
    private Transform source;
    public void SetSource(Transform laserTrap)
    {
        source = laserTrap;
        transform.position = source.position;
    }

    public void SetLength(float maxLength)
    {
        if (beamTransform == null || source == null) return;

        // 바닥이나 충돌 지점까지 레이저 뻗음
        RaycastHit2D hit = Physics2D.Raycast(source.position, Vector2.down, Mathf.Infinity, groundLayer);
        float actualLength = hit.collider != null ? hit.distance : maxLength;

        // 레이저 빔 크기 및 위치 설정
        beamTransform.localScale = new Vector3(1, actualLength, 1);
        beamTransform.position = source.position + Vector3.down * (actualLength / 2);

        // 이펙트 발생
        if (hit.collider != null && impactPool != null)
        {
            impactPool.SpawnImpact(hit);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHp>().DecreaseHp(damage);
        }
        else if(collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyFSM>().DecreaseHp(damage);
        }
    }

}
