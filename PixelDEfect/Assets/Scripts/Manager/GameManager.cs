using System.Collections;
using System.Collections.Generic;
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

    public void ProcessPlayerFall(GameObject player, int fallDamage)
    {
        if (player == null) return;
        
        StartCoroutine(FallRespawnProcess(player, fallDamage));
    }

    private IEnumerator FallRespawnProcess(GameObject player, int fallDamage)
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
            
            playerHp.OnInvincibility(2f);
        }

        // 즉시 체크포인트로 이동
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.TeleportToCheckpoint(player);
        }
    }
    
    public void PlayerDied()
    {
        DisableTimeEvents();
        
        StartCoroutine(RespawnProcess());
    }

    // 타임 매니저 이벤트 중지
    private void DisableTimeEvents()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.IsTimeFrozen())
        {
            TimeManager.Instance.ResumeTime();
        }
    }
    
    private IEnumerator RespawnProcess()
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
