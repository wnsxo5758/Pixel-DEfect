using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    [SerializeField]
    private GameObject spawnEnemy; // ½ºÆùµÉ wjr


    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Instantiate(spawnEnemy, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

}
