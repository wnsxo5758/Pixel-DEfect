using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactBase : MonoBehaviour
{
    [Header("설정")]
    [SerializeField]
    private LayerMask groundLayer; // 감지할 레이어
    [SerializeField]
    private float rayDistance = 1f;
    [SerializeField]
    private float checkInterval = 0.1f;

    [Header("이펙트")]

    private ImpactMemoryPool impactMemoryPool;

    private ImpactType impactType = ImpactType.Normal;


    private AudioSource audio;

    private bool isHit;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
        impactMemoryPool = GetComponent<ImpactMemoryPool>();
    }

    private void Start()
    {
        StartCoroutine(CheckGroundCoroutine());
    }

    private IEnumerator CheckGroundCoroutine()
    {
        while (true)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, groundLayer);
            if (hit.collider != null && !isHit)
            {
                Debug.Log("레이캐스트 충돌 감지됨");

                isHit = true;
                Vector2 hitPoint = hit.point;

                impactMemoryPool.OnSpawnImpact(impactType, hitPoint, Quaternion.identity);

                if (audio != null)
                    audio.Play();
            }
            else if (hit.collider == null)
            {
                isHit = false; // 떨어지면 다시 충돌 감지 가능하게 만듦
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDistance);
    }
}
