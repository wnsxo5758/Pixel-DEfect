using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Overlays;
using UnityEngine;

public class PressMachine: InteractableObject
{
    [Header("프레스기 설정")]
    [SerializeField]
    private float pressDelay; // 프레스 대기 시간
    [SerializeField]
    private float pressDuration; // 프레스가 누른후 대기 시간
    [SerializeField]
    private float pressSpeed; // 작동 속도                           
    [SerializeField]
    private PressHead pressHead; // 프레스 머리
    [SerializeField]
    private LayerMask groundLayer; // 바닥 감지 레이어

    private Vector3 originPos;
    private Vector3 targetPos;

    private bool isPressing = false;


    private void Start()
    {
        originPos = pressHead.transform.position;
        DetectGround();
        if (isActive)
        {
            StartCoroutine(nameof(PressRoutine));
        }

    }
    public override void Trigger()
    {
        isActive = !isActive;
        if(isActive)
        {
            StartCoroutine(nameof(PressRoutine));
        }
        else
        {
            StopAllCoroutines();
        }

    }

    private void DetectGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(pressHead.transform.position, Vector2.down, Mathf.Infinity, groundLayer);
        if (hit.collider != null)
        {
            targetPos = hit.point; // 바닥 위치 저장
        }
        else
        {
            targetPos = pressHead.transform.position; // 기본값 (이동 안 함)
        }
    }

    private IEnumerator PressRoutine()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(pressDelay);

            isPressing = true;
            pressHead.SetPressing(true);

            while (Vector3.Distance(pressHead.transform.position, targetPos) > 0.1f)
            {
                pressHead.transform.position = Vector3.MoveTowards(pressHead.transform.position, targetPos, pressSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(pressDuration);

            while (Vector3.Distance(pressHead.transform.position, originPos) > 0.1f)
            {
                pressHead.transform.position = Vector3.MoveTowards(pressHead.transform.position, originPos, pressSpeed * Time.deltaTime);
                yield return null;
            }
            pressHead.SetPressing(false);
            isPressing = false;
        }
    }
}
