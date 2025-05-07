using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySentence : MonoBehaviour
{
    public string[] sentences;                  // 일반 대사
    public string[] deathSentences;             // 사망 대사
    public Transform chatTr;
    public GameObject chatBoxPrefab;

    public Transform player;                    // 플레이어 Transform
    public float distanceThreshold = 5f;        // 대사 활성화 거리
    public float sentenceDelay = 3f;            // 대사 딜레이
    private bool deathDialogueShown = false;    // 대사 중복 방지

    private GameObject currentChatBox;          // 현재 출력 중인 말풍선
    private bool isDead = false;                // 사망 여부 체크
    private bool isTalking = false;             // 일반 대사 루프 중 여부


    private void Update()
    {
        if (isDead || isTalking || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= distanceThreshold)
        {
            StartCoroutine(RandomDialogueLoop());
        }
    }

    private IEnumerator RandomDialogueLoop()
    {
        isTalking = true;

        while (!isDead)
        {
            if (sentences.Length == 0) yield break;

            // 랜덤 문장 선택
            string randomLine = sentences[Random.Range(0, sentences.Length)];

            // 기존 말풍선 제거
            if (currentChatBox != null)
            {
                Destroy(currentChatBox);
            }

            // 새 말풍선 생성
            currentChatBox = Instantiate(chatBoxPrefab, chatTr);
            currentChatBox.GetComponent<ChatSystem>().Ondialogue(new string[] { randomLine }, chatTr);

            // 딜레이 후 다음 문장
            yield return new WaitForSeconds(sentenceDelay);
        }
    }

    public void ShowDeathDialogue()
    {
        if (isDead) return;

        isDead = true;

        // 루프 중단
        StopAllCoroutines();

        // 기존 말풍선 제거
        if (currentChatBox != null)
        {
            Destroy(currentChatBox);
        }

        // 사망 대사 출력
        currentChatBox = Instantiate(chatBoxPrefab, chatTr);
        currentChatBox.GetComponent<ChatSystem>().Ondialogue(deathSentences, chatTr);
    }
}
