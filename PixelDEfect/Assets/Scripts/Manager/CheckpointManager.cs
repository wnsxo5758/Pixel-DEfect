using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [System.Serializable]
    public class CheckpointData
    {
        public int id;
        public Vector3 position;
        public bool isActivated;
        public bool isRespawnPoint;
    }

    // 리스폰 체크포인트
    private int currentRespawnCheckpointID = -1;
    private Vector3 respawnPosition;
    
    // 현재 활성화된 체크포인트 ID
    private int currentFallbackCheckpointID = -1;
    private Vector3 fallbackPosition;
    
    // 모든 체크포인트 상태 관리
    private Dictionary<int, CheckpointData> checkpoints = new Dictionary<int, CheckpointData>();

    // 플레이어 상태 데이터
    private int savedPlayerHealth;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            respawnPosition = Vector3.zero;
            fallbackPosition = Vector3.zero;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 체크포인트 활성화 및 플레이어 상태 저장
    public void SetRespawnCheckpoint(int id, Vector3 position)
    {
        currentRespawnCheckpointID = id;
        respawnPosition = position;
        
        // 체크포인트 상태 저장
        CheckpointData data = new CheckpointData
        {
            id = id,
            position = position,
            isActivated = true,
            isRespawnPoint = true
        };

        checkpoints[id] = data;
        
        // 플레이어 상태 저장
        SavePlayerState();
    }

    public void SetFallbackCheckpoint(int id, Vector3 position)
    {
        currentFallbackCheckpointID = id;
        fallbackPosition = position;
        
        // 체크포인트 상태 저장
        CheckpointData data = new CheckpointData
        {
            id = id,
            position = position,
            isActivated = true,
            isRespawnPoint = false
        };

        checkpoints[id] = data;
    }
    
    // 현재 활성화된 체크포인트에서 플레이어 상태 복원
    public void RespawnAtCheckpoint(GameObject player)
    {
        if (currentRespawnCheckpointID != -1)
        {
            // 플레이어 위치 복원
            player.transform.position = respawnPosition;
            
            // 플레이어 상태 복원
            RestorePlayerState(player);
        }
    }

    // 낙사 체크포인트로 플레이어 이동
    public void MoveToFallbackCheckpoint(GameObject player)
    {
        Vector3 targetPosition;
        
        if (currentFallbackCheckpointID != -1)
        {
            targetPosition = fallbackPosition;
        }
        else if (currentRespawnCheckpointID != -1)
        {
            // 낙사 체크포인트가 없으면 리스폰 체크포인트 사용
            targetPosition = respawnPosition;
        }
        else
        {
            return;
        }
        
        // 플레이어 위치만 이동 (상태는 복원하지 않음)
        player.transform.position = targetPosition;
    }
    
    // 플레이어 상태 저장
    private void SavePlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHp playerHp = player.GetComponent<PlayerHp>();

            if (playerHp != null)
            {
                savedPlayerHealth = playerHp.GetCurrentHp();
            }
        }
    }
    
    // 플레이어 상태 복원
    private void RestorePlayerState(GameObject player)
    {
        if (player != null)
        {
            PlayerHp playerHp = player.GetComponent<PlayerHp>();

            if (playerHp != null)
            {
                playerHp.SetHp(playerHp.GetMaxHp());
            }
        }
    }

    public void TeleportToCheckpoint(GameObject player)
    {
        if (currentFallbackCheckpointID != -1)
        {
            // 낙사 체크포인트 우선 사용
            player.transform.position = fallbackPosition;
            Debug.Log("낙사 체크포인트로 즉시 이동");
        }
        else if (currentRespawnCheckpointID != -1)
        {
            // 낙사 체크포인트가 없으면 리스폰 체크포인트 사용
            player.transform.position = respawnPosition;
            Debug.Log("리스폰 체크포인트로 즉시 이동");
        }
        else
        {
            // 체크포인트가 없으면 게임 재시작
            GameManager.Instance.RestartGame();
        }
    }
    
    // 체크포인트 상태 확인
    public bool IsCheckpointActivated(int id)
    {
        return checkpoints.ContainsKey(id) && checkpoints[id].isActivated;
    }
    
    // 씬 로드 시 체크포인트 상태 초기화
    public void ResetCheckpoints()
    {
        checkpoints.Clear();
        currentRespawnCheckpointID = -1;
        currentFallbackCheckpointID = -1;
        respawnPosition = Vector3.zero;
        fallbackPosition = Vector3.zero;
    }
}
