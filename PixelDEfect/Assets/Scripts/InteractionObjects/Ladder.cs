using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private BoxCollider2D topPlatform;
    [SerializeField] private GameObject ladderSegmentPrefab;
    [SerializeField] private float segmentHeight;
    [SerializeField] private int ladderLength = 1;
    
    [Header("TopPlatform")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectionRadius = 0.5f;

    private Vector3 detectionPoint = Vector3.zero;
    
    private BoxCollider2D ladderCollider;
    private float ladderHeight;
    private readonly List<GameObject> ladderSegments = new List<GameObject>(); 

    private void Awake()
    {
        if (topPlatform == null)
        {
            topPlatform = transform.GetComponentInChildren<BoxCollider2D>();
        }
        
        ladderCollider = GetComponent<BoxCollider2D>();
        if (ladderCollider == null)
        {
            ladderCollider = gameObject.AddComponent<BoxCollider2D>();
            ladderCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        ladderHeight = ladderCollider.size.y;
        GenerateLadder();
    }
    
    // 에디터에서 값이 변경될 때 사다리를 업데이트하는 기능
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ClearLadder();
            GenerateLadder();
        }
    }
    
    private void GenerateLadder()
    {
        ClearLadder();

        if (ladderCollider != null)
        {
            ladderCollider.size = new Vector2(ladderCollider.size.x, ladderHeight + segmentHeight * (ladderLength - 1));
            ladderCollider.offset = new Vector2(ladderCollider.offset.x, 
                                    ladderHeight - (ladderHeight + segmentHeight * (ladderLength - 1)) / 2f);
        }

        // 사다리 스프라이트가 없고 프리팹이 지정된 경우에만 세그먼트 생성
        if (ladderSegmentPrefab != null)
        {
            for (int i = 1; i < ladderLength; i++)
            {
                Vector3 segmentPosition = transform.position + new Vector3(0, -i * segmentHeight, 0);
                GameObject segment = Instantiate(ladderSegmentPrefab, segmentPosition, Quaternion.identity);
                segment.transform.SetParent(transform);
                segment.transform.localScale = Vector3.one;
                ladderSegments.Add(segment);
            }
        }
    }
    
    private void ClearLadder()
    {
        // 기존 생성된 세그먼트 제거
        foreach (var segment in ladderSegments)
        {
            if (segment != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(segment);
                }
                else
                {
                    DestroyImmediate(segment);
                }
            }
        }

        ladderSegments.Clear();
    }

    public void SetLadderLength(int newLength)
    {
        if (newLength > 0 && newLength != ladderLength)
        {
            ladderLength = newLength;
            ClearLadder();
            GenerateLadder();
        }
    }

    private bool DetectPlayerOnTop()
    {
        detectionPoint = topPlatform.transform.position + new Vector3(0, topPlatform.size.y / 2f, 0);
        
        Collider2D playerCollider = Physics2D.OverlapCircle(
            detectionPoint,
            detectionRadius,
            playerLayer);

        return playerCollider != null;
    }

    private float PlayerFlipX(float x)
    {
        return transform.localScale.x < 0 ? - 1f : 1f;
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            float vertical = player.VerticalInput();

            if (!player.IsOnLadder)
            {
                if (DetectPlayerOnTop())
                {
                    if (vertical < 0f)
                    {
                        topPlatform.isTrigger = true;
                        
                        player.transform.localScale =
                            new Vector3(PlayerFlipX(player.transform.localScale.x) * Mathf.Abs(player.transform.localScale.x),
                                player.transform.localScale.y, player.transform.localScale.z);
                        
                        player.IsOnLadder = true;
                        player.ChangeState(new PlayerStates.Climb());
                    }
                }
                else if (Mathf.Abs(vertical) > 0f)
                {
                    topPlatform.isTrigger = false;
                    
                    player.transform.localScale =
                        new Vector3(PlayerFlipX(player.transform.localScale.x) * Mathf.Abs(player.transform.localScale.x),
                            player.transform.localScale.y, player.transform.localScale.z);

                    CenterPlayerOnLadder(player);
                    
                    player.IsOnLadder = true;
                    player.ChangeState(new PlayerStates.Climb());
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player.IsOnLadder)
            {
                player.ChangeState(new PlayerStates.Idle());
            }
            
            topPlatform.isTrigger = false;
        }
    }

    private void CenterPlayerOnLadder(PlayerController player)
    {
        // 사다리 중앙 X 좌표
        float ladderCenterX = ladderCollider.offset.x;
        
        // 플레이어 현재 위치
        Vector3 playerPos = player.transform.position;
        
        // X 좌표만 수정
        player.transform.position = new Vector3(ladderCenterX, playerPos.y, playerPos.z);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(detectionPoint, detectionRadius);
    }
}
