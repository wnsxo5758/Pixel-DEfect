using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private StageData stageData;
    
    private int currentSkillNumber; //선택된 스킬 넘버
    
    private MovementRigidbody2D movement;
    private PlayerHp playerHp;
    private PlayerAttack playerAttack;
    private PlayerInteraction playerInteraction;
    private PlayerStateMachine<PlayerController> stateMachine;

    public bool IsOnLadder { get; set; } = false; //사다리 

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
        stateMachine.Setup(this, new PlayerStates.Idle());
        stateMachine.SetGlobalState(new PlayerStates.StateGlobal());
        
        InputManager.Instance.OnJumpPressed += OnJump;
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

        if (playerInteraction.IsConnected)
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
        if (movement.IsGrounded)
        {
            ChangeState(new PlayerStates.Jump());
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

    public void OnHold() // 홀드 입력
    {
        if (playerInteraction.CheckHold())
        {
            ChangeState(new PlayerStates.Hold());
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
            ChangeState(new PlayerStates.Idle());
        }
    }
    
    public void UpdateMove(float x) // 이동
    {
        movement.MoveTo(x);

        float xPos = Mathf.Clamp(transform.position.x, stageData.PlayerLimitMinX, stageData.PlayerLimitMaxX);
        transform.position = new Vector2(xPos, transform.position.y);
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
    
    /*
    public void UpdateAttack() // 공격
    {
        if (Input.GetKeyDown(meleeAttack))
        {
            playerAttack.MeleeAttack();
        }
        else if (Input.GetKeyDown(magicAttack))
        {
            playerAttack.MagicAttack(currentSkillNumber);
        }
    }
    

    public void HealPlayer()
    {
        if (Input.GetKeyDown(healKeyCode))
        {
            playerHp.IncreaseHp();
        }
    }
    
    public void UpdateSkillNumber()
    {
        if (Input.GetKeyDown(selectMagicBack)) // 이전 마법 설정
        {

        }
        else if (Input.GetKeyDown(selectMagicFront)) // 다음 마법 설정
        {

        }
    }
    */

    public void ChangeState(State<PlayerController> newState)
    {
        stateMachine.ChangeState(newState);
    }

    public void RevertToPreviousState()
    {
        stateMachine.RevertToPreviousState();
    }
    
    public void SpriteFlipX(float x)
    {
        if (x == 0) return;
        transform.localScale = new Vector3((x < 0 ? -1 : 1), 1, 1);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(1000, 50, 300, 20),
            "State: " + stateMachine.CurrentState.GetType().Name);
    }
    
    /* private void UpdateInteract(float x)
    {
        if (x != 0)
        {
            rayDirection = new Vector2(Mathf.Sign(x), 0f);
        }
        
        RaycastHit2D hitInfo = Physics2D.Raycast(rayPoint.position, rayDirection
            , rayDistance);

        if (hitInfo.collider != null && hitInfo.collider.gameObject.layer == layerIndex)
        {
            if (Input.GetKeyDown(interactKeyCode) && grabbedObject == null) // 그랩 가능 상태
            {
                grabbedObject = hitInfo.collider.gameObject;
                grabbedObject.GetComponent<Rigidbody2D>().isKinematic = true;
                grabbedObject.transform.position = grabPoint.position;
                grabbedObject.transform.SetParent(transform);
            }
            else if (Input.GetKeyDown(interactKeyCode)) // 내려 놓기
            {
                grabbedObject.GetComponent<Rigidbody2D>().isKinematic = false;
                grabbedObject.transform.SetParent(null);
                grabbedObject = null;
            }
            else if (Input.GetKeyDown(throwKeyCode)) // 던지기
            {
                Vector2 throwDir = rayDirection + new Vector2(0, 1f);
                grabbedObject.GetComponent<Rigidbody2D>().isKinematic = false;
                grabbedObject.transform.SetParent(null);
                grabbedObject.GetComponent<Rigidbody2D>().AddForce(throwDir * throwPower, ForceMode2D.Impulse);
                grabbedObject = null;
            }
        }
        
        Debug.DrawRay(rayPoint.position,  rayDirection * rayDistance);
    }
    */
}
