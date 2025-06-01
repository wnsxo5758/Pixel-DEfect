using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private float respawnInvulnerabilityTime = 2f;

    [Header("투비 컨티뉴")]
    [SerializeField]
    private GameObject toBeCon;
    private GameObject player;
    
    private bool isPlayerDead = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void ProcessPlayerFall(int fallDamage)
    {
        if (player == null) return;
        
        StartCoroutine(FallRespawnProcess(fallDamage));
    }

    private IEnumerator FallRespawnProcess(int fallDamage)
    {
        PlayerHp playerHp = player.GetComponent<PlayerHp>();
        if (playerHp != null)
        {
            playerHp.TakeFallDamage(fallDamage);

            if (playerHp.GetCurrentHp() <= 0)
            {
                PlayerDied();
                yield break;
            }
            
            playerHp.OnInvincibility(respawnInvulnerabilityTime);
        }

        // 즉시 체크포인트로 이동
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.TeleportToCheckpoint(player);
        }
    }
    
    public void PlayerDied()
    {
        isPlayerDead = true;
        DisableTimeEvents();
    }

    // 타임 매니저 이벤트 중지
    private void DisableTimeEvents()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.IsTimeFrozen())
        {
            TimeManager.Instance.ResumeTime();
        }
    }
    
    // 수동 부활 메서드
    public void RespawnPlayer()
    {
        if (!isPlayerDead) return;

        StartCoroutine(ManualRespawnProcess());
    }

    private IEnumerator ManualRespawnProcess()
    {
        // 딜레이 
        yield return new WaitForSeconds(respawnDelay);
        
        // 체크포인트 매니저가 있는지 확인
        if (CheckpointManager.Instance != null && player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.ResetOnRespawn();
            }
            
            CheckpointManager.Instance.RespawnCheckpoint(player);
            
            isPlayerDead = false;
        }
        else
        {
            RestartGame();
        }
    }
    
    public void RestartGame()
    {
        StartCoroutine(RestartTimer());
    }

    private IEnumerator RestartTimer()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        
        yield return new WaitForSeconds(respawnDelay);
        
        CheckpointManager.Instance.ResetCheckpoints();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ToBe()
    {
        toBeCon.SetActive(true);
    }
}
