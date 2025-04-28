using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    
    [Header("카메라 추적 설정")]
    [SerializeField] private StageData stageData;
    [SerializeField] private Transform target;
    [SerializeField] private bool trackX = true, trackY = false;

    [Header("Y축 추적 설정")] 
    [SerializeField] private float yFollowThreshold = 10f; // 추적 시작하는 최소 y 차이
    [SerializeField] private float yStopThreshold = 0.05f; // 추적 중지 판단 기준
    [SerializeField] private float yStopDelay = 1f; // 추적 중지를 위한 대기 시간
    [SerializeField] private float yLerpSpeed = 5f; // Y축 보간 속도
    
    [Header("흔들림 설정")]
    [SerializeField] private float defaultShakeDuration = 0.2f;
    [SerializeField] private float defaultShakeMagnitude = 0.1f;
    [SerializeField] private float defaultShakeFadeTime = 0.1f;

    private Vector3 targetPosition; // 카메라가 추적하는 목표 위치
    private Vector3 shakeOffset;
    private float offsetY;
    private float initialZ;
    
    // 추적 관련 변수
    private bool isYFollowing;
    private float yFollowTimer;
    private float lastTargetY;
    
    // 흔들림 관련 변수
    private float shakeElapsedTime;
    private float shakeDuration;
    private float shakeMagnitude;
    private float shakeFadeTime;
    private bool isShaking = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        initialZ = transform.position.z;
        
        targetPosition = transform.position;
        shakeOffset = Vector3.zero;

        if (target != null)
        {
            offsetY = Mathf.Abs(transform.position.y - target.position.y);
        }
    }

    private void LateUpdate()
    {
        // 추적 계산
        CalculateTargetPosition();
        
        // 흔들림 효과 계산
        if (isShaking)
        {
            CalculateShakeEffect();
        }
        
        // 최종 위치 결정
        Vector3 finalPosition = targetPosition + shakeOffset;
        
        finalPosition.z = initialZ;
        
        // 카메라 위치 업데이트
        transform.position = finalPosition;
    }

    private void CalculateTargetPosition()
    {
        if (target == null) return;

        Vector3 newTargetPos = targetPosition;
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
        if (trackX)
            newTargetPos.x = targetPos.x;

        if (isYFollowing)
        {
            float targetY = targetPos.y + offsetY;
            newTargetPos.y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * yLerpSpeed);
        }

        // 클램프 적용 후 위치 갱신
        if (stageData != null)
        {
            newTargetPos.x = Mathf.Clamp(newTargetPos.x, stageData.CameraLimitMinX, stageData.CameraLimitMaxX);
        }
        
        targetPosition = newTargetPos;
    }

    private void CalculateShakeEffect()
    {
        shakeElapsedTime += Time.deltaTime;

        float fadePercentage = 1.0f;
        if (shakeDuration > 0)
        {
            fadePercentage = Mathf.Clamp01(1f - ((shakeElapsedTime - (shakeDuration - shakeFadeTime)) / shakeFadeTime));
        }
            
        float currentMagnitude = shakeMagnitude * fadePercentage;

        shakeOffset = new Vector3(
            Random.Range(-1f, 1f) * currentMagnitude,
            Random.Range(-1f, 1f) * currentMagnitude,
            0f
        );
        
        if (shakeElapsedTime >= shakeDuration)
        {
            isShaking = false;
            shakeOffset = Vector3.zero;
        }
    }
    
    
    public void ShakeScreen()
    {
        ShakeScreen(defaultShakeDuration, defaultShakeMagnitude, defaultShakeFadeTime);
    }

    public void ShakeScreen(float duration, float magnitude, float fadeTime)
    {
        shakeElapsedTime = 0f;
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeFadeTime = fadeTime;
        
        isShaking = true;
    }
}
