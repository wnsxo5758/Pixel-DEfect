using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [SerializeField]
    protected int damage; // 총알 데미지
    [SerializeField]
    protected float speed; // 총알 속도
    [SerializeField]
    protected AudioClip hitSound;
    [SerializeField]
    private bool players; // 플레이어 것인가?

    MovementRigidbody2D movement;
    protected AudioSource audio;
    protected Animator animator;
    private MemoryPool memoryPool;
    private void Awake()
    {
        movement = GetComponent<MovementRigidbody2D>();
        audio = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
    }

    public virtual void SetUp(float x, MemoryPool _memoryPool)
    {
        this.memoryPool = _memoryPool;
        movement.MoveTo(x);
    }

    private  void OnTriggerEnter2D(Collider2D collision)
    {
        if (players == true)
        {
            if (collision.CompareTag("Enemy"))
            {
                collision.GetComponent<EnemyFSM>().DecreaseHp(damage);
            }
        }
        else
        {
            if (collision.CompareTag("Player"))
            {
                collision.GetComponent<PlayerHp>().DecreaseHp(damage);
            }

        }
        StartCoroutine(nameof(DestoryBullet));
    }

    protected virtual IEnumerator DestoryBullet()
    {
        movement.MoveTo(0);
        GetComponent<Collider2D>().enabled = false;
        //애니메이션 추가시 설정
        //animator.SetTrigger("isHit");
        //PlaySound(hitSound);
        //yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        yield return null;
        memoryPool.DeactivatePoolItems(gameObject);
    }

    private void PlaySound(AudioClip _clip)
    {
        audio.Stop();
        audio.clip = _clip;
        audio.Play();
    }

}
