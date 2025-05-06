using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBT : EnemyBT
{
    [Header("보스 기본 설정")] 
    [SerializeField] protected int currentPhase = 0;
    [SerializeField] protected int maxPhase = 3;
    [SerializeField] protected float[] phaseHpThreshold; // 각 페이즈 전환 체력 비율

    [Header("보스 UI 설정")] 
    [SerializeField] protected GameObject bossHpBar;

    [Header("보스 패턴 설정")] 
    [SerializeField] protected float patternCooldown = 5f;
    [SerializeField] protected float specialAttackProbability = 0.3f;

    protected override void Awake()
    {
        
    }

    protected override void Start()
    {
        
    }

    protected override void Update()
    {
        
    }

    protected virtual void InitializePhasePatterns()
    {
        // 각 페이즈별 패턴 초기화 (자식 클래스에서 구현)
    }

    protected virtual void SetupBossBehaviorTree()
    {
        
    }
    
    
}
