using UnityEngine;

[System.Serializable]
public class VendingMachineHealContext
{
    [Header("회복 설정")] 
    public int healAmount;      // 총 회복량
    public float healRate;        // 초당 회복량
    public VendingMachine vendingMachine;

    [Header("상태 정보")] 
    public float totalHealTime;
    public float elapsedTime;
    public int healedAmount;
    
    public VendingMachineHealContext()
    {
        healAmount = 8;
        healRate = 2f;
        totalHealTime = 0f;
        elapsedTime = 0f;
        healedAmount = 0;
    }
    
    /// <summary>
    /// 회복 시간 계산
    /// </summary>
    public void CalculateHealTime()
    {
        totalHealTime = healAmount / healRate;
    }
    
    /// <summary>
    /// 회복 진행률 반환 (0~1)
    /// </summary>
    public float GetHealProgress()
    {
        if (totalHealTime <= 0) return 1f;
        return Mathf.Clamp01(elapsedTime / totalHealTime);
    }
    
    /// <summary>
    /// 회복이 완료되었는지 확인
    /// </summary>
    public bool IsHealComplete()
    {
        bool amountComplete = healedAmount >= healAmount;
        bool timeComplete = totalHealTime > 0 && elapsedTime >= totalHealTime;
        return amountComplete || timeComplete;
    }
}
