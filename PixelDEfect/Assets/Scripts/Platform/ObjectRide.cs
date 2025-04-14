using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRide : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform); // 컨테이너를 부모로 설정
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null); // 부모 해제
        }
    }
}

