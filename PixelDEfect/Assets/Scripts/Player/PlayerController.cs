using PlayerStates;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("스테이지")]
    [SerializeField] private StageData stageData;

    [Header("웅크리기")] 
    [SerializeField] private float crouchCheckDistance = 0.5f;
    [SerializeField] private LayerMask aboveLayer;

    [Header("구르기")] 
    [SerializeField] private float rollCooldown = 1f;
    private bool canRoll = true;
    
    private MovementRigidbody2D movement;
    private PlayerHp playerHp;
    private PlayerAttack playerAttack;
    private PlayerInteraction playerInteraction;
    private PlayerStateMachine<PlayerController> stateMachine;

    public bool IsOnLadder { get; set; } //사다리 
    public bool IsCrouching { get; set; } //웅크리기
    public bool IsRolling { get; set; }
    
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
        InputManager.Instance.OnRollPressed += OnRoll;

        if (playerHp != null)
        {
            playerHp.OnPlayerDeath += OnPlayerDeath;
        }
        
        InputManager.Instance.SetCanHold(true);
    }
    
    private void Update()
    {
        if (playerHp != null && playerHp.IsDead) return;
        
        stateMachine.Execute();
    }

    //입력 관련 메소드
    public float HorizontalInput() // 좌우 입력
    {
        return InputManager.Instance.HorizontalInput;
    }

    public float VerticalInput() // 상하 입력 (사다리)
    {
        float y = InputManager.Instance.VerticalInput;

        return y;
    }

    public void OnJump() // 점프 입력
    {
        if (movement.IsGrounded)
        {
            if (IsCrouching && !HasSpaceAbove()) return;
            
            movement.Jump();
            ChangeState(new Jump());
        }
    }

    public void OnCrouch() // 웅크리기 입력
    {
        if (movement.IsGrounded && !IsCrouching)
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

    public void OnRoll() // 구르기 입력
    {
        if (canRoll && movement.IsGrounded && !IsStateLimited())
        {
            ChangeState(new Roll());
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
            ChangeState(new Jump());
        }
    }

    public IEnumerator StartRollCoroutine()
    {
        canRoll = false;
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    public bool CanTakeDamage(GameObject damageSource)
    {
        // 구르기 중이 아니면 항상 데미지를 받음
        if (GetCurrentState() is not Roll) return true;
        
        // 구르기 중일 때 타격 주체 판단
        if (damageSource != null)
        {
            // 적 투사체나 적의 공격은 회피
            if (damageSource.CompareTag("Enemy") || 
                damageSource.CompareTag("EnemyProjectile"))
            {
                return false;
            }

            // 환경 장애물은 회피 불가
            if (damageSource.CompareTag("Obstacle"))
            {
                return true;
            }
        }

        return true;
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

    private bool IsStateLimited()
    {
        var currentState = stateMachine.CurrentState;
        return currentState is Crawl || currentState is Hold || currentState is Climb ||
               currentState is Attack || currentState is Roll;
    }
    
    private void OnPlayerDeath()
    {
        DisablePlayerControl();
    }
    
    private void DisablePlayerControl()
    {
        // 물리 이동 정지
        if (movement != null)
        {
            movement.DisableRigidbody();
        }
        
        // 다른 컴포넌트 비활성화
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }
        
        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }

        // 입력 이벤트 해제
        UnSubscribeInputEvents();
    }

    private void UnSubscribeInputEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed -= OnJump;
            InputManager.Instance.OnHoldPressed -= OnHold;
            InputManager.Instance.OnCrouchPressed -= OnCrouch;
            InputManager.Instance.OnCrouchReleased -= UnCrouch;
            InputManager.Instance.OnLadderJumpPressed -= OnLadderJump;
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

    public void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpPressed -= OnJump;
            InputManager.Instance.OnCrouchPressed -= OnCrouch;
            InputManager.Instance.OnHoldPressed -= OnHold;
            InputManager.Instance.OnCrouchReleased -= UnCrouch;
            InputManager.Instance.OnRollPressed -= OnRoll;
        }

        if (playerHp != null)
        {
            playerHp.OnPlayerDeath -= OnPlayerDeath;
        }
    }
}
