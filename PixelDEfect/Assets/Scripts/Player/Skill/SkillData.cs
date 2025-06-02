using UnityEngine;

[System.Serializable]
public class SkillData
{
    public SkillType skillType;
    public string skillName;
    public string description;
    public Sprite skillIcon;
    public bool isUnlocked;

    public SkillData(SkillType type, string name, string desc, Sprite icon = null)
    {
        skillType = type;
        skillName = name;
        description = desc;
        skillIcon = icon;
        isUnlocked = false;
    }
}
