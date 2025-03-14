using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIPlayerData : MonoBehaviour
{
    [Header("체력")]
    [SerializeField]
    private Image[] hpImages; // 체력 이미지

    [Header("코인")]
    [SerializeField]
    private TextMeshProUGUI textCoin; // 플레이어가 가진 코인 수를 나타내는 텍스트
    [Header("총알")]
    [SerializeField]
    private TextMeshProUGUI textBullet; // 플레이어가 가진 총알 수 
    [Header("구급약")]
    [SerializeField]
    private TextMeshProUGUI textMedicKit; // 플레이어가 가진 구급약 수

    public void SetHp(int index, bool isActive) // 체력 업데이트
    {
        hpImages[index].color = isActive == true ? Color.white : Color.black;
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
