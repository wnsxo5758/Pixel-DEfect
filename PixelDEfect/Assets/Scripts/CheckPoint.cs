using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private int checkpointID;
    [SerializeField] private bool isActivated = false;
    [SerializeField] private GameObject visualEffect;

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
        
        // 체크포인트 매니저에 등록
        CheckpointManager.Instance.SetActiveCheckpoint(checkpointID, transform.position);
        
        
    }
}
