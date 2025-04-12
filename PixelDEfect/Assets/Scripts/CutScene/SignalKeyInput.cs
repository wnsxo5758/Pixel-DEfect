using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalKeyInput : MonoBehaviour
{
    private bool simulateFKey = false;

    void Update()
    {
        // 실제 F 키 입력 처리
        if (Input.GetKeyDown(KeyCode.F))
        {
            TriggerFKeyAction();
        }

        // 자동 입력 처리
        if (simulateFKey)
        {
            simulateFKey = false;
            TriggerFKeyAction();
        }
    }

    public void SimulateFKey()
    {
        Debug.Log("Timeline에서 F 키 자동 입력됨!");
        simulateFKey = true;
    }

    // F 키 입력 효과가 실행되는 곳
    private void TriggerFKeyAction()
    {
        Debug.Log("F 키 입력 효과 실행!");
    }
}
