using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ValveButton : ButtonBase
{
    [Header("벨브 설정")]
    [SerializeField]
    private float pressTimeRequired; //필요한 요구치

    [SerializeField]
    private float currentPressTime; // 현재 누른 시간
    public bool isPressing; // 벨브를 누르고 있는가


    private void Update()
    {
        // 요구치에 도달했지만 아직 작동하지 않은 경우
        if (isActiving == false && currentPressTime >= pressTimeRequired)
        {
            isActiving = true;
            StartCoroutine(nameof(ButtonActive));
        }
        else if (!isPressing)
        {
            //밸브가 작동이 완료된 상태가 아니고, 누른 시간이 0보다 큰 경우
            if (currentPressTime > 0 && isActiving == false)
            {
                //시간이 지날수록 누른 시간이 떨어진다.
                currentPressTime -= Time.deltaTime;
            }
        }
        // 벨브를 누르는 중인 경우
        else if(isPressing)
        {
            if(currentPressTime < pressTimeRequired)
                currentPressTime += Time.deltaTime;
            else isPressing = false;
        }
    }
    
    
    
    protected override IEnumerator ButtonActive() // 버튼을  누른경우
    {
        isActiving = true; // 작동시작
        audio.Play();  // 효과음 
        if (connectedObjects != null) // 작동되는 오브젝트가 있다면
        {
            foreach (var obj in connectedObjects)
            {
                obj.Trigger();
            }
        }
        yield return null; // 1초 대기, 나중에 수치 수정
    }
}
