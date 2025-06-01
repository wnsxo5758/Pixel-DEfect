using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIPlayerData : MonoBehaviour
{
    HPIcon hpIcon;
    UIDeath uiDeath;
    private void Awake()
    {
        hpIcon = GetComponentInChildren<HPIcon>();
        uiDeath = GetComponentInChildren<UIDeath>();
    }

    public void SetHpAll(int currentHP)
    {
        hpIcon.SetHP(currentHP);
    }

    public void SetDeathUI()
    {
        uiDeath.ShowDeathUI();
    }

    public void ResetDeathUI()
    {
        uiDeath.SetUp();
    }

}
