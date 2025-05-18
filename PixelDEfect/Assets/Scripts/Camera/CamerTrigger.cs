using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamerTrigger : MonoBehaviour
{
    public CinemachineVirtualCamera targetCamera;   // 트리거에 닿으면 전환할 카메라

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))             // 플레이어가 트리거에 닿았을 때
        {
            CameraChange.SwitchCamera(targetCamera);
        }
    }
}
