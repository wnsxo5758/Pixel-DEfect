using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [Header("설정")]
    public int damage = 2;
    public LayerMask targetLayer;
    [SerializeField]
    private Vector2 attackBoxOffset;     // 공격 판정의 오프셋
    [SerializeField]
    private Vector2 attackBoxSize;

    [Header("스킬 사운드")]
    [SerializeField]
    private AudioClip lightingClip;
    [SerializeField]
    private AudioClip attackClip;

    private Coroutine attackCoroutine;
    private HashSet<Collider2D> alreadyAttacked = new HashSet<Collider2D>();


    private bool isActive = false;
    private Animator animator;

    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        isActive = false;
        animator?.Play("SkillReady", -1, 0f);
    }

    // === Animation Events ===

    public void OnSkillReady() // 스킬 시작시 
    {
        //시작 사운드 
        PlaySound(lightingClip);
    }
    public void OnSkillReadyEnd()
    {
        animator?.Play("SkillAttack");
        isActive = true;
    }
    public void OnSkillAttackStart()
    {
        PlaySound(attackClip);
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        attackCoroutine = StartCoroutine(AttackRoutine());

    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            Vector2 center = (Vector2)transform.position + (Vector2)(transform.right * attackBoxOffset.x + transform.up * attackBoxOffset.y);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, attackBoxSize, 0f, targetLayer);

            foreach (var hit in hits)
            {
                if (alreadyAttacked.Contains(hit)) continue;

                PlayerHp hp = hit.GetComponent<PlayerHp>();
                if (hp != null)
                {
                    DeathData deathData = new DeathData(DeathCause.Press, 0);
                    hp.DecreaseHp(damage, Vector2.zero, deathData, false);
                    alreadyAttacked.Add(hit);
                }
            }

            yield return null; // 매 프레임 반복
        }
    }

    public void OnSkillAttackEnd()
    {
        animator?.Play("SkillEnd");
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        alreadyAttacked.Clear();
    }

    public void OnSkillEnd()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip _clip)
    {
        audioSource.Stop();
        audioSource.clip = _clip;
        audioSource.Play();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 center = (Vector2)transform.position + (Vector2)(transform.right * attackBoxOffset.x + transform.up * attackBoxOffset.y);
        Gizmos.DrawWireCube(center, attackBoxSize);
    }

}
