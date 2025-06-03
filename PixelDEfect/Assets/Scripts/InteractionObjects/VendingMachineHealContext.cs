using UnityEngine;

[System.Serializable]
public class VendingMachineHealContext
{
    [Header("회복 설정")] 
    public int healAmount;      // 총 회복량
    public float healRate;      // 초당 회복량
    public VendingMachine vendingMachine;

    [Header("이동 설정")] 
    public Vector3 targetPosition;
    public float moveSpeed;
    public float positionTolerance;

    [Header("상태 정보")] 
    public bool hasReachedPosition;
    public float totalHealTime;
    public float elapsedTime;
    public int healedAmount;

    public VendingMachineHealContext(VendingMachine machine, Vector3 target, float speed, float tolerance, int heal,
        float rate)
    {
        vendingMachine = machine;
        targetPosition = target;

        moveSpeed = speed;
        positionTolerance = tolerance;
        healAmount = heal;
        healRate = rate;
        
        hasReachedPosition = false;
        totalHealTime = 0;
        elapsedTime = 0;
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
    /// 목표 위치에 도달했는지 확인
    /// </summary>
    public bool IsAtTargetPosition(Vector3 currentPosition)
    {
        float xDistance = Mathf.Abs(currentPosition.x - targetPosition.x);
        return xDistance <= positionTolerance;
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
