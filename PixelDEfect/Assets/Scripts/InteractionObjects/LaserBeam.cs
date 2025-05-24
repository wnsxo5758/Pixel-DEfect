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

        // ✅ 항상 Y축만 scale 변경
        beamTransform.localScale = new Vector3(1, actualLength, 1);

        // ✅ 로컬 Y축 기준으로 offset
        beamTransform.position = source.position + GetOffsetVector3(actualLength);

        // ✅ 회전은 미리 설정되어 있다고 가정
        beamTransform.rotation = GetRotation();

        // ✅ 이펙트
        if (hit.collider != null && pool != null)
        {
            if (impactEffect == null)
                impactEffect = pool.SpawnImpactAndReturn(hit);

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
        // ✅ 항상 로컬 Y축 기준이므로, 회전된 방향과 관계없이 "up"을 기준
        return transform.up * (length / 2f);
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

    public void Deactivate()
    {
        if (impactEffect != null)
        {
            impactEffect.SetActive(false);
            impactEffect = null;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DeathData deathData = new DeathData(DeathCause.Laser);
            collision.GetComponent<PlayerHp>()?.DecreaseHp(damage, deathData);
        }
        else if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyFSM>()?.DecreaseHp(damage);
        }
    }
}

