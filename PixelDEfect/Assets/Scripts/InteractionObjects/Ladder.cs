using System.Collections.Generic;
using PlayerStates;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private BoxCollider2D topPlatform;
    [SerializeField] private int ladderLength = 1;
    [SerializeField] private GameObject ladderSegmentPrefab;
    [SerializeField] private float segmentHeight = 1f;
    
    private BoxCollider2D ladderCollider;
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
            ladderCollider.size = new Vector2(ladderCollider.size.x, segmentHeight * ladderLength);
            ladderCollider.offset = new Vector2(0, -segmentHeight * ladderLength / 2f + segmentHeight / 2f);
        }

        // 사다리 스프라이트가 없고 프리팹이 지정된 경우에만 세그먼트 생성
        if (ladderSegmentPrefab != null)
        {
            for (int i = 1; i < ladderLength; i++)
            {
                Vector3 segmentPosition = transform.position + new Vector3(0, -i * segmentHeight, 0);
                
                GameObject segment = Instantiate(ladderSegmentPrefab, segmentPosition, Quaternion.identity);
                segment.transform.SetParent(transform);
                
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
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            float vertical = player.VerticalInput();

            if (Mathf.Abs(vertical) > 0 && !player.IsOnLadder)
            {
                player.ChangeState(new Climb());
                topPlatform.isTrigger = true;
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
                player.ChangeState(new Idle());
            }
            
            topPlatform.isTrigger = false;
        }
    }
}
