using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("스킬 설정")] 
    [SerializeField] private SkillData[] allSkills;
    [SerializeField] private bool debug = false;

    [Header("스킬 획득 UI")] 
    [SerializeField] private GameObject skillAcquiredUI;
    [SerializeField] private float uiDisplayDuration = 3f;
    
    // 스킬 획득 이벤트
    public System.Action<SkillType> OnSkillAcquired;

    private HashSet<SkillType> unlockedSkills = new HashSet<SkillType>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
            InitializeSkills();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSkills()
    {
        // 기본 스킬 데이터 초기화
        if (allSkills == null || allSkills.Length == 0)
        {
            allSkills = new[]
            {
                new SkillData(SkillType.Teleport, "순간이동", "던진 무기의 위치로 순간이동할 수 있습니다."),
                new SkillData(SkillType.TimeStop, "시간 정지", "짧은 시간 동안 시간을 정지시킬 수 있습니다.")
            };
        }
        else if (debug)
        {
            UnlockSkill(SkillType.Teleport);
            UnlockSkill(SkillType.TimeStop);
        }
    }
    
    // 스킬 획득
    public void UnlockSkill(SkillType skillType)
    {
        if (skillType == SkillType.None) return;

        if (!unlockedSkills.Contains(skillType))
        {
            unlockedSkills.Add(skillType);
            
            // 스킬 데이터 업데이트
            for (int i = 0; i < allSkills.Length; i++)
            {
                if (allSkills[i].skillType == skillType)
                {
                    allSkills[i].isUnlocked = true;
                    break;
                }
            }
            
            Debug.Log($"스킬 획득: {GetSkillName(skillType)}");
            
            // 이벤트 호출
            OnSkillAcquired?.Invoke(skillType);
            
            // UI 표시
        }
    }

    // 스킬 보유 확인
    public bool HasSkill(SkillType skillType)
    {
        return unlockedSkills.Contains(skillType);
    }
    
    // 스킬 이름 반환
    public string GetSkillName(SkillType skillType)
    {
        foreach (var skill in allSkills)
        {
            if (skill.skillType == skillType)
                return skill.skillName;
        }
        return "알 수 없는 스킬";
    }
    
    // 스킬 설명 반환
    public string GetSkillDescription(SkillType skillType)
    {
        foreach (var skill in allSkills)
        {
            if (skill.skillType == skillType)
                return skill.description;
        }

        return "";
    }

    // 스킬 데이터 반환
    public SkillData GetSkillData(SkillType skillType)
    {
        foreach (var skill in allSkills)
        {
            if (skill.skillType == skillType)
                return skill;
        }
        
        return null;
    }
    
    // 스킬 획득 UI 표시
    private void ShowSkillAcquiredUI(SkillType skillType)
    {
        if (skillAcquiredUI != null)
        {
            StartCoroutine(DisplaySkillAcquiredUI(skillType));
        }
    }

    private System.Collections.IEnumerator DisplaySkillAcquiredUI(SkillType skillType)
    {
        skillAcquiredUI.SetActive(true);
        
        // UI 컴포넌트 업데이트
        // SkillAcquiredUI uiComponent = skillAcquiredUI.GetComponent<SkillAcquiredUI>();
        // if (uiComponent != null)
        // {
        //     uiComponent.DisplaySkill(GetSkillData(skillType));
        // }

        yield return new WaitForSeconds(uiDisplayDuration);
        
        skillAcquiredUI.SetActive(false);
    }
}
