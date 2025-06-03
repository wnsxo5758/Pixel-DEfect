using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private int checkpointID;
    [SerializeField] private bool isActivated = false;
    [SerializeField] private bool respawn = false;
    [SerializeField] private GameObject visualEffect;
    [SerializeField] private BoxCollider2D collider;

    public bool IsRespawnPoint => respawn;
    public int CheckpointID => checkpointID;
    
    private void Start()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        // 체크포인트 활성화
        isActivated = true;

        Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y - collider.size.y/2 + 1f);

        if (respawn)
        {
            // 리스폰 체크포인트로 등록
            CheckpointManager.Instance.SetRespawnCheckpoint(checkpointID, spawnPos);
        }
        else
        {
            // 낙사 체크포인트로 등록
            CheckpointManager.Instance.SetFallbackCheckpoint(checkpointID, spawnPos);
        }
    }
}
