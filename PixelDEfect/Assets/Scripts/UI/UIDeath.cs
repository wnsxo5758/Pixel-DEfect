using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDeath : MonoBehaviour
{
    [SerializeField] private Image backgroundImage; // ���� �̹��� (��: ���� ������)
    [SerializeField] private Image foregroundImage; // ���� UI (��: "YOU DIED" �ؽ�Ʈ ��)
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Button respawnButton;
    [SerializeField] private float buttonAppearDelay = 2f;
    
    private bool isFading = false;
    private bool isShowing = false;

    void Start()
    {
        SetUp();
        SetupButton();
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
    
    public void SetUp()
    {
        InitImage(backgroundImage);
        InitImage(foregroundImage);

        isShowing = false;
    }

    private void SetupButton()
    {
        if (respawnButton != null)
        {
            respawnButton.onClick.AddListener(OnRespawnButtonClicked);
            respawnButton.gameObject.SetActive(false);
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
        isShowing = true;
        
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

        // ���� ����
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
        
        yield return new WaitForSeconds(buttonAppearDelay);
        ShowButton();
    }

    private void ShowButton()
    {
        if (respawnButton != null)
        {
            respawnButton.gameObject.SetActive(true);
            StartCoroutine(FadeInButton(respawnButton));
        }
    }

    private IEnumerator FadeInButton(Button button, float delay = 0f)
    {
        if (button == null) yield break;
        
        yield return new WaitForSeconds(delay);
        
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        
        float elapsed = 0f;
        float buttonFadeDuration = 0.3f;
        
        while (elapsed < buttonFadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / buttonFadeDuration);
            canvasGroup.alpha = alpha;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    public void HideDeathUI()
    {
        if (!isShowing) return;

        StartCoroutine(FadeOutImages());
    }
    
    private IEnumerator FadeOutImages()
    {
        float elapsed = 0f;
        float fadeOutDuration = 0.5f;

        Color bgColor = backgroundImage.color;
        Color fgColor = foregroundImage.color;

        // 버튼 먼저 숨기기
        if (respawnButton != null)
            respawnButton.gameObject.SetActive(false);
        

        // 이미지 페이드아웃
        while (elapsed < fadeOutDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);

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
            backgroundImage.color = new Color(bgColor.r, bgColor.g, bgColor.b, 0f);
            backgroundImage.raycastTarget = false;
        }

        if (foregroundImage != null)
        {
            foregroundImage.color = new Color(fgColor.r, fgColor.g, fgColor.b, 0f);
            foregroundImage.raycastTarget = false;
        }
        
        isShowing = false;
    }
    
    // 버튼 이벤트 핸들러들
    private void OnRespawnButtonClicked()
    {
        Debug.Log("Respawn 버튼 클릭됨");
        
        // 버튼 비활성화 (중복 클릭 방지)
        if (respawnButton != null)
            respawnButton.interactable = false;
        
        // 사망 UI 숨기기
        HideDeathUI();
        
        // GameManager에게 부활 요청
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer();
        }
        else
        {
            Debug.LogError("GameManager.Instance가 null입니다!");
        }
    }

    private void OnDestroy()
    {
        if (respawnButton != null)
            respawnButton.onClick.RemoveListener(OnRespawnButtonClicked);
    }
}