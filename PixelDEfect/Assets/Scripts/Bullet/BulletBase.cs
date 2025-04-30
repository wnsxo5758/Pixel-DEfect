using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [Header("총알 세팅")]
    [SerializeField]
    protected int damage; // 총알 데미지
    [SerializeField]
    protected float speed; // 총알 속도
    [SerializeField]
    protected AudioClip hitSound;
    [SerializeField]
    protected LayerMask hitLayer; // 피격대상 레이어


    private Vector2 moveDir;

    Rigidbody2D rigid;

    protected AudioSource audioSoruce;
    protected Animator animator;
    private MemoryPool memoryPool;
    private Collider2D collider2D;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        audioSoruce = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        collider2D = GetComponent<Collider2D>();
    }

    public virtual void SetUp(Vector2 direction, MemoryPool _memoryPool)
    {
        this.memoryPool = _memoryPool;

        moveDir = direction.normalized;

        // Rigidbody2D 이동 적용
        if (rigid != null)
        {
            rigid.velocity = moveDir * speed;
        }

        // 회전 적용
        RotateToDirection(moveDir);

        // Collider 활성화 (풀에서 재사용 시 초기화 필수)
        if (collider2D != null)
        {
            collider2D.enabled = true;
        }


    }

    private  void OnTriggerEnter2D(Collider2D collision)
    {
        if ((hitLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            StartCoroutine(nameof(DestoryBullet));
        }
    }

    //총알 비활성화(Destory 아님)
    protected virtual IEnumerator DestoryBullet()
    {
        GetComponent<Collider2D>().enabled = false;
        yield return null;
        memoryPool.DeactivatePoolItems(gameObject);
    }

    //사운두
    private void PlaySound(AudioClip _clip)
    {
        audioSoruce.Stop();
        audioSoruce.clip = _clip;
        audioSoruce.Play();
    }
    
    // 총알 회전시 슬프라이트 회전
    protected void RotateToDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


}
