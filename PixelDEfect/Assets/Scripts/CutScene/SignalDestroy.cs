using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalDestroy : MonoBehaviour
{
    public GameObject target; // 없앨 오브젝트

    public void DestroyTarget()
    {
        Destroy(target); // 진짜로 삭제
    }

    public void DisableTarget()
    {
        target.SetActive(false); // 사라지지만 나중에 다시 활성화 가능
    }
}
