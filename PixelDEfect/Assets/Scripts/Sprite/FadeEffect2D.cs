using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class FadeEffect2D : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField]
    private Color flashColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField]
    private float flashInterval = 0.2f;
    [SerializeField]
    private int flashRepeat = 2;

    private SpriteRenderer spriteRenderer;
    private EnemyHp enemyHp;
    private Color originColor;
    private Coroutine flashCo;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originColor = spriteRenderer.color;
        enemyHp = GetComponentInParent<EnemyHp>();
    }

    private void OnEnable()
    {
        if (enemyHp != null)
        {
            enemyHp.OnDamaged += HandleDamaged;
            enemyHp.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (enemyHp != null)
        {
            enemyHp.OnDamaged -= HandleDamaged;
            enemyHp.OnDied -= HandleDied;
        }
        StopFlash();
        spriteRenderer.color = originColor;
    }
    private void HandleDamaged(int damage, bool _)
    {
        // 피격 시 짧게 n회 깜빡임
        if (flashCo != null) StopCoroutine(flashCo);
        flashCo = StartCoroutine(FlashRoutine());
    }

    private void HandleDied()
    {
        // 사망 시 효과 종료 및 원상복구
        StopFlash();
        spriteRenderer.color = originColor;
    }
    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashRepeat; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originColor;
            yield return new WaitForSeconds(flashInterval);
        }
        flashCo = null;
    }
    private void StopFlash()
    {
        if (flashCo != null)
        {
            StopCoroutine(flashCo);
            flashCo = null;
        }
    }

}
