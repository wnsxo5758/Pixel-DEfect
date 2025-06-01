using UnityEngine;

[System.Serializable]
public class ManaDrainContext
{
    [Header("마나 드레인 설정")] 
    public ManagerRobotBoss targetBoss; // 대상 보스
    public float drainInterval = 0.5f;  // 드레인 간격
    public int drainAmount = 1;         // 드레인 양

    [Header("상태 정보")] 
    public float elapsedTime = 0f;      // 경과 시간
    public float nextDrainTime = 0f;    // 다음 드레인 시간
    public bool isDraining = false;     // 드레인 중인지 여부
    public int totalDrained = 0;        // 총 드레인된 양

    public ManaDrainContext(ManagerRobotBoss boss)
    {
        targetBoss = boss;
        drainInterval = 0.5f;
        drainAmount = 1;
        nextDrainTime = drainInterval;
        isDraining = true;
        elapsedTime = 0f;
        totalDrained = 0;
    }

    public float GetDrainProgress()
    {
        if (targetBoss == null) return 1f;
        return 1f - ((float)targetBoss.CurrentHp / targetBoss.MaxHp);
    }

    public bool IsDrainComplete()
    {
        if (targetBoss == null) return true;
        return targetBoss.CurrentHp <= 0 || !targetBoss.IsStunned();
    }

    public bool CanPerformDrain()
    {
        return isDraining && !IsDrainComplete() && elapsedTime >= nextDrainTime;
    }

    public void OnDrainPerformed()
    {
        totalDrained += drainAmount;
        nextDrainTime = elapsedTime + drainInterval;
    }

    public void StopDrain()
    {
        isDraining = false;
    }
}
