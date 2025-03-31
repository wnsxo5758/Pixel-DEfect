using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RespawnType { AfterTime = 0, PlayerDead } //
public class PlatformDrop : PlatformBase
{
    [SerializeField]
    private RespawnType respawnType = RespawnType.AfterTime;
    [SerializeField]
    private float respawnTime = 2f;

    private BoxCollider2D boxCollider2D;
    private Rigidbody2D rigid;
    private Vector3 originPos;
    private Animator animator;
    private AudioSource audio;
    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        audio = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        originPos = transform.position;
    }

    public override void UpdateCollision(GameObject other)
    {
        if (IsHit == true) return;
        IsHit = true;
        StartCoroutine(nameof(Process));
    }

    private IEnumerator Process()
    {
        yield return StartCoroutine(nameof(OnShake));
        OnDrop();

        if (respawnType == RespawnType.AfterTime)
        {
            StartCoroutine(nameof(OnRespawn));
        }
        else
        {
            Destroy(gameObject, respawnTime);
        }
    }

    private IEnumerator OnShake()
    {
        audio.Play();
        animator.SetTrigger("Warning");
        float percent = 0;
        float shakeAngle = 5;
        float shakeSpeed = 10;
        float shakeTime = 1.5f;

        while (percent < 1)
        {
            percent += Time.deltaTime / shakeTime;

            float z = Mathf.Lerp(-shakeAngle, shakeAngle, Mathf.PingPong(Time.time * shakeSpeed, 1));
            transform.rotation = Quaternion.Euler(0, 0, z);

            yield return null;
        }
        transform.rotation = Quaternion.identity;
    }

    private void OnDrop()
    {
        boxCollider2D.enabled = false;
        rigid.isKinematic = false;
    }

    private IEnumerator OnRespawn()
    {
        yield return new WaitForSeconds(respawnTime);
        IsHit = false;
        transform.position = originPos;
        boxCollider2D.enabled = true;
        rigid.isKinematic = true;
        rigid.velocity = Vector2.zero;

    }
}
