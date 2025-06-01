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
    private void Start()
    {
        SetState(currentHP); 
    }

    public void SetState(int damage) 
    {
        currentHP -= damage;
        SetHPAnim(currentHP);
    }

    private void SetHPAnim(int _currentHP)
    {
        animator.SetInteger("currentHP", _currentHP);
    }
}
