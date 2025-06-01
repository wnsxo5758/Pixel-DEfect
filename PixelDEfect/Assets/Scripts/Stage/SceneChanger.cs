using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;            // 이동할 씬 이름
    [SerializeField] private string targetTag = "Player";   // 충돌할 태그

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
