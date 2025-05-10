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
    
    // 현재 감지된 상호작용 오브젝트들
    private List<GameObject> detectedInteractables = new List<GameObject>();
    private ButtonBase currentButton;
    private ValveButton currentValve;
    private DoorBase currentDoor;
    private HoldObject currentHoldObject;
    private WeaponPickup nearbyWeapon;
    private ThrownWeapon nearbyThrownWeapon;
    private FixedJoint2D holdJoint;
    private bool isHolding = false;
    private float groundCheckTimer = 0f;
    
    // 상호작용 타입 열거형
    public enum InteractionType
    {
        None,
        WeaponPickup,
        ThrownWeapon,
        Button,
        Valve,
        Door,
        Holdable
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
    }
    
    private void Update()
    {
        // 주변 상호작용 가능 오브젝트 감지
        DetectedInteractables();
        
        CheckHoldObjectGrounded();
    }

    // 상호작용 오브젝트 감지
    private void DetectedInteractables()
    {
        // 이전 감지 목록 초기화
        detectedInteractables.Clear();
        currentInteractionType = InteractionType.None;
        currentButton = null;
        currentValve = null;
        currentDoor = null;
        
        if (!isHolding)
        {
            currentHoldObject = null;
        }
        
        nearbyWeapon = null;
        nearbyThrownWeapon = null;
        
        // 주변 오브젝트 감지
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            interactionPoint.position, interactionRadius, interactableLayer);

        if (colliders.Length > 0)
        {
            // 각 콜라이더를 확인하고 우선순위에 따라 처리
            foreach (Collider2D collider in colliders)
            {
                detectedInteractables.Add(collider.gameObject);
                
                // 우선순위에 따라 상호작용 타입 설정
                DetermineInteractionType(collider);
            }
            
            // 상호작용 프롬프트 표시
            if (interactionPrompt != null && currentInteractionType != InteractionType.None)
            {
                interactionPrompt.SetActive(true);
                
                // 프롬프트 위치 설정
                GameObject targetObject = null;
                switch (currentInteractionType)
                {
                    case InteractionType.WeaponPickup:
                        targetObject = nearbyWeapon?.gameObject;
                        break;
                    case InteractionType.ThrownWeapon:
                        targetObject = nearbyThrownWeapon?.gameObject;
                        break;
                    case InteractionType.Button:
                        targetObject = currentButton?.gameObject;
                        break;
                    case InteractionType.Valve:
                        targetObject = currentValve?.gameObject;
                        break;
                    case InteractionType.Door:
                        targetObject = currentDoor?.gameObject;
                        break;
                    case InteractionType.Holdable:
                        targetObject = currentHoldObject?.gameObject;
                        break;
                }

                if (targetObject != null)
                {
                    interactionPrompt.transform.position = targetObject.transform.position + Vector3.up * 0.5f;
                }
            }
        }
        else
        {
            // 감지된 오브젝트가 없으면 프롬프트 숨기기
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
        
        // PlayerController에 상호작용 가능 여부 알림
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetInteractionAvailable(currentInteractionType != InteractionType.None, currentInteractionType);
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
    
    // 상호작용 타입 결정 (우선순위 설정)
    private void DetermineInteractionType(Collider2D collider)
    {
        // 1순위: 무기 줍기
        WeaponPickup weaponPickup = collider.GetComponent<WeaponPickup>();
        if (weaponPickup != null)
        {
            currentInteractionType = InteractionType.WeaponPickup;
            nearbyWeapon = weaponPickup;
            return;
        }
        if (currentInteractionType == InteractionType.WeaponPickup) return;

        // 2순위: 던져진 무기 픽업
        ThrownWeapon thrownWeapon = collider.GetComponent<ThrownWeapon>();
        if (thrownWeapon != null && thrownWeapon.IsStuck())
        {
            currentInteractionType = InteractionType.ThrownWeapon;
            nearbyThrownWeapon = thrownWeapon;
            return;
        }
        if (currentInteractionType == InteractionType.ThrownWeapon) return;

        
        // 3순위: 버튼 상호작용
        ButtonBase button = collider.GetComponent<ButtonBase>();
        if (button != null)
        {
            currentInteractionType = InteractionType.Button;
            currentButton = button;
            return;
        }
        if (currentInteractionType == InteractionType.Button) return;


        // 4순위: 밸브 상호작용
        ValveButton valve = collider.GetComponent<ValveButton>();
        if (valve != null)
        {
            currentInteractionType = InteractionType.Valve;
            currentValve = valve;
            return;
        }
        if (currentInteractionType == InteractionType.Valve) return;

        // 5순위: 문
        DoorBase door = collider.GetComponent<DoorBase>();
        if (door != null)
        {
            currentInteractionType = InteractionType.Door;
            currentDoor = door;
            return;
        }
        if (currentInteractionType == InteractionType.Door) return;
        
        // 6순위: 홀드 오브젝트
        if (!isHolding)
        {
            HoldObject holdObject = collider.GetComponent<HoldObject>();
            if (holdObject != null && holdObject.IsGrounded)
            {
                currentInteractionType = InteractionType.Holdable;
                currentHoldObject = holdObject;
            }
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
                          currentInteractionType == InteractionType.Valve))
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
            
            case InteractionType.Button:
                // 버튼 누르기
                if (currentButton != null)
                {
                    currentButton.ButtonTrigger();
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
            
            case InteractionType.Door:
                if (currentDoor != null)
                {
                    currentDoor.ActiveDoor(gameObject);
                }
                break;
            
            case InteractionType.Holdable:
                // 물체 잡기 시작
                if (!isHolding && currentHoldObject != null && currentHoldObject.IsGrounded)
                {
                    StartHolding();
                }
                break;
        }
    }
    
    // 상호작용 종료 처리
    private void HandleInteractionEnd()
    {
        // 현재 밸브를 돌리고 있었다면
        if (currentValve != null)
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
    
    // 물체 잡기 시작
    private void StartHolding()
    {
        if (currentHoldObject == null || !currentHoldObject.IsGrounded) return;
        
        // 플레이어와 물체 사이의 거리 확인
        float distance = Vector2.Distance(transform.position, currentHoldObject.transform.position);
        
        // 너무 멀면 잡기 불가
        if (distance > holdDistance)
        {
            return;
        }
        
        // 플레이어가 물체를 향해 방향 전환
        FaceTowardsObject(currentHoldObject.transform.position);
        
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
            holdJoint.connectedBody = targetRb;
            holdJoint.enabled = true;
            
            // 물체 회전 고정
            targetRb.freezeRotation = true;
        }
        
        // 상태 전환
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ChangeState(new PlayerStates.Hold());
        }
    }
    
    // 플레이어가 물체를 향해 방향 전환
    private void FaceTowardsObject(Vector3 objectPosition)
    {
        // 물체가 플레이어 기준 어느 방향에 있는지 계산
        float directionToObject = objectPosition.x - transform.position.x;
        
        // 플레이어 방향 설정
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(directionToObject);
        transform.localScale = newScale;
    }
    
    // 물체 잡기 종료
    private void StopHolding()
    {
        if (!isHolding) return;

        isHolding = false;
        groundCheckTimer = 0f;

        if (holdJoint != null)
        {
            Destroy(holdJoint);
            holdJoint = null;
        }
        
        currentHoldObject = null;
        
        // 상태 전환
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null && controller.GetCurrentState() is PlayerStates.Hold)
        {
            controller.ChangeState(new PlayerStates.Idle());
        }
    }

    // 현재 잡고 있는 상태인지 확인
    public bool IsHolding()
    {
        return isHolding;
    }

    // 밸브 감지 여부 확인
    public bool IsNearValve()
    {
        return currentInteractionType == InteractionType.Valve && currentValve != null;
    }
    
    // 감지된 무기 정보 반환
    public bool HasNearbyWeapon()
    {
        return currentInteractionType == InteractionType.WeaponPickup ||
               currentInteractionType == InteractionType.ThrownWeapon;
    }

    // 던져진 무기 정보 반환
    public ThrownWeapon GetNearbyThrownWeapon()
    {
        return nearbyThrownWeapon;
    }

    // 일반 무기 정보 반환
    public WeaponPickup GetNearbyWeaponPickup()
    {
        return nearbyWeapon;
    }
    
    // 디버그 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint != null ?
            interactionPoint.position : transform.position, interactionRadius);
    }
}
