using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Header("레이저 빔설정")]
    [SerializeField] private int damage = 1;
    [SerializeField] private Transform beamTransform;
    [SerializeField] private LayerMask groundLayer;

    private Transform source;
    private LaserDirection direction = LaserDirection.Down;

    private ImpactMemoryPool pool;
    private GameObject impactEffect;

    private void Awake()
    {
        pool = GetComponent<ImpactMemoryPool>();
    }

    public void SetSource(Transform laserTrap)
    {
        source = laserTrap;
        transform.position = source.position;
    }

    public void SetDirection(LaserDirection dir)
    {
        direction = dir;
    }

    public void SetLength(float maxLength)
    {
        if (beamTransform == null || source == null) return;

        Vector2 dirVec = GetDirectionVector2D();
        RaycastHit2D hit = Physics2D.Raycast(source.position, dirVec, Mathf.Infinity, groundLayer);
        float actualLength = hit.collider != null ? hit.distance : maxLength;

        // 레이저 스케일 조정
        beamTransform.localScale = direction switch
        {
            LaserDirection.Left or LaserDirection.Right => new Vector3(actualLength, 1, 1),
            _ => new Vector3(1, actualLength, 1)
        };

        // 레이저 위치 조정 (중앙 보정)
        beamTransform.position = source.position + GetOffsetVector3(actualLength);

        // 회전 유지
        beamTransform.rotation = GetRotation();

        // 이펙트 생성 및 위치 처리
        if (hit.collider != null && pool != null)
        {
            if (impactEffect == null)
            {
                impactEffect = pool.SpawnImpactAndReturn(hit);
            }

            // ✅ 정확한 충돌 위치에 생성
            impactEffect.transform.position = hit.point;
        }
        else
        {
            if (impactEffect != null)
            {
                impactEffect.SetActive(false);
                impactEffect = null;
            }
        }
    }

    private Vector2 GetDirectionVector2D()
    {
        return direction switch
        {
            LaserDirection.Up => Vector2.up,
            LaserDirection.Down => Vector2.down,
            LaserDirection.Left => Vector2.left,
            LaserDirection.Right => Vector2.right,
            _ => Vector2.down
        };
    }

    private Vector3 GetOffsetVector3(float length)
    {
        return direction switch
        {
            LaserDirection.Up => Vector3.up * (length / 2),
            LaserDirection.Down => Vector3.down * (length / 2),
            LaserDirection.Left => Vector3.left * (length / 2),
            LaserDirection.Right => Vector3.right * (length / 2),
            _ => Vector3.down * (length / 2)
        };
    }

    private Quaternion GetRotation()
    {
        return direction switch
        {
            LaserDirection.Up => Quaternion.Euler(0, 0, 0),
            LaserDirection.Down => Quaternion.Euler(0, 0, 180),
            LaserDirection.Left => Quaternion.Euler(0, 0, 90),
            LaserDirection.Right => Quaternion.Euler(0, 0, -90),
            _ => Quaternion.identity
        };
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHp>()?.DecreaseHp(damage);
        }
        else if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyFSM>()?.DecreaseHp(damage);
        }
    }
}

