using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HPIcon : MonoBehaviour
{
    [Header("현재 체력")]
    [SerializeField]
    private int currentHP;

    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetHP(int _currentHP)
    {
        currentHP = _currentHP;
        SetHPAnim();
    }
    public void SetState(int damage) 
    {
        currentHP -= damage;
        SetHPAnim();
    }

    private void SetHPAnim()
    {
        animator.SetInteger("currentHP", currentHP);
    }
}
