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
    }
    
    // 현재 활성화된 체크포인트 ID
    private int currentCheckpointID = -1;
    
    // 체크포인트 위치 저장
    private Vector3 respawnPosition;
    
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
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 체크포인트 활성화 및 플레이어 상태 저장
    public void SetActiveCheckpoint(int id, Vector3 position)
    {
        currentCheckpointID = id;
        respawnPosition = position;
        
        // 체크포인트 상태 저장
        CheckpointData data = new CheckpointData
        {
            id = id,
            position = position,
            isActivated = true
        };

        checkpoints[id] = data;
        
        // 플레이어 상태 저장
        SavePlayerState();
    }
    
    // 현재 활성화된 체크포인트에서 플레이어 상태 복원
    public void RespawnCheckpoint(GameObject player)
    {
        if (currentCheckpointID != -1)
        {
            // 플레이어 위치 복원
            player.transform.position = respawnPosition;
            
            // 플레이어 상태 복원
            RestorePlayerState(player);
        }
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
                playerHp.SetHp(savedPlayerHealth);
            }
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
        currentCheckpointID = -1;
        respawnPosition = Vector3.zero;
    }
}
