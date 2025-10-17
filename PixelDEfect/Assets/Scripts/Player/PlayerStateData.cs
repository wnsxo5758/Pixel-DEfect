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
    public GameObject thrownWeaponPrefab; // 던질 수 있는 무기 프리팹 참조
    public Vector3 spawnPosition;
    public List<SkillType> unlockedSkills = new List<SkillType>(); // HashSet → List로 변경 (직렬화 가능)

    public PlayerStateData()
    {
        currentHp = 4;
        maxHp = 4;
        hasWeapon = false;
        weaponPrefabName = "";
        thrownWeaponPrefab = null;
        spawnPosition = Vector3.zero;
        unlockedSkills = new List<SkillType>();
    }
}
