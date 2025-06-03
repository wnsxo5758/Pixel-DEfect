using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BossEvents
{
    // 보스 사망 이벤트
    public static System.Action<ManagerRobotBoss> OnBossDefeated;
    
    // 보스 잔해 변환 완료 이벤트
    public static System.Action<ManagerRobotBoss> OnBossTransformedToRemains;
    
    // 보스 사망 이벤트 발생
    public static void BossDefeated(ManagerRobotBoss boss)
    {
        Debug.Log($"BossEvents: 보스 사망 이벤트 발생 - {boss.name}");
        OnBossDefeated?.Invoke(boss);
    }
    
    // 보스 잔해 변환 완료 이벤트 발생
    public static void BossTransformedToRemains(ManagerRobotBoss boss)
    {
        Debug.Log($"BossEvents: 보스 잔해 변환 완료 이벤트 발생 - {boss.name}");
        OnBossTransformedToRemains?.Invoke(boss);
    }
    
    // 모든 이벤트 구독 해제 (씬 전환 시 메모리 누수 방지)
    public static void ClearAllEvents()
    {
        OnBossDefeated = null;
        OnBossTransformedToRemains = null;
    }
}
