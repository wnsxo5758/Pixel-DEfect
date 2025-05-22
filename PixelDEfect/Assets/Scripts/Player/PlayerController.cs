using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("스테이지")]
    [SerializeField] private StageData stageData;

    [Header("웅크리기")] 
    [SerializeField] private float crouchCheckDistance = 0.5f;
    [SerializeField] private LayerMask aboveLayer;

    [Header("구르기")] 
    [SerializeField] private float rollCooldown = 1f;
    
    private MovementRigidbody2D movement;
    private PlayerHp playerHp;
    private PlayerAttack playerAttack;
    private PlayerInteraction playerInteraction;
    private PlayerStateMachine<PlayerController> stateMachine;
    private PlayerAnimator animator;
    
    // 이동 입력 저장용 변수
    private Vector2 moveInput;

    private PlayerInteraction.InteractionType currentInteractionType = PlayerInteraction.InteractionType.None;
    private bool canInteract = false;
    private bool canRoll = true;
    
    public bool IsOnLadder { get; set; } //사다리 
    public bool WantToStand { get; set; } // 앉았을 때 일어날 수 있는 상태
    
    private void Awake()
    {
        movement = GetComponent<MovementRigidbody2D>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHp = GetComponent<PlayerHp>();
        playerInteraction = GetComponent<PlayerInteraction>();
        animator = GetComponentInChildren<PlayerAnimator>();
        stateMachine = new PlayerStateMachine<PlayerController>();
    }

    private void Start()
    {
        // 상태 머신 초기화
        stateMachine.Setup(this, new PlayerStates.Idle());
        stateMachine.SetGlobalState(new PlayerStates.StateGlobal());
        if (playerHp != null)
        {
            playerHp.OnPlayerDeath += OnPlayerDeath;
        }
        
        TimeManager.Instance.UnlockTimeStopAbility();
    }
    
    private void Update()
    {
        if (playerHp != null && playerHp.IsDead) return;
        
        stateMachine.Execute();
    }

    // Move 이벤트
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    // Jump 이벤트
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            // 현재 상태에 따라 점프 처리
            if (GetCurrentState() is PlayerStates.Idle || GetCurrentState() is PlayerStates.Run)
            {
                if (movement.IsGrounded)
                {
                    movement.Jump();

                    if (animator != null)
                    {
                        animator.JumpAnim();
                    }
                    
                    ChangeState(new PlayerStates.Jump());
                }
            }
            // 사다리에서 점프하는 경우
            else if (GetCurrentState() is PlayerStates.Climb)
            {
                OnLadderJump();
            }
        }
    }

    // Crouch 이벤트
    public void OnCrouch(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                OnCrouchDown();
                break;
            case InputActionPhase.Canceled:
                OnCrouchUp();
                break;
        }
    }

    // Roll 이벤트
    public void OnRoll(InputAction.CallbackContext context) 
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (GetCurrentState() is PlayerStates.Idle || GetCurrentState() is PlayerStates.Run)
            {
                if (movement.IsGrounded && canRoll)
                {
                    ChangeState(new PlayerStates.Roll());
                }
            }
        }
    }
    
    // Interact 이벤트
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (playerInteraction != null)
        {
            if (context.phase == InputActionPhase.Started ||
                context.phase == InputActionPhase.Canceled)
            {
                playerInteraction.ProcessInteraction(context);
            }
        }
    }
    
    // 근접 공격 이벤트
    public void OnMeleeAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (playerAttack != null)
            {
                playerAttack.PerformMeleeAttack();
            }
        }
    }
    
    // 텔레포트 이벤트
    public void OnTeleport(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (playerAttack != null)
            {
                playerAttack.PerformTeleport();
            }
        }
    }
    
    // ThrowWeapon 이벤트
    public void OnThrowWeapon(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (playerAttack != null)
            {
                playerAttack.PerformThrowWeapon();
            }
        }
    }
    
    
    // 이동 업데이트
    public void UpdateMove(float input) // 이동
    {
        if (GetCurrentState() is PlayerStates.Crawl)
        {
            movement.Crawl(input);
        }
        else
        {
            movement.MoveTo(input);
        }

        float xPos = Mathf.Clamp(transform.position.x, stageData.PlayerLimitMinX, stageData.PlayerLimitMaxX);
        transform.position = new Vector2(xPos, transform.position.y);
    }

    // 스프라이트 방향 설정
    public void SpriteFlipX(float direction)
    {
        if (direction != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * Mathf.Sign(direction),
                transform.localScale.y,
                transform.localScale.z);
        }
    }
    
    // 수직 입력 값 반환
    public float VerticalInput()
    {
        return moveInput.y;
    }

    // 수평 입력 값 반환
    public float HorizontalInput()
    {
        return moveInput.x;
    }
    
    // 웅크리기 시작
    private void OnCrouchDown()
    {
        if (GetCurrentState() is PlayerStates.Idle || GetCurrentState() is PlayerStates.Run)
        {
            WantToStand = false;
            ChangeState(new PlayerStates.Crawl());
        }
    }
    
    // 웅크리기 종료
    private void OnCrouchUp()
    {
        if (GetCurrentState() is PlayerStates.Crawl)
        {
            if (!HasSpaceAbove())
            {
                WantToStand = true;
                
                return;
            }
            
            ChangeState(new PlayerStates.Idle());
        }
    }

    public void SetInteractionAvailable(bool available, PlayerInteraction.InteractionType type)
    {
        canInteract = available;
        currentInteractionType = type;
    }
    
    // 사다리에서 점프
   private void OnLadderJump()
    {
        if (GetCurrentState() is PlayerStates.Climb)
        {
            animator.LadderJumpAnim();
            movement.LadderJump(HorizontalInput());
            IsOnLadder = false;
            ChangeState(new PlayerStates.Jump());
        }
    }

    public IEnumerator StartRollCoroutine()
    {
        canRoll = false;
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    public bool OnAttackReceived(bool canFreeze)
    {
        if (GetCurrentState() is PlayerStates.Roll rollState)
        {
            if (canFreeze)
            {
                return rollState.CheckDodgeAndTriggerTimeStop(this);
            }

            return true;
        }

        return false;
    }

    // 위에 공간이 있는지 확인 (웅크리기 해제 가능 여부)
    public bool HasSpaceAbove() 
    {
        Vector3 rayOrigin = transform.position + new Vector3(0, 1f, 0);
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, 
                            crouchCheckDistance, aboveLayer);
        
        Debug.DrawRay(rayOrigin, Vector2.up * crouchCheckDistance, hit ? Color.red : Color.green);

        return hit.collider == null;
    }
    
    // 지면 충돌
    public void UpdateBelowCollision() 
    {
        if (movement.HitBelowObject != null)
        {
            if (movement.HitBelowObject.TryGetComponent<PlatformBase>(out var platform))
            {
                platform.UpdateCollision(gameObject);
            }
        }
    }
    
    private void OnPlayerDeath()
    {
        DisablePlayerControl();
    }

    public void ResetOnRespawn()
    {
        if (playerHp != null)
        {
            playerHp.ResetDeathState();
        }
        
        EnablePlayerControl();
        
        ChangeState(new PlayerStates.Idle());

        ResetAllPlayerStates();

        ResetAnimationsToCurrentState();
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
    }

    public void EnablePlayerControl()
    {
        // 물리 속성 다시 활성화
        if (movement != null)
        {
            movement.EnableRigidbody();
        }

        if (playerAttack != null)
        {
            playerAttack.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }
    }

    private void ResetAllPlayerStates()
    {
        // 입력 변수 초기화
        moveInput = Vector2.zero;
        
        // 상호작용 관련 초기화
        canInteract = false;
        currentInteractionType = PlayerInteraction.InteractionType.None;
        
        // 액션 관련 초기화
        canRoll = true;
        
        // 특수 상태 초기화
        IsOnLadder = false;
        WantToStand = false;
    }

    private void ResetAnimationsToCurrentState()
    {
        if (animator != null)
        {
            
            animator.SetCrouchAnim(false);
            animator.SetClimbAnim(false);
            animator.SetHasWeapon(playerAttack != null && playerAttack.HasWeapon());

            animator.ResetAllAnimationStates();
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
    
    public bool IsGrounded() => movement.IsGrounded;
    
    public void OnDestroy()
    {
        if (playerHp != null)
        {
            playerHp.OnPlayerDeath -= OnPlayerDeath;
        }
    }
}
