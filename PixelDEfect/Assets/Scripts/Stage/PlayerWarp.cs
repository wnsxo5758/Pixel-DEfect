using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWarp : MonoBehaviour
{
    [Header("플레이어 오브젝트")]
    public Transform player;

    [Header("이동할 위치 리스트 (1~9 키에 대응)")]
    public Vector3[] positions;

    void Update()
    {
        for (int i = 0; i < positions.Length && i < 9; i++) // 숫자키 1~9만 대응
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))       // Alpha1은 1번 키, Alpha2는 2번 키...
            {
                MovePlayerToIndex(i);
            }
        }
    }

    void MovePlayerToIndex(int index)
    {
        if (player != null && index < positions.Length)
        {
            player.position = positions[index];
        }
    }
}
