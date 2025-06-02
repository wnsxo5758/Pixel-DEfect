using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCoolUI : MonoBehaviour
{
    [SerializeField] private Image cooldownImage;
    [SerializeField] private TimeManager timeManager;

    private float cooldownTime;  // 쿨타임 전체 시간
    private float cooldownTimer; // 현재 남은 시간
    private bool isCooling = false;

    public void StartCooldown()
    {
        cooldownTime = timeManager.TimeFreezeCoolDown;
        cooldownTimer = cooldownTime;
        isCooling = true;
    }

    private void Update()
    {
        if (!isCooling) return;

        cooldownTimer -= Time.deltaTime;

        // ✅ 남은 시간에 비례해 채우기
        float fill = Mathf.Clamp01(1f - (cooldownTimer / cooldownTime));
        cooldownImage.fillAmount = fill;

        if (cooldownTimer <= 0f)
        {
            isCooling = false;
            cooldownImage.fillAmount = 1f; // 쿨 완료
        }
    }
}
