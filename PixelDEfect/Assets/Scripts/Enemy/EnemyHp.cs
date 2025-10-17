using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHp : MonoBehaviour,IDamageable
{
    [Header("체력 설정")]
    [SerializeField]
    private int maxHp; // 최대 체력
    [SerializeField]
    private float stunTimer;
    
    private int currentHp; // 현재 체력
    private bool isDeath; // 사망했는가
    private bool isHit; 

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;
    public bool IsDeath => isDeath;

    public bool IsHit => isHit;

    public event Action<int, bool> OnDamaged;
    public event Action OnDied;

    Rigidbody2D rigid2D;
    Blackboard blackboard;
    private void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        blackboard = GetComponent<Blackboard>();
    }

    private void OnEnable()
    {
        currentHp = maxHp;
        isDeath = false;
        isHit = false;
        stunTimer = 0f;


        blackboard.SetValue("CurrentHp", CurrentHp);
        blackboard.SetValue("MaxHp", maxHp);
        blackboard.SetValue("IsHit", false);
        blackboard.SetValue("IsDead", false);
        blackboard.SetValue("StunTimer", 0f);

    }

    public void DecreaseHP(int amount)
    {
    
    }

    public void IncreaseHP(int amount)
    {

    }

    public void Death()
    {

    }
}
