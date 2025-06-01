using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachinShake : MonoBehaviour
{
    [Header("기본 흔들림 설정")]
    [SerializeField] private float defaultShakeDuration = 0.2f;
    [SerializeField] private float defaultShakeMagnitude = 1.0f;
    [SerializeField] private float defaultShakeFadeTime = 0.1f;

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlin;

    private float shakeElapsedTime;
    private float shakeDuration;
    private float shakeMagnitude;
    private float shakeFadeTime;
    private bool isShaking = false;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (perlin != null)
        {
            perlin.m_AmplitudeGain = 0f;
        }
    }

    private void LateUpdate()
    {
        if (!isShaking || perlin == null) return;

        shakeElapsedTime += Time.deltaTime;

        float fadePercentage = 1.0f;
        if (shakeDuration > 0f)
        {
            fadePercentage = Mathf.Clamp01(1f - ((shakeElapsedTime - (shakeDuration - shakeFadeTime)) / shakeFadeTime));
        }

        float currentMagnitude = shakeMagnitude * fadePercentage;
        perlin.m_AmplitudeGain = currentMagnitude;

        if (shakeElapsedTime >= shakeDuration)
        {
            isShaking = false;
            perlin.m_AmplitudeGain = 0f;
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
