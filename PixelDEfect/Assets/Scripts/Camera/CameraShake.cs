using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Shake Settings")]
    [SerializeField] private float defaultShakeDuration = 0.2f;
    [SerializeField] private float defaultShakeMagnitude = 0.1f;
    [SerializeField] private float defaultShakeFadeTime = 0.1f;

    private Vector3 initialPosition;
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
        }
    }

    private void OnEnable()
    {
        initialPosition = transform.localPosition;
    }
    
    private void Update()
    {
        if (isShaking)
        {
            ProcessShake();
        }
    }

    public void ShakeScreen()
    {
        ShakeScreen(defaultShakeDuration, defaultShakeMagnitude, defaultShakeFadeTime);
    }

    public void ShakeScreen(float duration, float magnitude, float fadeTime)
    {
        initialPosition = transform.localPosition;
        shakeElapsedTime = 0f;
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeFadeTime = fadeTime;
        
        isShaking = true;
    }

    private void ProcessShake()
    {
        shakeElapsedTime += Time.deltaTime;

        float fadePercentage =
            Mathf.Clamp01(1f - ((shakeElapsedTime - (shakeDuration - shakeFadeTime)) / shakeFadeTime));
        float currentMagnitude = shakeMagnitude * fadePercentage;
        
        float x = Random.Range(-1f, 1f) * currentMagnitude;
        float y = Random.Range(-1f, 1f) * currentMagnitude;

        transform.localPosition = new Vector3(
            initialPosition.x + x,
            initialPosition.y + y,
            initialPosition.z
        );

        if (shakeElapsedTime >= shakeDuration)
        {
            isShaking = false;
            transform.localPosition = initialPosition;
        }
    }
}
