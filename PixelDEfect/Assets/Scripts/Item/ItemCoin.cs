using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCoin : ItemBase
{
    [SerializeField]
    private int value; // 코인의 가치

    public int Value => value;

    public override void UpdateCollision(Transform target)
    {
        target.GetComponent<PlayerData>().Coin += value;
        Debug.Log($"플레이어는 {value}의 코인을 흭득");
        Destroy(gameObject);
    }
}
