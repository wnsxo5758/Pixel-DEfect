using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCoolUI : MonoBehaviour
{
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Image clockHandImage;

    private float cooldownTime;  // 쿨타임 전체 시간
    private float cooldownTimer; // 현재 남은 시간
    private bool isCooling = false;

    public void StartCooldown()
    {
        cooldownTime = TimeManager.Instance.TimeFreezeCoolDown;
        cooldownTimer = cooldownTime;
        isCooling = true;
    }

    private void Update()
    {
        if (!isCooling) return;

        cooldownTimer -= Time.deltaTime;

        float fill = Mathf.Clamp01(1f - (cooldownTimer / cooldownTime));
        cooldownImage.fillAmount = fill;

        if (clockHandImage != null)
        {
            // 회전: 0 → -360도 (시계방향)
            float angle = -360f * fill;
            clockHandImage.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
        }


        if (cooldownTimer <= 0f)
        {
            isCooling = false;
            cooldownImage.fillAmount = 1f; // 쿨 완료
        }
    }
}
