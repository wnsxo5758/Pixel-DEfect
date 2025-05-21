using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private float respawnInvulnerabilityTime = 2f;
    
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
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
        }
        
        // 딜레이 
        yield return new WaitForSeconds(respawnDelay);
        
        // 체크포인트 매니저가 있는지 확인
        if (CheckpointManager.Instance != null)
        {
            // 플레이어 위치 및 상태 복원
            RespawnPlayer();
            
            // 플레이어 활성화 및 컨트롤러 복원
            if (player != null)
            {
                PlayerController controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.enabled = true;
                }
                
                // 체크포인트에서 부활
                CheckpointManager.Instance.RespawnCheckpoint(player);
            }
        }
        else
        {
            RestartGame();
        }
    }

    // 플레이어 부활 및 상태 복원
    private void RespawnPlayer()
    {
        if (player == null) return;
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
}
