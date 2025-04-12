using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

public class ValveButton : ButtonBase
{
    [Header("벨브 설정")]
    [SerializeField]
    private float pressTimeRequired; //필요한 요구치
    [SerializeField]
    private float currentPressTime; // 현재 누른 시간
    public bool isPressing; // 벨브를 누르고 있는가
    [Header("밸브 효과음")]
    [SerializeField]
    private AudioClip rotateClip; // 돌아갈때의 효과음
    [SerializeField]
    private AudioClip rewindClip; // 되돌아갈때의 효과음

    private Collider2D colllider;
    private void Start()
    {
        colllider = GetComponent<Collider2D>();
    }
    private void Update()
    {
        if(isPressing)
        {
            if (currentPressTime < pressTimeRequired)
            {
                Debug.Log($"밸브 작동 중 : {currentPressTime}");
                currentPressTime += Time.deltaTime;
                RotateValve(1);
                LoopAudioPlay(rotateClip);
            }

            else if(currentPressTime >= pressTimeRequired && isActiving == false)
            {
                LoopAudioStop();
                StartCoroutine(nameof(ButtonActive));
            }
            
        }
        if (!isPressing)
        {
            //밸브가 작동이 완료된 상태가 아니고, 누른 시간이 0보다 큰 경우
            if (currentPressTime > 0 && isActiving == false)
            {
                //시간이 지날수록 누른 시간이 떨어진다.
                currentPressTime -= Time.deltaTime;
                Debug.Log($"밸브 떨어지는 중 : {currentPressTime}");
                RotateValve(-1);
                LoopAudioPlay(rewindClip);
            }
            else
            {
                LoopAudioStop();
            }
        }
    }

    public float GetPressRatio()
    {
        return Mathf.Clamp01(currentPressTime / pressTimeRequired);
    }


    protected override IEnumerator ButtonActive() // 버튼을  누른경우
    {
        isActiving = true; // 작동시작
        audio.Play();  // 효과음 
        colllider.enabled = false;
        if (connectedObjects != null) // 작동되는 오브젝트가 있다면
        {
            foreach (var obj in connectedObjects)
            {
                obj.Trigger();
            }
        }
        yield return null; // 1초 대기, 나중에 수치 수정
    }

    private void RotateValve(int dir)
    {
        float rotateSpeed = 360f / pressTimeRequired;

        float deltaRotation = rotateSpeed * Time.deltaTime * dir;

        sprite.transform.Rotate(Vector3.forward, deltaRotation);
    }

    private void LoopAudioPlay(AudioClip _clip)
    {
        if (audio == null) return;
        if (audio.clip != _clip)
        {
            audio.clip = _clip;
            audio.Play();
        }
        else if(! audio.isPlaying)
        {
            audio.Play();
        }
    }

    private void LoopAudioStop()
    {
        if(audio != null && audio.isPlaying)
        {
            audio.Stop();   
        }
    }
}
