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
    private AudioSource audioSource;
    private SpriteRenderer sprite;
    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
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
        audioSource.Play();
        animator.SetBool("Warning",true);
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
        rigid.isKinematic = false;
        rigid.gravityScale = 1;
        boxCollider2D.enabled = false;
        StartCoroutine(nameof(FadeOut));
    }

    private IEnumerator OnRespawn()
    {
        yield return new WaitForSeconds(respawnTime);
        animator.SetBool("Warning", false);
        IsHit = false;
        transform.position = originPos;
        boxCollider2D.enabled = true;
        rigid.isKinematic = true;
        rigid.velocity = Vector2.zero;
        StartCoroutine(nameof(FadeIn));

    }

    private IEnumerator FadeOut()
    {
        float duration = 1f; // 사라지는 시간
        float elapsed = 0;
        Color startColor = sprite.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / duration);
            sprite.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        sprite.color = new Color(startColor.r, startColor.g, startColor.b, 0);
    }
    private IEnumerator FadeIn()
    {
        float duration = 1f; // 나타나는 시간
        float elapsed = 0;
        Color startColor = sprite.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / duration);
            sprite.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        sprite.color = new Color(startColor.r, startColor.g, startColor.b, 1);
    }
}
