using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 설정")] 
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private GameObject interactionPrompt;

    [Header("홀드 설정")] 
    [SerializeField] private float holdDistance = 1f;
    [SerializeField] private LayerMask holdableLayer;
    [SerializeField] private float autoReleaseDelay = 0.1f; // 지면에서 떨어졌을 때 딜레이
    [SerializeField] private Vector2 raycastOffset;
    
    // Trigger 방식으로 감지할 오브젝트들
    private ValveButton currentValve;
    private ButtonBase currentButton;
    private DoorBase currentDoor;
    private VendingMachine currentVendingMachine;
    private SkillToken currentSkillToken;
    
    // Raycast로 감지할 오브젝트
    private HoldObject currentHoldObject;
    
    // 원형 범위로 감지할 오브젝트들(무기)
    private WeaponPickup nearbyWeapon;
    private ThrownWeapon nearbyThrownWeapon;

    private ManagerRobotBoss currentManaDrainBoss;
    
    private FixedJoint2D holdJoint;
    private bool isHolding = false;
    private float groundCheckTimer = 0f;
    
    // 상호작용 타입 열거형
    public enum InteractionType
    {
        None,
        WeaponPickup,
        ThrownWeapon,
        ManaDrain,
        Holdable,
        Valve,
        Button,
        Door,
        VendingMachine,
        SkillToken
    }
    
    // 현재 상호작용 타입
    private InteractionType currentInteractionType = InteractionType.None;

    private void Awake()
    {
        // 상호작용 포인트가 없으면 자신의 위치로 설정
        if (interactionPoint == null)
            interactionPoint = transform;
        
        // 상호작용 프롬프트 초기 상태
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        raycastOffset = new Vector2(0, interactionPoint.localPosition.y);
    }
    
    private void Update()
    {
        DetectWeapons();
        
        DetectHoldableObject();
        
        DetermineInteractionType();
        
        CheckHoldObjectGrounded();
    }

    private void DetectWeapons()
    {
        nearbyWeapon = null;
        nearbyThrownWeapon = null;
        
        // 주변 오브젝트 감지
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            interactionPoint.position, interactionRadius, interactableLayer);

        foreach (Collider2D collider in colliders)
        {
            // 무기 픽업
            WeaponPickup weaponPickup = collider.GetComponent<WeaponPickup>();
            if (weaponPickup != null)
            {
                nearbyWeapon = weaponPickup;
                continue;
            }
            
            // 던져진 무기
            ThrownWeapon thrownWeapon = collider.GetComponent<ThrownWeapon>();
            if (thrownWeapon != null && thrownWeapon.IsStuck())
            {
                nearbyThrownWeapon = thrownWeapon;
            }
        }
    }

    private void DetectHoldableObject()
    {
        // 홀딩 중이면 스킵
        if (isHolding) return;
        
        Vector2 raycastDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 raycastOrigin = (Vector2)transform.position + raycastOffset;
        
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, holdDistance, holdableLayer);
        Debug.DrawRay(raycastOrigin, raycastDirection * holdDistance, Color.red);

        if (hit.collider != null)
        {
            HoldObject holdObject = hit.collider.GetComponent<HoldObject>();
            if (holdObject != null && holdObject.IsGrounded)
            {
                currentHoldObject = holdObject;
                return;
            }
        }
        
        currentHoldObject = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;
        
        // 밸브
        if (other.CompareTag("Valve"))
        {
            currentValve = other.GetComponent<ValveButton>();
        }
        // 버튼
        else if(other.CompareTag("Button"))
        {
            currentButton = other.GetComponent<ButtonBase>();
        }
        // 문
        else if (other.CompareTag("Door"))
        {
            currentDoor = other.GetComponent<DoorBase>();
        }
        else if (other.CompareTag("VendingMachine"))
        {
            currentVendingMachine = other.GetComponent<VendingMachine>();
            if (currentVendingMachine != null)
            {
                // 들어왔을 때 프롬프트 처리
            }
        }
        else if (other.CompareTag("Boss"))
        {
            ManagerRobotBoss boss = other.GetComponentInParent<ManagerRobotBoss>();
            if (boss != null && boss.IsStunned() && boss.CanManaDrain())
            {
                currentManaDrainBoss = boss;
            }
        }
        else if (other.CompareTag("SkillToken"))
        {
            SkillToken skillToken = other.GetComponent<SkillToken>();
            if (skillToken != null && !skillToken.IsAcquired())
            {
                currentSkillToken = skillToken;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Valve"))
        {
            currentValve = null;
        }
        else if (other.CompareTag("Button"))
        {
            currentButton = null;
        }
        else if (other.CompareTag("Door"))
        {
            currentDoor = null;
        }
        else if (other.CompareTag("VendingMachine"))
        {
            // 나갔을 때 프롬프트 처리
            currentVendingMachine = null;
        }
        else if (other.CompareTag("Boss"))
        {
            currentManaDrainBoss = null;
        }
        else if (other.CompareTag("SkillToken"))
        {
            currentSkillToken = null;
        }
    }
    
    // 상호작용 타입 결정 (우선순위 설정)
    private void DetermineInteractionType()
    {
        if (currentInteractionType == InteractionType.Valve && currentValve != null && currentValve.isPressing)
        {
            return;
        }

        if (currentInteractionType == InteractionType.Holdable && isHolding)
        {
            return;
        }
        
        // 우선순위에 따라 상호작용 타입 결정
        if (nearbyWeapon != null)
        {
            currentInteractionType = InteractionType.WeaponPickup;
        }
        else if (nearbyThrownWeapon != null)
        {
            currentInteractionType = InteractionType.ThrownWeapon;
        }
        else if (currentManaDrainBoss != null)
        {
            currentInteractionType = InteractionType.ManaDrain;
        }
        else if (currentSkillToken != null)
        {
            currentInteractionType = InteractionType.SkillToken;
        }
        else if (currentHoldObject != null)
        {
            currentInteractionType = InteractionType.Holdable;
        }
        else if (currentValve != null)
        {
            currentInteractionType = InteractionType.Valve;
        }
        else if (currentButton != null)
        {
            currentInteractionType = InteractionType.Button;
        }
        else if (currentDoor != null)
        {
            currentInteractionType = InteractionType.Door;
        }
        else if (currentVendingMachine != null)
        {
            currentInteractionType = InteractionType.VendingMachine;
        }
        else
        {
            currentInteractionType = InteractionType.None;
        }
        
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetInteractionAvailable(currentInteractionType != InteractionType.None, currentInteractionType);
        }
    }

    // 프롬프트 업데이트
    private void UpdateInteractionPrompt()
    {
        if (interactionPrompt == null) return;

        if (currentInteractionType != InteractionType.None)
        {
            interactionPrompt.SetActive(true);

            GameObject targetObject = GetCurrentInteractionTarget();
            if (targetObject != null)
            {
                interactionPrompt.transform.position = targetObject.transform.position + Vector3.up * 0.5f;
            }
        }
        else
        {
            interactionPrompt.SetActive(false);
        }
    }

    private GameObject GetCurrentInteractionTarget()
    {
        switch (currentInteractionType)
        {
            case InteractionType.WeaponPickup:
                return nearbyWeapon?.gameObject;
            case InteractionType.ThrownWeapon:
                return nearbyThrownWeapon?.gameObject;
            case InteractionType.ManaDrain:
                return currentManaDrainBoss.gameObject;
            case InteractionType.SkillToken:
                return currentSkillToken.gameObject;
            case InteractionType.Holdable:
                return currentHoldObject?.gameObject;
            case InteractionType.Valve:
                return currentValve?.gameObject;
            case InteractionType.Button:
                return currentButton?.gameObject;
            case InteractionType.Door:
                return currentDoor?.gameObject;
            case InteractionType.VendingMachine:
                return currentVendingMachine?.gameObject;
            default:
                return null;
        }
    }
    
    // PlayerController에서 호출되는 상호작용 메서드
    public void ProcessInteraction(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            HandleInteractionStart();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            HandleInteractionEnd();
        }
    }

    // 상호작용 시작 처리
    private void HandleInteractionStart()
    {
        // 잡고 있는 물체가 있고, 문이나 밸브 상호작용이 있는 경우 물체 놓기
        if (isHolding && (currentInteractionType == InteractionType.Door ||
                          currentInteractionType == InteractionType.Valve ||
                          currentInteractionType == InteractionType.VendingMachine))
        {
            StopHolding();
        }
        
        // 상호작용 타입에 따라 다른 처리
        switch (currentInteractionType)
        {
            case InteractionType.WeaponPickup:
            case InteractionType.ThrownWeapon:
                PickupWeapon();
                break;
            
            case InteractionType.ManaDrain:
                if (currentManaDrainBoss != null)
                {
                    StartManaDrain();
                }
                break;
            
            case InteractionType.SkillToken:
                AcquireSkillToken();
                break;
            
            case InteractionType.Holdable:
                // 물체 잡기 시작
                if (!isHolding && currentHoldObject != null && currentHoldObject.IsGrounded)
                {
                    StartHolding();
                }
                break;
            
            case InteractionType.Valve:
                // 밸브 회전 시작
                if (currentValve != null)
                {
                    currentValve.isPressing = true;
                    
                    // 밸브 상태로 전환
                    PlayerController controller = GetComponent<PlayerController>();
                    if (controller != null)
                    {
                        controller.ChangeState(new PlayerStates.Valve());
                    }
                }
                break;

            case InteractionType.Button:
                // 버튼 누르기
                if (currentButton != null)
                {
                    currentButton.ButtonTrigger();
                }
                break;
            
            case InteractionType.Door:
                if (currentDoor != null)
                {
                    currentDoor.ActiveDoor(gameObject);
                }
                break;
            case InteractionType.VendingMachine:
                if (currentVendingMachine != null)
                {
                    currentVendingMachine.TryUseVendingMachine(gameObject);
                }
                break;
        }
    }
    
    // 상호작용 종료 처리
    private void HandleInteractionEnd()
    {
        // 현재 밸브를 돌리고 있었다면
        if (currentValve != null && currentValve.isPressing)
        {
            currentValve.isPressing = false;
            
            // 밸브 상태 종료
            PlayerController controller = GetComponent<PlayerController>();
            if (controller != null && controller.GetCurrentState() is PlayerStates.Valve)
            {
                controller.ChangeState(new PlayerStates.Idle());
            }
        }
        
        // 물체를 잡고 있었다면
        if (isHolding)
        {
            StopHolding();
        }
        
        // 마나 드레인 중이었다면
        
    }
    
    // 물체 잡기 시작
    private void StartHolding()
    {
        if (currentHoldObject == null || !currentHoldObject.IsGrounded) return;
        
        isHolding = true;
        groundCheckTimer = 0f;
        
        // 물리 연결 생성
        if (holdJoint == null)
        {
            holdJoint = gameObject.AddComponent<FixedJoint2D>();
        }
        
        Rigidbody2D targetRb = currentHoldObject.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            holdJoint.autoConfigureConnectedAnchor = true;
            holdJoint.connectedBody = targetRb;
            holdJoint.enabled = true;
            
            // 물체 회전 고정
            targetRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        MovementRigidbody2D movement = GetComponent<MovementRigidbody2D>();
        if (movement != null)
        {
            movement.InteractSpeed = targetRb.mass;
        }
        
        // 상태 전환
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ChangeState(new PlayerStates.Hold());
        }
    }
    
    // 물체 잡기 종료
    public void StopHolding()
    {
        if (!isHolding) return;

        isHolding = false;
        groundCheckTimer = 0f;

        if (holdJoint != null)
        {
            Destroy(holdJoint);
            holdJoint = null;
        }
        
        Rigidbody2D targetRb = currentHoldObject.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            // 물체 회전 고정
            targetRb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }
        
        currentHoldObject = null;
        
        MovementRigidbody2D movement = GetComponent<MovementRigidbody2D>();
        if (movement != null)
        {
            movement.InteractSpeed = 1;
        }
        
        // 상태 전환
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null && controller.GetCurrentState() is PlayerStates.Hold)
        {
            controller.ChangeState(new PlayerStates.Idle());
        }
    }

    // 잡고 있는 물체의 지면 접촉 확인
    private void CheckHoldObjectGrounded()
    {
        if (isHolding && currentHoldObject != null)
        {
            // 지면에 닿아있지 않으면
            if (!currentHoldObject.IsGrounded)
            {
                // 타이머 증가
                groundCheckTimer += Time.deltaTime;
                
                // 일정 시간 이상 지면에서 떨어져 있으면 연결 해제
                if (groundCheckTimer >= autoReleaseDelay)
                {
                    StopHolding();
                }
            }
            else
            {
                groundCheckTimer = 0f;
            }
        }
    }

    // 무기 줍기
    private void PickupWeapon()
    {
        // PlayerAttack 컴포넌트에 무기 픽업 요청
        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.ProcessWeaponPickup(nearbyWeapon, nearbyThrownWeapon);
        }
    }

    private void StartManaDrain()
    {
        if (currentManaDrainBoss == null && !currentManaDrainBoss.CanManaDrain() &&
            currentManaDrainBoss.IsManaDraining()) return;
        
        // 마나 드레인 컨텍스트 생성
        ManaDrainContext drainContext = new ManaDrainContext(currentManaDrainBoss);
        
        // 마나 드레인 상태로 전환
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ChangeState(new PlayerStates.ManaDrain(drainContext));
        }
    }

    private void StopManaDrain()
    {
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null && controller.GetCurrentState() is PlayerStates.ManaDrain)
        {
            controller.ChangeState(new PlayerStates.Idle());
        }
    }
    
    // 스킬 토큰 획득 메서드
    private void AcquireSkillToken()
    {
        if (currentSkillToken == null || currentSkillToken.IsAcquired())
            return;

        SkillType skillType = currentSkillToken.GetSkillType();
        
        // 이미 획득한 스킬인지 확인
        if (SkillManager.Instance != null && SkillManager.Instance.HasSkill(skillType))
        {
            Debug.Log($"이미 획득한 스킬입니다: {skillType}");
            return;
        }

        // 스킬 획득 처리
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.UnlockSkill(skillType);
            
            // 토큰 획득 처리
            currentSkillToken.OnTokenAcquired();
            
            // 현재 토큰 참조 제거
            currentSkillToken = null;
        }
        else
        {
            Debug.LogError("SkillManager.Instance가 null입니다!");
        }
    }
    
    // 디버그 시각화
    private void OnDrawGizmosSelected()
    {
        // 무기 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint != null ?
            interactionPoint.position : transform.position, interactionRadius);
    }
    
    public bool IsHolding() => isHolding;
    public bool IsNearValve() => currentInteractionType == InteractionType.Valve && currentValve != null;
    public bool HasNearbyWeapon() => currentInteractionType == InteractionType.WeaponPickup ||
                                    currentInteractionType == InteractionType.ThrownWeapon;
    public ThrownWeapon GetNearbyThrownWeapon() => nearbyThrownWeapon;
    public WeaponPickup GetNearbyWeaponPickup() => nearbyWeapon;
}
