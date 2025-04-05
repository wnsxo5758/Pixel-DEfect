using PlayerStates;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("스테이지")]
    [SerializeField] private StageData stageData;

    [Header("웅크리기")] 
    [SerializeField] private float crouchCheckDistance = 0.5f;
    [SerializeField] private LayerMask aboveLayer;
    
    private MovementRigidbody2D movement;
    private PlayerHp playerHp;
    private PlayerAttack playerAttack;
    private PlayerInteraction playerInteraction;
    private PlayerStateMachine<PlayerController> stateMachine;
    
    public bool IsOnLadder { get; set; } = false; //사다리 
    public bool IsCrouching { get; set; } = false; //웅크리기

    private void Awake()
    {
        movement = GetComponent<MovementRigidbody2D>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHp = GetComponent<PlayerHp>();
        playerInteraction = GetComponent<PlayerInteraction>();
    }

    private void Start()
    {
        stateMachine = new PlayerStateMachine<PlayerController>();
        stateMachine.Setup(this, new Idle());
        stateMachine.SetGlobalState(new StateGlobal());
        
        InputManager.Instance.OnJumpPressed += OnJump;
        InputManager.Instance.OnCrouchPressed += OnCrouch;
        InputManager.Instance.OnHoldPressed += OnHold;
    }
    
    private void Update()
    {
        stateMachine.Execute();
    }

    //입력 관련 메소드
    public float HorizontalInput() // 좌우 입력
    {
        float x = InputManager.Instance.HorizontalInput;
        float offset = 0.5f + InputManager.Instance.SprintInput * 0.5f;

        if (playerInteraction.IsConnected || IsCrouching)
        {
            offset = 0.5f;
        }

        return x * offset;
    }

    public float VerticalInput() // 상하 입력 (사다리)
    {
        float y = InputManager.Instance.VerticalInput;

        return y;
    }
    
    public void OnJump() // 점프 입력
    {
        if (movement.IsGrounded && HasSpaceAbove())
        {
            ChangeState(new Jump());
        }
        
        /* 롱점프 구현 시
         if (Input.GetKey(jumpKeyCode))
        {
            movement.IsLongJump = true;
        }
        else if (Input.GetKeyUp(jumpKeyCode))
        {
            movement.IsLongJump = false;
        }
        */
    }

    public void OnCrouch() // 웅크리기 입력
    {
        if (!IsCrouching)
        {
            IsCrouching = true;
            ChangeState(new Crawl());
        }
    }

    public void UnCrouch() // 웅크리기 해제
    {
        if (IsCrouching && HasSpaceAbove())
        {
            IsCrouching = false;
            ChangeState(new Idle());
        }
    }

    public void OnHold() // 홀드 입력
    {
        if (playerInteraction.CheckHold())
        {
            ChangeState(new Hold());
        }
        else
        {
            if (playerInteraction.IsConnected)
            {
                RevertToPreviousState();
            }
        }
    } 
    
    public void OnLadderJump()
    {
        if (IsOnLadder)
        {
            movement.LadderJump(HorizontalInput());
            ChangeState(new Idle());
        }
    }
    
    public void UpdateMove(float x) // 이동
    {
        if (IsCrouching)
        {
            movement.Crawl(x);
        }
        else
        {
            movement.MoveTo(x);
        }

        float xPos = Mathf.Clamp(transform.position.x, stageData.PlayerLimitMinX, stageData.PlayerLimitMaxX);
        transform.position = new Vector2(xPos, transform.position.y);
    }

    public bool HasSpaceAbove() // 웅크리기 상태에서 머리 위에 충분한 공간이 있는지 확인
    {
        Vector3 rayOrigin = transform.position + new Vector3(0, 1f, 0);
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, 
                            crouchCheckDistance, aboveLayer);
        
        Debug.DrawRay(rayOrigin, Vector2.up * crouchCheckDistance, Color.green);

        return hit.collider == null;
    }
    
    public void UpdateBelowCollision() // 바닥이 플랫폼인지 확인
    {
        if (movement.HitBelowObject != null)
        {
            if (movement.HitBelowObject.TryGetComponent<PlatformBase>(out var platform))
            {
                platform.UpdateCollision(gameObject);
            }
        }
    }
    
    public void ChangeState(State<PlayerController> newState)
    {
        stateMachine.ChangeState(newState);
    }

    public void RevertToPreviousState()
    {
        stateMachine.RevertToPreviousState();
    }

    public State<PlayerController> GetCurrentState()
    {
        return stateMachine.CurrentState;
    }
    
    public void SpriteFlipX(float x)
    {
        if (x == 0) return;
        transform.localScale = new Vector3((x < 0 ? -1.2f : 1.2f), 
                                                        transform.localScale.y, transform.localScale.z);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(1000, 50, 300, 20),
            "State: " + stateMachine.CurrentState.GetType().Name);
    }
}
