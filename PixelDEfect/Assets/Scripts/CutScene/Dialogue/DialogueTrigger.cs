using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogueData;
    public GameObject chatBoxPrefab;
    public Transform chatTarget;

    private GameObject currentChatBox;

    public void TriggerDialogueFromTimeline()
    {
        if (currentChatBox != null)
            Destroy(currentChatBox);

        currentChatBox = Instantiate(chatBoxPrefab, chatTarget);
        var DialogueSystem = currentChatBox.GetComponent<DialogueSystem>();
        DialogueSystem.StartDialogue(dialogueData.lines.ToArray(), chatTarget);
    }
}
