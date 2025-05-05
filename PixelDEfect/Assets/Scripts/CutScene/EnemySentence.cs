using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySentence : MonoBehaviour
{
    public string[] sentences;
    public Transform chatTr;
    public GameObject chatBoxPrefab;

    private void Start()
    {
        Invoke("TalkEnemy", 5f);
    }

    public void TalkEnemy()
    {
        GameObject go = Instantiate(chatBoxPrefab, chatTr);
        go.GetComponent<ChatSystem>().Ondialogue(sentences, chatTr);
        Invoke("TalkEnemy", 5f);
    }
}
