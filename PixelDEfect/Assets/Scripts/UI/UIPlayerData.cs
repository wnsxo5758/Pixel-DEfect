using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIPlayerData : MonoBehaviour
{
    public HPIcon[] hpIcons;
    UIDeath uiDeath;
    private void Awake()
    {
        uiDeath = GetComponentInChildren<UIDeath>();
    }

    public void SetHpAll(int currentHP)
    {
        hpIcons[0].SetHP(currentHP);
        hpIcons[1].SetHP(currentHP);
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
