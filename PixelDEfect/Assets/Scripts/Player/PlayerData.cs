using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField]
    private UIPlayerData uIPlayerData;

    private int coin = 0;
    private int bullet = 0;

    public int Coin
    {
        set
        {
            Mathf.Clamp(value, 0, 9999);

            uIPlayerData.SetCoin(coin);
        }
        get => coin;
    }

    public int Bullet
    {
        set
        {
            Mathf.Clamp(value, 0, 20);
            uIPlayerData.SetBullet(bullet);
        }
    }

    private void Awake()
    {
        Coin = 10;
        bullet = 10;
    }
}