using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CutScene/DialogueData")]
public class DialogueData : ScriptableObject
{
    [TextArea]
    public List<string> lines;
}
