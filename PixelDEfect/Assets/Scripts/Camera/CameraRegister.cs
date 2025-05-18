using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraRegister : MonoBehaviour
{
    private void OnEnable()
    {
        CameraChange.Register(GetComponent<CinemachineVirtualCamera>());   
    }

    private void OnDisable()
    {
        CameraChange.Unregister(GetComponent<CinemachineVirtualCamera>());
    }
}
