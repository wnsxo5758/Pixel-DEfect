using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    [SerializeField]
    private StageData stageData;
    [SerializeField]
    private Transform target;
    [SerializeField]
    private bool x, y, z;

    private float offsetY;

    [SerializeField] private float yFollowThreshold = 10f; // 추적 시작하는 최소 y 차이
    [SerializeField] private float yStopThreshold = 0.05f; // 추적 중지 판단 기준
    [SerializeField] private float yStopDelay = 1f; // 추적 중지를 위한 대기 시간
    [SerializeField] private float yLerpSpeed = 5f; // Y축 보간 속도


    private bool isYFollowing;
    private float yFollowTimer;
    private float lastTargetY;


    private void Awake()
    {
        offsetY = Mathf.Abs(transform.position.y - target.position.y);
    }

    private void LateUpdate()
    {
        Vector3 targetPos = target.position;

        // Y축 추적 상태 판단 로직
        float yDiff = Mathf.Abs(transform.position.y - targetPos.y);

        if (!isYFollowing && yDiff > yFollowThreshold)
        {
            isYFollowing = true;
            yFollowTimer = 0;
        }

        if (isYFollowing)
        {
            float yMoveDelta = Mathf.Abs(lastTargetY - targetPos.y);
            if (yMoveDelta < yStopThreshold)
            {
                yFollowTimer += Time.deltaTime;
                if (yFollowTimer >= yStopDelay)
                {
                    isYFollowing = false;
                }
            }
            else
            {
                yFollowTimer = 0; // 계속 움직이면 타이머 리셋
            }

            lastTargetY = targetPos.y;
        }

        // 목표 위치 계산
        Vector3 newPos = transform.position;

        if (x)
            newPos.x = targetPos.x;

        if (isYFollowing)
        {
            float targetY = targetPos.y + offsetY;
            newPos.y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * yLerpSpeed);
        }

        if (z)
            newPos.z = targetPos.z;

        // 클램프 적용 후 위치 갱신
        newPos.x = Mathf.Clamp(newPos.x, stageData.CameraLimitMinX, stageData.CameraLimitMaxX);
        transform.position = newPos;
    }
}
