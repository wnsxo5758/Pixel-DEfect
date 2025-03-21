using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorBase : MonoBehaviour
{
    [SerializeField]
    private Transform targetDoor; // 연결된 다른 문
    [SerializeField]
    private string targetScene; // 연결된 다른 씬 
    [SerializeField]
    private Vector2 spawnOffset = new Vector2(0, 0);


    public void ActiveDoor(GameObject player)
    {
        if(!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else if (targetDoor != null)
        {
            player.transform.position = targetDoor.position+(Vector3)spawnOffset;
        }
    }
}
