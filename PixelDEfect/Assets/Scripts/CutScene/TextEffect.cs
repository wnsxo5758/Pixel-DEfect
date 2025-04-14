using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextEffect : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float blinkSpeed = 1.5f;

    private bool blinking = false;

    void Update()
    {
        if (blinking)
        {
            float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f; // 0 ~ 1
            canvasGroup.alpha = alpha;
        }
    }

    public void StartBlink()
    {
        blinking = true;
        canvasGroup.alpha = 1f;
    }

    public void StopBlink()
    {
        blinking = false;
        canvasGroup.alpha = 0f;
    }
}
