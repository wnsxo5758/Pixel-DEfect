using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 키")]
    [SerializeField] private KeyCode input = KeyCode.G;

    [Header("레이캐스트")]
    [SerializeField] private float rayDistance = 1f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform interactPoint;

    [Header("스프링 조인트")] 
    [SerializeField] private float springFrequency = 0f;
    [SerializeField] private float springDamping = 0.9f;
    [SerializeField] private float springDistance = 1f;

    private GameObject detectedObject;
    private Rigidbody2D objectRb;
    private SpringJoint2D springJoint;
    public bool IsConnected { get; private set; }
    
    private ButtonBase button; //가까운 버튼
    private DoorBase door; // 가까운 문
    private Transform respawnPoint; // 리스폰 포인트(장애물에 죽을 경우)

    private MovementRigidbody2D movement;

    private void Start()
    {
        movement = GetComponent<MovementRigidbody2D>();
        springJoint = gameObject.AddComponent<SpringJoint2D>();
        springJoint.anchor = interactPoint.localPosition;
        springJoint.enabled = false;
        springJoint.autoConfigureDistance = true;
        springJoint.frequency = springFrequency;
        springJoint.dampingRatio = springDamping;
        springJoint.distance = springDistance;
        
        IsConnected = false;
        interactableLayer = LayerMask.GetMask("Objects");
    }
    
    private void Update()
    {
        DetectInteractableObject();
        
        if(Input.GetKeyDown(input) && button!= null)
        {
            button.ButtonTrigger();
            Debug.Log("버튼을 클릭");
        }
        else if(Input.GetKeyDown(input) && door != null)
        {
            door.ActiveDoor(gameObject);
            Debug.Log("문을 사용");
        }
        
        if (Input.GetKeyDown(input)) 
        {
            if (IsConnected)
            {
                DisconnectObject();
            }
            else if (detectedObject != null)
            {
                if (door != null) return; //문에 서있는 상태에서 상호작용 예외처리
                ConnectObject();
            }
        }
    }
    
    //상호작용 감지
    void DetectInteractableObject()
    {
        Vector2 direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        RaycastHit2D hit = Physics2D.Raycast(interactPoint.position, 
                            direction, rayDistance, interactableLayer);
        Debug.DrawRay(interactPoint.position, direction * rayDistance, Color.red);

        if (hit.collider != null)
        {
            detectedObject = hit.collider.gameObject;
        }
        else
        {
            detectedObject = null;
        }
    }
    
    //오브젝트 연결
    void ConnectObject() 
    {
        if (detectedObject == null) return;
        
        objectRb = detectedObject.GetComponent<Rigidbody2D>();

        if (objectRb != null)
        {
            Vector2 anchor = objectRb.transform.InverseTransformPoint(interactPoint.position);
            springJoint.connectedAnchor = new Vector2(0, anchor.y);
            springJoint.connectedBody = objectRb;
            springJoint.enabled = true;
            IsConnected = true;
            objectRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            movement.InteractSpeed = objectRb.mass;
        }
    }
    
    //오브젝트 연결 해제
    void DisconnectObject() 
    {
        springJoint.enabled = false;
        springJoint.connectedBody = null;
        IsConnected = false;
        objectRb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        objectRb = null;
        movement.InteractSpeed = 1;
    }
    
    //GUI 디버그 표시
    void OnGUI() 
    {
        GUI.Label(new Rect(1000, 10, 300, 20), 
            "연결 상태: " + (IsConnected ? "연결됨" : "연결되지 않음"));
        if (detectedObject != null)
        {
            GUI.Label(new Rect(1000, 30, 300, 20), "감지된 오브젝트:" + detectedObject.name);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Button"))
        {
            button = collision.GetComponent<ButtonBase>();
        }
        else if (collision.CompareTag("Door"))
        {
            door = collision.GetComponent<DoorBase>();
        }
        else if (collision.CompareTag("SpawnPoint"))
        {
            respawnPoint = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Button"))
        {
            button = null;
        }
        else if(collision.CompareTag("Door"))
        {
            door = null;
        }
    }

    public void MoveToSpawnPoint()
    {
        if(respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }
        else
        {
            //리스폰 포인트가 없는 경우 추가예정
        }
    }
}
