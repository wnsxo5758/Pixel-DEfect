using UnityEngine;
using TMPro;
using System.Collections;

public class PropsSign : MonoBehaviour
{
    [SerializeField] private GameObject guideObject;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float displayTime = 2f; // 기획자가 지정한 표시 시간

    private SpriteRenderer[] spriteRenderers;
    private TextMeshPro[] tmpTexts;
    private Coroutine currentRoutine;

    private bool isActivated = false; // 1회성 여부

    private void Awake()
    {
        spriteRenderers = guideObject.GetComponentsInChildren<SpriteRenderer>();
        tmpTexts = guideObject.GetComponentsInChildren<TextMeshPro>();

        SetAlpha(0f); // 시작 시 투명하게
        guideObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActivated) return; // 이미 실행된 경우 무시

        if (collision.CompareTag("Player"))
        {
            isActivated = true; // 다시 작동 못 하게 설정
            if (currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(FadeSequence());
        }
    }

    private IEnumerator FadeSequence()
    {
        // 페이드 인
        yield return StartCoroutine(FadeTo(1f));

        // 일정 시간 유지
        yield return new WaitForSeconds(displayTime);

        // 페이드 아웃
        yield return StartCoroutine(FadeTo(0f));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float timer = 0f;

        // 현재 알파값 기준
        float startAlpha = spriteRenderers.Length > 0 ? spriteRenderers[0].color.a : 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        foreach (var tmp in tmpTexts)
        {
            Color c = tmp.color;
            c.a = alpha;
            tmp.color = c;
        }
    }
}