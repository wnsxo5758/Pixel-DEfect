using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [Header("설정")]
    public int damage = 2;
    public LayerMask targetLayer;
    [Header("스킬 사운드")]
    [SerializeField]
    private AudioClip lightingClip;
    [SerializeField]
    private AudioClip attackClip;

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
    }
    public void OnSkillAttackEnd()
    {
        animator?.Play("SkillEnd");
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
    // === 충돌 시 데미지 ===
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if ((targetLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            PlayerHp hp = other.GetComponent<PlayerHp>();
            if (hp != null)
            {
                DeathData deathData = new DeathData(DeathCause.Press, 0); // dir은 외부 처리
                hp.DecreaseHp(damage, Vector2.zero, deathData, false);
            }
        }
    }
}
