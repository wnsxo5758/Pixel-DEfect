using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FadeOutUI : MonoBehaviour
{
    [Header("UI관련")]
    [SerializeField]
    private Image blackScreen;     // 검은색 Image (전체 화면 덮기)
    [SerializeField] 
    private float fadeDuration = 1f; // 페이드 시간
    [Header("씬이동 관련")]
    [SerializeField]
    private string loadSceneName;

    private void Awake()
    {
        // 시작할 때는 투명하게 만들어 놓기
        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = 0f;
            blackScreen.color = c;
            blackScreen.enabled = false; // 화면에 안 보이게만 함
        }
    }

    public void FadeStart()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        blackScreen.enabled = true;

        float timer = 0f;
        Color c = blackScreen.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            blackScreen.color = c;
            yield return null;
        }

        c.a = 1f;
        blackScreen.color = c;
        LoadGameManager();
    }

    public void LoadGameManager()
    {
        if(loadSceneName != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(loadSceneName);
        }

    }
}
