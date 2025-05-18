using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchCamera : MonoBehaviour
{
    public CinemachineVirtualCamera cam1;
    public CinemachineVirtualCamera cam2;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CameraChange.SwitchCamera(cam1);
        }    

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CameraChange.SwitchCamera(cam2);
        }
    }
}
