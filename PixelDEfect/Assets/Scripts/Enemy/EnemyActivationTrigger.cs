using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActivationTrigger : MonoBehaviour
{
    [Header("ActivationEnemys")]
    public GameObject[] objectsToActivate;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))     // 플레이어에만 반응
        {
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                    obj.SetActive(true);    // 오브젝트 활성화
            }

            // 한 번만 실행 후 자기 자신 비활성화 (선택)
            gameObject.SetActive(false);
        }
    }
}
