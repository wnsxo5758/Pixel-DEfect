using UnityEngine;

public class ManaCollider : MonoBehaviour
{
    [Header("마나 콜라이더 설정")] 
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private GameObject visualEffect;

    private ManagerRobotBoss bossReference;
    private CircleCollider2D collider2D;
    private bool playerInRange = false;

    private void Awake()
    {
        collider2D = GetComponent<CircleCollider2D>();
        if (collider2D == null)
        {
            collider2D = gameObject.AddComponent<CircleCollider2D>();
        }
        
        collider2D.isTrigger = true;
        collider2D.radius = interactionRadius;

        if (playerLayer == 0)
        {
            playerLayer = LayerMask.GetMask("Player");
        }

        gameObject.tag = "Boss";
    }

    private void Start()
    {
        // 시각 효과
        
    }

    private void Update()
    {
        // 보스가 기절 상태가 아니거나 사망했으면 자동 제거
        if (bossReference != null && (!bossReference.IsStunned() || bossReference.CurrentHp <= 0))
        {
            DestroyManaCollider();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 레이어 확인
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;

                if (bossReference != null && bossReference.IsManaDraining())
                {
                    bossReference.StopManaDraining();
                }
            }
        }
    }

    public void SetBossReference(ManagerRobotBoss boss)
    {
        bossReference = boss;
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }

    public bool CanInteract()
    {
        return playerInRange && bossReference != null &&
               bossReference.CanManaDrain() && !bossReference.IsManaDraining();
    }

    private void DestroyManaCollider()
    {
        if (bossReference != null)
        {
            bossReference.StopManaDraining();
        }
        
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        // 상호작용 범위 시각화
        Gizmos.color = playerInRange ? Color.yellow : Color.cyan ;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
