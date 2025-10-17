using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 데이터 (HP, 무기, 스킬)의 저장/복원을 관리하는 싱글톤 시스템
/// 책임: 플레이어 데이터 저장, 씬 전환 시 복원, 게임 재시작 시 초기화
/// </summary>
public class PlayerDataManager : Singleton<PlayerDataManager>
{
    [Header("기본 무기 설정")]
    [SerializeField] private GameObject defaultWeaponPrefab;

    [Header("디버그")]
    [SerializeField] private bool debugMode = false;

    // 플레이어 데이터
    private PlayerStateData playerData;
    private bool isFirstStage = true;

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

        // 플레이어 데이터 초기화
        playerData = new PlayerStateData();

        // SceneSystem 이벤트 구독
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.OnSceneLoadStart += OnSceneLoadStart;
            SceneSystem.Instance.OnSceneLoadComplete += OnSceneLoadComplete;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // 이벤트 구독 해제
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.OnSceneLoadStart -= OnSceneLoadStart;
            SceneSystem.Instance.OnSceneLoadComplete -= OnSceneLoadComplete;
        }
    }

    #endregion

    #region Scene Events

    /// <summary>
    /// 씬 로드 시작 시 플레이어 데이터 저장
    /// </summary>
    private void OnSceneLoadStart(string sceneName)
    {
        // 게임플레이 씬에서만 저장
        if (SceneSystem.Instance.IsGameplayScene(SceneSystem.Instance.GetCurrentSceneName()))
        {
            SavePlayerData();
        }

        // 타이틀이나 엔딩으로 가면 초기화
        if (SceneSystem.Instance.IsTitleScene(sceneName) || SceneSystem.Instance.IsEndScene(sceneName))
        {
            ResetPlayerData();
        }
    }

    /// <summary>
    /// 씬 로드 완료 시 플레이어 데이터 복원
    /// </summary>
    private void OnSceneLoadComplete(string sceneName)
    {
        // Stage1이면 첫 스테이지로 설정
        if (sceneName == "Stage1")
        {
            isFirstStage = true;
        }
        else if (SceneSystem.Instance.IsGameplayScene(sceneName))
        {
            isFirstStage = false;
        }

        // 게임플레이 씬에서만 복원
        if (SceneSystem.Instance.IsGameplayScene(sceneName))
        {
            // 약간의 딜레이 후 복원
            StartCoroutine(RestorePlayerDataDelayed());
        }
    }

    #endregion

    #region Save & Restore

    /// <summary>
    /// 플레이어 데이터 저장
    /// </summary>
    public void SavePlayerData()
    {
        GameObject player = GetPlayer();
        if (player == null) return;

        // HP 저장
        PlayerHp playerHp = player.GetComponent<PlayerHp>();
        if (playerHp != null)
        {
            playerData.currentHp = playerHp.GetCurrentHp();
            playerData.maxHp = playerHp.GetMaxHp();
        }

        // 무기 정보 저장
        PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerData.hasWeapon = playerAttack.HasWeapon();
            if (playerData.hasWeapon)
            {
                WeaponBase currentWeapon = playerAttack.GetCurrentWeapon();
                if (currentWeapon != null)
                {
                    playerData.weaponPrefabName = currentWeapon.WeaponName;
                }

                // thrownWeaponPrefab 저장 (던지기에 필요)
                playerData.thrownWeaponPrefab = playerAttack.GetThrownWeaponPrefab();
            }
            else
            {
                playerData.thrownWeaponPrefab = null;
            }
        }

        // 스킬 정보 저장
        SaveSkillData();

        // 위치 저장
        playerData.spawnPosition = player.transform.position;

        if (debugMode)
        {
            Debug.Log($"[PlayerDataManager] 저장: HP {playerData.currentHp}/{playerData.maxHp}, 무기 {playerData.hasWeapon}");
        }
    }

    /// <summary>
    /// 플레이어 데이터 복원 (딜레이)
    /// </summary>
    private System.Collections.IEnumerator RestorePlayerDataDelayed()
    {
        yield return new WaitForSeconds(0.2f);
        RestorePlayerData();
    }

    /// <summary>
    /// 플레이어 데이터 복원
    /// </summary>
    public void RestorePlayerData()
    {
        // 첫 스테이지면 복원하지 않음
        if (isFirstStage)
        {
            if (debugMode) Debug.Log($"[PlayerDataManager] Stage1 - 복원 안함");
            return;
        }

        GameObject player = GetPlayer();
        if (player == null)
        {
            Debug.LogError("[PlayerDataManager] 플레이어를 찾을 수 없습니다!");
            return;
        }

        if (debugMode)
        {
            Debug.Log($"[PlayerDataManager] 복원 시작: HP {playerData.currentHp}/{playerData.maxHp}, 무기 {playerData.hasWeapon}");
        }

        // HP 복원
        PlayerHp playerHp = player.GetComponent<PlayerHp>();
        if (playerHp != null)
        {
            playerHp.SetHp(playerData.currentHp);
        }

        // 무기 복원
        RestoreWeaponData(player);

        // 스킬 복원
        RestoreSkillData();

        // PlayerWarp 참조 업데이트
        UpdatePlayerWarpReference(player);

        if (debugMode)
        {
            Debug.Log($"[PlayerDataManager] 복원 완료");
        }
    }

    #endregion

    #region Weapon Management

    /// <summary>
    /// 무기 데이터 복원
    /// </summary>
    private void RestoreWeaponData(GameObject player)
    {
        PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
        if (playerAttack == null)
        {
            Debug.LogError("[PlayerDataManager] PlayerAttack 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // 무기가 있어야 하는데 없으면 직접 장착
        if (playerData.hasWeapon && !playerAttack.HasWeapon())
        {
            if (defaultWeaponPrefab != null && playerData.thrownWeaponPrefab != null)
            {
                EquipWeaponDirectly(player, playerAttack);

                if (debugMode)
                {
                    Debug.Log($"[PlayerDataManager] 무기 복원 완료");
                }
            }
            else
            {
                Debug.LogError("[PlayerDataManager] 무기 프리팹이 null입니다!");
            }
        }
    }

    /// <summary>
    /// 무기 직접 장착 (WeaponPickup 생성 없이)
    /// </summary>
    private void EquipWeaponDirectly(GameObject player, PlayerAttack playerAttack)
    {
        try
        {
            // WeaponBase 데이터 가져오기
            WeaponBase weaponData = defaultWeaponPrefab.GetComponent<WeaponBase>();
            if (weaponData == null)
            {
                Debug.LogError("[PlayerDataManager] defaultWeaponPrefab에 WeaponBase가 없습니다!");
                return;
            }

            // thrownWeaponPrefab 설정
            playerAttack.SetThrownWeaponPrefab(playerData.thrownWeaponPrefab);

            // WeaponPickup 임시 오브젝트 생성
            GameObject tempPickup = new GameObject("TempWeaponPickup");
            WeaponPickup weaponPickup = tempPickup.AddComponent<WeaponPickup>();

            // WeaponPickup에 필요한 정보 설정 (Reflection 사용)
            var weaponPrefabField = typeof(WeaponPickup).GetField("weaponPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cachedWeaponDataField = typeof(WeaponPickup).GetField("cachedWeaponData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (weaponPrefabField != null && cachedWeaponDataField != null)
            {
                weaponPrefabField.SetValue(weaponPickup, playerData.thrownWeaponPrefab);
                cachedWeaponDataField.SetValue(weaponPickup, weaponData);
            }

            // 무기 픽업 처리
            playerAttack.ProcessWeaponPickup(weaponPickup, null);

            // 임시 오브젝트 제거
            if (tempPickup != null)
            {
                Destroy(tempPickup);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerDataManager] 무기 장착 에러: {e.Message}");
        }
    }

    #endregion

    #region Skill Management

    /// <summary>
    /// 스킬 데이터 저장
    /// </summary>
    private void SaveSkillData()
    {
        if (SkillManager.Instance == null) return;

        playerData.unlockedSkills.Clear();

        if (SkillManager.Instance.HasSkill(SkillType.Teleport))
        {
            playerData.unlockedSkills.Add(SkillType.Teleport);
        }

        if (SkillManager.Instance.HasSkill(SkillType.TimeStop))
        {
            playerData.unlockedSkills.Add(SkillType.TimeStop);
        }
    }

    /// <summary>
    /// 스킬 데이터 복원
    /// </summary>
    private void RestoreSkillData()
    {
        if (SkillManager.Instance == null) return;

        foreach (SkillType skillType in playerData.unlockedSkills)
        {
            if (!SkillManager.Instance.HasSkill(skillType))
            {
                SkillManager.Instance.UnlockSkill(skillType);
            }
        }
    }

    /// <summary>
    /// PlayerWarp 참조 업데이트
    /// </summary>
    private void UpdatePlayerWarpReference(GameObject player)
    {
        PlayerWarp playerWarp = FindObjectOfType<PlayerWarp>();
        if (playerWarp != null)
        {
            playerWarp.player = player.transform;

            if (debugMode)
            {
                Debug.Log($"[PlayerDataManager] PlayerWarp 참조 업데이트 완료");
            }
        }
    }

    #endregion

    #region Data Management

    /// <summary>
    /// 플레이어 데이터 초기화
    /// </summary>
    public void ResetPlayerData()
    {
        playerData = new PlayerStateData();
        isFirstStage = true;

        if (debugMode)
        {
            Debug.Log("[PlayerDataManager] 데이터 초기화");
        }
    }

    /// <summary>
    /// 현재 저장된 데이터 반환
    /// </summary>
    public PlayerStateData GetCurrentData()
    {
        return playerData;
    }

    /// <summary>
    /// 첫 스테이지인지 확인
    /// </summary>
    public bool IsFirstStage()
    {
        return isFirstStage;
    }

    #endregion

    #region Utility

    /// <summary>
    /// 플레이어 오브젝트 찾기
    /// </summary>
    private GameObject GetPlayer()
    {
        // PlayerPersistenceSystem에서 먼저 찾기
        if (PlayerPersistenceSystem.Instance != null)
        {
            GameObject player = PlayerPersistenceSystem.Instance.GetPersistentPlayer();
            if (player != null) return player;
        }

        // 없으면 씬에서 찾기
        return GameObject.FindGameObjectWithTag("Player");
    }

    #endregion
}
