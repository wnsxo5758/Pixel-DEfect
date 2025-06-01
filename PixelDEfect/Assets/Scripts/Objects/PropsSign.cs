using UnityEngine;
using TMPro;
using System.Collections;

public class PropsSign : MonoBehaviour
{
    [SerializeField] private GameObject guideObject;
    [SerializeField] private float fadeDuration = 0.5f;

    private SpriteRenderer[] spriteRenderers;
    private TextMeshPro[] tmpTexts;
    private Coroutine currentFade;

    private void Awake()
    {
        spriteRenderers = guideObject.GetComponentsInChildren<SpriteRenderer>();
        tmpTexts = guideObject.GetComponentsInChildren<TextMeshPro>();

        SetAlpha(0f); // 시작 시 투명하게
        guideObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(FadeTo(1f)); // 나타남
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(FadeTo(0f)); // 사라짐
        }
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float timer = 0f;

        // 첫 SpriteRenderer 알파값 기준
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
            tmp.color = c; // 바로 이게 Vertex Color 알파값에 반영됨
        }
    }
}