using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDeath : MonoBehaviour
{
    [SerializeField] private Image backgroundImage; // 배경용 이미지 (예: 검정 반투명)
    [SerializeField] private Image foregroundImage; // 전경 UI (예: "YOU DIED" 텍스트 등)
    [SerializeField] private float fadeDuration = 1f;

    private bool isFading = false;

    void Start()
    {
        InitImage(backgroundImage);
        InitImage(foregroundImage);
    }

    private void InitImage(Image img)
    {
        if (img != null)
        {
            Color color = img.color;
            color.a = 0f;
            img.color = color;
            img.raycastTarget = false;
        }
    }

    public void ShowDeathUI()
    {
        if (!isFading && backgroundImage != null && foregroundImage != null)
        {
            StartCoroutine(FadeInImages());
        }
    }

    private IEnumerator FadeInImages()
    {
        isFading = true;
        float elapsed = 0f;

        Color bgColor = backgroundImage.color;
        Color fgColor = foregroundImage.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);

            if (backgroundImage != null)
                backgroundImage.color = new Color(bgColor.r, bgColor.g, bgColor.b, alpha);

            if (foregroundImage != null)
                foregroundImage.color = new Color(fgColor.r, fgColor.g, fgColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 최종 보정
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(bgColor.r, bgColor.g, bgColor.b, 1f);
            backgroundImage.raycastTarget = true;
        }

        if (foregroundImage != null)
        {
            foregroundImage.color = new Color(fgColor.r, fgColor.g, fgColor.b, 1f);
            foregroundImage.raycastTarget = true;
        }

        isFading = false;
    }
}