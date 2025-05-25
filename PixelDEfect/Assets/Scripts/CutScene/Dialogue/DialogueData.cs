using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DialogueLine
{
    [TextArea]
    public string line;
    public float waitAfterLine = 3f;  // 각 대사의 유지 시간
}

[CreateAssetMenu(menuName = "CutScene/DialogueData")]
public class DialogueData : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}
