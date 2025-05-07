using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySentence : MonoBehaviour
{
    public string[] sentences;                  // 일반 대사
    public string[] deathSentences;             // 사망 대사
    public Transform chatTr;
    public GameObject chatBoxPrefab;

    private bool deathDialogueShown = false;    // 대사 중복 방지
    private GameObject currentChatBox;          // 현재 출력 중인 말풍선
    private bool isDead = false;                // 사망 여부 체크

    public void ShowDeathDialogue()
    {
        if (isDead) return;

        isDead = true;

        // 기존 말풍선 강제 제거
        if (currentChatBox != null)
        {
            Destroy(currentChatBox);
        }

        currentChatBox = Instantiate(chatBoxPrefab, chatTr);
        currentChatBox.GetComponent<ChatSystem>().Ondialogue(deathSentences, chatTr);

        // 반복 호출 중단
        CancelInvoke("TalkEnemy");
    }

    private void Start()
    {
        Invoke("TalkEnemy", 5f);
    }

    public void TalkEnemy()
    {
        // 사망 중이면 실행 X
        if (isDead) return;

        // 기존 말풍선이 있으면 제거
        if (currentChatBox != null)
        {
            Destroy(currentChatBox);
        }

        currentChatBox = Instantiate(chatBoxPrefab, chatTr);
        currentChatBox.GetComponent<ChatSystem>().Ondialogue(sentences, chatTr);

        Invoke("TalkEnemy", 5f);
    }
}
