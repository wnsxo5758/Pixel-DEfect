using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIPlayerData : MonoBehaviour
{
    [Header("Ã¼·Â")]
    [SerializeField]
    private GameObject uiHP;


    HPIcon hpIcon;
    private void Awake()
    {
        hpIcon = GetComponentInChildren<HPIcon>();
    }

    public void SetHpAll(int currentHP)
    {
        hpIcon.SetHP(currentHP);
    }



}
