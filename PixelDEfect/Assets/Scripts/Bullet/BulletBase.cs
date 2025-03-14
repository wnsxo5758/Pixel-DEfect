using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [SerializeField]
    private int damage; // 총알 데미지
    [SerializeField]
    private float speed; // 총알 속도
    [SerializeField]
    private AudioClip hitSound;
    [SerializeField]
    private bool players; // 플레이어 것인가?

    MovementRigidbody2D movement;
    AudioSource audio;
    Animator animator;
    private void Awake()
    {
        movement = GetComponent<MovementRigidbody2D>();
        audio = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
    }

    public void SetUp(float x)
    {
        movement.MoveTo(x);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (players == true)
        {
            if (collision.CompareTag("Enemy"))
            {
                collision.GetComponent<EnemyBase>().DecreaseHp(damage);
            }
            //만약 버튼이나 그런 것들이 총알과 상호작용한다면 사용
            //else if()
            //{

            //}
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

    private IEnumerator DestoryBullet()
    {
        animator.SetTrigger("isHit");
        PlaySound(hitSound);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

    private void PlaySound(AudioClip _clip)
    {
        audio.Stop();
        audio.clip = _clip;
        audio.Play();
    }

}
