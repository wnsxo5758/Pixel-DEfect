using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogueData;
    public GameObject chatBoxPrefab;
    public Transform chatTarget;

    private GameObject currentChatBox;

    /// Timeline에서 이 함수만 호출하면 됨!
    public void ShowTimelineLine(int lineIndex)
    {
        if (currentChatBox != null)
            Destroy(currentChatBox);

        if (lineIndex < 0 || lineIndex >= dialogueData.lines.Count)
        {
            Debug.LogWarning("잘못된 대사 인덱스");
            return;
        }

        var lineData = dialogueData.lines[lineIndex];

        currentChatBox = Instantiate(chatBoxPrefab, chatTarget);
        var dialogueSystem = currentChatBox.GetComponent<DialogueSystem>();
        dialogueSystem.ShowSingleLine(lineData.line, chatTarget, lineData.waitAfterLine);
    }
}
