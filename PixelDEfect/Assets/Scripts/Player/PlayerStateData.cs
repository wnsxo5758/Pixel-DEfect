using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStateData
{
    public int currentHp;
    public int maxHp;
    public bool hasWeapon;
    public string weaponPrefabName;
    public Vector3 spawnPosition;
    public HashSet<SkillType> unlockedSkills = new HashSet<SkillType>();

    public PlayerStateData()
    {
        currentHp = 4;
        maxHp = 4;
        hasWeapon = false;
        weaponPrefabName = "";
        spawnPosition = Vector3.zero;
        unlockedSkills = new HashSet<SkillType>();
    }
}
