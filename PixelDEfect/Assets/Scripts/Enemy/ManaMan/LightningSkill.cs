using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [Header("설정")]
    public int damage = 2;
    public LayerMask targetLayer;

    private bool isActive = false;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        isActive = false;
        animator?.Play("SkillReady", -1, 0f);
    }

    // === Animation Events ===

    public void OnSkillReadyEnd()
    {
        animator?.Play("SkillAttack");
        isActive = true;
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
