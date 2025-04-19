using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIPlayerData : MonoBehaviour
{
    [Header("체력")]
    [SerializeField]
    private GameObject[] hpBars; // 체력 이미지

    [Header("코인")]
    [SerializeField]
    private TextMeshProUGUI textCoin; // 플레이어가 가진 코인 수를 나타내는 텍스트
    [Header("총알")]
    [SerializeField]
    private TextMeshProUGUI textBullet; // 플레이어가 가진 총알 수 
    [Header("구급약")]
    [SerializeField]
    private TextMeshProUGUI textMedicKit; // 플레이어가 가진 구급약 수

    public void SetHpAll(int currentHp)
    {
        // Debug.Log("체력 UI 작동");
        for (int i = 0; i < hpBars.Length; i++)
        {
            Animator anim = hpBars[i].GetComponent<Animator>();

            if (anim == null) continue;

            if (i < currentHp)
            {
                // Debug.Log("체력 UI 애니메이션작동");
                anim.ResetTrigger("Damage");
                anim.SetTrigger("Heal");
            }
            else
            {
                // Debug.Log("체력 UI 애니메이션작동");
                anim.ResetTrigger("Heal");
                anim.SetTrigger("Damage");
            }
        }
    }

    public void SetCoin(int _Count) // 코인 업데이트
    {
        textCoin.text = $"x {_Count}";
    }

    public void SetBullet(int _bullet) // 총알 업데이트
    {
        textBullet.text = $"x {_bullet}";
    }

    public void SetMedicKit(int _Count) //구급약 업데이트
    {
        textMedicKit.text = $"x {_Count}";
    }


}
