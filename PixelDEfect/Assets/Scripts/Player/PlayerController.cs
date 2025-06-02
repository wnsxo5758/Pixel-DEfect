using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("스테이지")]
    [SerializeField] private StageData stageData;

    [Header("디버그 설정")] 
    [SerializeField] private bool enableDeveloperDebug = false;
    

    [Header("웅크리기")] 
    [SerializeField] private float crouchCheckDistance = 0.5f;
    [SerializeField] private LayerMask aboveLayer;

    [Header("구르기")] 
    [SerializeField] private float rollCooldown = 1f;

    [Header("리스폰 딜레이")] 
    [SerializeField] private float respawnDelay = 2f;
    
    private MovementRigidbody2D movement;
    private PlayerHp playerHp;
    private PlayerAttack playerAttack;
    private PlayerInteraction playerInteraction;
    private PlayerStateMachine<PlayerController> stateMachine;
    private PlayerAnimator animator;
    private PlayerSound playerSound;
    // 이동 입력 저장용 변수
    private Vector2 moveInput;

    private PlayerInteraction.InteractionType currentInteractionType = PlayerInteraction.InteractionType.None;
    private bool canInteract = false;
    private bool canRoll = true;
    
    public bool IsOnLadder { get; set; } //사다리 
    public bool WantToStand { get; set; } // 앉았을 때 일어날 수 있는 상태
    
    private void Awake()
    {
        playerSound = GetComponentInChildren<PlayerSound>();
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
                    playerSound.JumpSound();
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
            if (GetCurrentState() is PlayerStates.Idle or PlayerStates.Run)
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
        EnablePlayerControl();
        
        ResetAllPlayerStates();

        ResetAnimationsToCurrentState();
        
        StartCoroutine(DelayedRespawn());
    }

    private IEnumerator DelayedRespawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        
        if (playerHp != null)
        {
            playerHp.ResetDeathState();
        }
        
        ChangeState(new PlayerStates.Idle());
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
        UpdateMove(0);
        
        // 상호작용 관련 초기화
        canInteract = false;
        currentInteractionType = PlayerInteraction.InteractionType.None;
        
        // 공격 관련 초기화
        playerAttack.ResetOnRespawn();
        
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
            animator.MovementAnim(0);
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

    public void DebugDeath(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed && enableDeveloperDebug)
        {
            playerHp.TriggerDebugDeath();
        }
    }
        
    public void OnDestroy()
    {
        if (playerHp != null)
        {
            playerHp.OnPlayerDeath -= OnPlayerDeath;
        }
    }

    private void OnGUI()
    {
        // 개발자 디버그가 활성화되어 있을 때만 표시
        if (!enableDeveloperDebug) return;
    
        // GUI 스타일 설정
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 16;
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;
    
        // 배경 박스 스타일
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.7f));
        
        // 현재 상태 정보 수집
        string currentStateName = GetCurrentStateName();
    
        // 화면 왼쪽 상단에 상태 정보 표시
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
    
        GUILayout.BeginVertical(boxStyle);
    
        GUILayout.Label("=== Player Debug Info ===", labelStyle);
        GUILayout.Space(5);
    
        GUILayout.Label($"Current State: {currentStateName}", labelStyle);
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    // 현재 상태 이름을 문자열로 반환
    public string GetCurrentStateName()
    {
        var currentState = GetCurrentState();
        if (currentState == null) return "None";
    
        // 상태 타입 이름에서 네임스페이스 제거
        string fullName = currentState.GetType().Name;
        return fullName;
    }
    
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = color;
    
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
