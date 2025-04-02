using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBase : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField]
    private bool canRange; // 원거리 공격으로도 열리는가
    [SerializeField]
    protected bool isActive; // 버튼이 활성화되었는가 
    [SerializeField]
    protected bool isActiving; // 작동 중인가
    [SerializeField]
    protected bool isCount; // 시간제한이 있는 버튼인가
    [SerializeField]
    protected float timeToLimit; // 시간제한 값
    [SerializeField]
    protected InteractableObject[] connectedObjects; // 버튼과 상호작용할 오브젝트
    
    [Header("스프라이트")]
    [SerializeField]
    private Sprite activeSprite; //활성화시 스프라이트
    [SerializeField]
    private Sprite inActiveSprite; // 비활성화시

    private SpriteRenderer sprite;
    protected AudioSource audio;
    private void Awake()
    {
        sprite  = GetComponentInChildren<SpriteRenderer>();
        audio = GetComponent<AudioSource>();
        UpdateSprite();
    }

    protected void UpdateSprite()
    {
        if(sprite != null)
        {
            sprite.sprite = isActive ? activeSprite : inActiveSprite;
        }
    }

    public void ButtonTrigger() // 버튼 활성화시 작동
    {
        if(isCount)
        {
            if (isActiving == false)
            {
                StartCoroutine(nameof(TimeLimitButton));
            }

        }
        else
        {
            if(isActiving == false)
            {
                StartCoroutine(nameof(ButtonActive));
            }

        }
    }
    protected virtual IEnumerator TimeLimitButton()
    {
        isActiving = true;
        isActive = true;
        UpdateSprite(); // 버튼 스프라이트 변경
        audio.Play();  // 효과음 
        if (connectedObjects != null) // 작동되는 오브젝트가 있다면
        {
            foreach (var obj in connectedObjects)
            {
                obj.Trigger();
            }
        }
        yield return new WaitForSeconds(timeToLimit); // 
        isActive = false;
        isActiving = false;
        UpdateSprite(); // 버튼 스프라이트 변경
        if (connectedObjects != null) // 작동되는 오브젝트가 있다면
        {
            foreach (var obj in connectedObjects)
            {
                obj.Trigger();
            }
        }


    }
    protected virtual IEnumerator ButtonActive()
    {
        isActiving = true; // 작동시작
        isActive = !isActive; // 누르면 활성화
        UpdateSprite(); // 버튼 스프라이트 변경
        audio.Play();  // 효과음 
        if (connectedObjects != null) // 작동되는 오브젝트가 있다면
        {
            foreach (var obj in connectedObjects)
            {
                obj.Trigger();
            }
        }
        yield return new WaitForSeconds(1.0f); // 1초 대기, 나중에 수치 수정

        isActiving = false; // 작동 완료
    }
}
