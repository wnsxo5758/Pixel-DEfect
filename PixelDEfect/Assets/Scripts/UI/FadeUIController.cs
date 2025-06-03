using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeUIController : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] 
    private Image blackScreen;       // 전체 화면 덮는 검정 이미지
    [SerializeField] 
    private GameObject targetUI;     // 최종적으로 보여줄 UI

    [Header("시간 설정")]
    [SerializeField] 
    private float fadeInDuration = 1f;   // 페이드 인 시간
    [SerializeField] 
    private float holdDuration = 1f;     // 대기 시간
    [SerializeField] 
    private float fadeOutDuration = 1f;  // 페이드 아웃 시간

    [Header("효과음")]
    [SerializeField]
    private AudioClip uiAppearSound;
    [SerializeField]
    private AudioClip ev2Sound;
    [SerializeField]
    private AudioClip ev2ArriveClip;

    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // 초기 상태 설정
        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = 1f; // 시작은 검은 상태
            blackScreen.color = c;
            blackScreen.enabled = true;
        }

        if (targetUI != null)
        {
            targetUI.SetActive(false); // UI는 처음에 비활성화
        }
    }

    private void Start()
    {
        StartCoroutine(FadeSequence());
        PlaySound(ev2Sound);
    }

    private IEnumerator FadeSequence()
    {
        // 1. 페이드 인: 점점 밝아짐
        yield return StartCoroutine(Fade(1f, 0f, fadeInDuration));

        // 2. 대기
        yield return new WaitForSeconds(holdDuration);

        PlaySound(ev2ArriveClip);
        // 3. 페이드 아웃: 다시 어두워짐
        yield return StartCoroutine(Fade(0f, 1f, fadeOutDuration));

        // 4. UI 활성화
        if (targetUI != null)
        {
            targetUI.SetActive(true);
        }

        // 5. 효과음 재생
        if (audioSource != null && uiAppearSound != null)
        {
            PlaySound(uiAppearSound);
        }
    }

    private IEnumerator Fade(float fromAlpha, float toAlpha, float duration)
    {
        float timer = 0f;
        Color c = blackScreen.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, timer / duration);
            c.a = alpha;
            blackScreen.color = c;
            yield return null;
        }

        c.a = toAlpha;
        blackScreen.color = c;
    }

    public void LoopSound(AudioClip _clip)
    {

    }
    public void PlaySound(AudioClip _clip)
    {
        audioSource.Stop();
        audioSource.clip = _clip;
        audioSource.Play();
    }
}
