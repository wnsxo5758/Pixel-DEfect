using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFlow : MonoBehaviour
{
    [SerializeField]
    private float riseSpeed = 1f; // 물이 올라가는 속도

    [SerializeField]
    private float targetY = 5f; // 물이 도달해야 할 Y 위치

    private bool isRising = true;

    void Update()
    {
        if (!isRising) return;

        // 현재 위치
        Vector3 currentPosition = transform.position;

        // 목표 위치보다 아래에 있으면 위로 이동
        if (currentPosition.y < targetY)
        {
            float newY = Mathf.MoveTowards(currentPosition.y, targetY, riseSpeed * Time.deltaTime);
            transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
        }
        else
        {
            isRising = false; // 목표 지점 도달 시 멈춤
        }
    }

    public void StartFlow()
    {
        isRising = true;
    }

    public void StopFlow()
    {
        isRising = false;
    }
}
