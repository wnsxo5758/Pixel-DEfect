using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance { get; private set; }

    [Header("기본 무기 설정")] 
    [SerializeField] private GameObject defaultWeaponPrefab;

    private PlayerStateData playerState;
    private bool isFirstStage = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
            playerState = new PlayerStateData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        StartCoroutine(RestorePlayerStateDelayed());
    }

    // 플레이어 상태 저장
    public void SavePlayerState(GameObject player)
    {
        if (player == null) return;
        
        PlayerHp playerHp = player.GetComponent<PlayerHp>();
        PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
        
        if (playerHp != null)
        {
            playerState.currentHp = playerHp.GetCurrentHp();
            playerState.maxHp = playerHp.GetMaxHp();
        }
        
        if (playerAttack != null)
        {
            playerState.hasWeapon = playerAttack.HasWeapon();
            if (playerState.hasWeapon)
            {
                WeaponBase currentWeapon = playerAttack.GetCurrentWeapon();
                if (currentWeapon != null)
                {
                    playerState.weaponPrefabName = currentWeapon.WeaponName;
                }
            }
        }
        
        // 스킬 상태 저장
        if (SkillManager.Instance != null)
        {
            playerState.unlockedSkills.Clear();
            if (SkillManager.Instance.HasSkill(SkillType.Teleport))
                playerState.unlockedSkills.Add(SkillType.Teleport);
            if (SkillManager.Instance.HasSkill(SkillType.TimeStop))
                playerState.unlockedSkills.Add(SkillType.TimeStop);
        }
        
        playerState.spawnPosition = player.transform.position;
        
        isFirstStage = false; // 더 이상 첫 번째 스테이지가 아님
        
        Debug.Log($"플레이어 상태 저장 완료 - HP: {playerState.currentHp}/{playerState.maxHp}, 무기: {playerState.hasWeapon}");
    }
    
    // 플레이어 상태 복원
    public void RestorePlayerState(GameObject player)
    {
        if (player == null || isFirstStage) return;
        
        PlayerHp playerHp = player.GetComponent<PlayerHp>();
        PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        // 체력 복원
        if (playerHp != null)
        {
            playerHp.SetHp(playerState.currentHp);
        }
        
        // 무기 상태 복원
        if (playerAttack != null)
        {
            if (!playerState.hasWeapon && !playerAttack.HasWeapon())
            {
                // 무기가 없는 상태에서 넘어왔으면 기본 무기 지급
                if (defaultWeaponPrefab != null)
                {
                    StartCoroutine(GiveDefaultWeapon(player));
                }
            }
        }
        
        // 스킬 상태 복원
        if (SkillManager.Instance != null)
        {
            foreach (SkillType skillType in playerState.unlockedSkills)
            {
                if (!SkillManager.Instance.HasSkill(skillType))
                {
                    SkillManager.Instance.UnlockSkill(skillType);
                }
            }
        }
        
        Debug.Log($"플레이어 상태 복원 완료 - HP: {playerState.currentHp}, 무기: {playerState.hasWeapon}");
    }
    
    // 지연된 플레이어 상태 복원 (씬 로드 후 플레이어가 생성되기를 기다림)
    private IEnumerator RestorePlayerStateDelayed()
    {
        yield return new WaitForSeconds(0.1f); // 플레이어 생성 대기
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            RestorePlayerState(player);
        }
    }
    
    // 기본 무기 지급
    private IEnumerator GiveDefaultWeapon(GameObject player)
    {
        yield return new WaitForSeconds(0.2f); // 플레이어 초기화 대기
        
        if (defaultWeaponPrefab != null)
        {
            // 무기 픽업 오브젝트를 플레이어 근처에 생성
            Vector3 weaponSpawnPos = player.transform.position + Vector3.right * 2f;
            GameObject weaponPickupObj = Instantiate(defaultWeaponPrefab, weaponSpawnPos, Quaternion.identity);
            
            // WeaponPickup 컴포넌트가 있는지 확인하고 없으면 추가
            WeaponPickup weaponPickup = weaponPickupObj.GetComponent<WeaponPickup>();
            if (weaponPickup == null)
            {
                weaponPickup = weaponPickupObj.AddComponent<WeaponPickup>();
            }
            
            Debug.Log("기본 무기(크로우바) 지급 완료");
        }
    }
    
    // 첫 번째 스테이지 여부 확인
    public bool IsFirstStage()
    {
        return isFirstStage;
    }
    
    // 상태 초기화 (게임 재시작 시)
    public void ResetPlayerState()
    {
        playerState = new PlayerStateData();
        isFirstStage = true;
    }
    
    // 현재 저장된 상태 정보 반환
    public PlayerStateData GetCurrentState()
    {
        return playerState;
    }
}
