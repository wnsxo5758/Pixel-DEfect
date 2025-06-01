using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeStopUIEffect : MonoBehaviour
{
    [Header("UI 이펙트 설정")] 
    [SerializeField] private GameObject uiEffectPanel;
    [SerializeField] private Image effectImage;
    [SerializeField] private Animator uiAnimator;

    [Header("애니메이션 트리거 이름")] 
    [SerializeField] private string startTriggerName = "StartTimeStop";
    [SerializeField] private string endTriggerName = "EndTimeStop";
    
    // 애니메이션 상태 해시
    private int startTriggerHash;
    private int endTriggerHash;
    
    // 상태 관리
    private bool isAnimating = false;

    private void Awake()
    {
        startTriggerHash = Animator.StringToHash(startTriggerName);
        endTriggerHash = Animator.StringToHash(endTriggerName);
        
        //  컴포넌트 자동 할당
        if (uiEffectPanel == null)
            uiEffectPanel = gameObject.transform.parent.gameObject;

        if (effectImage == null)
            effectImage = GetComponent<Image>();

        if (uiAnimator == null)
            uiAnimator = GetComponent<Animator>();
    }
    
    void Start()
    {
        if (TimeManager.Instance != null)
        {
            Debug.Log("Event 할당");
            TimeManager.Instance.OnTimeStopBegin += OnTimeStopBegin;
        }
        
        if (uiEffectPanel != null)
            uiEffectPanel.SetActive(false);
    }

    private void OnTimeStopBegin()
    {
        Debug.Log("TimeStopBegin");
        PlayStartAnimation();
    }

    private void PlayStartAnimation()
    {
        if (isAnimating)
        {
            return;
        }

        if (uiAnimator == null)
        {
            return;
        }
        
        if (uiEffectPanel != null)
            uiEffectPanel.SetActive(true);
        
        isAnimating = true;
        
        uiAnimator.SetTrigger(startTriggerHash);
    }

    private void OnAnimationEnd()
    {
        isAnimating = false;
        
        uiEffectPanel.SetActive(false);
    }
}
