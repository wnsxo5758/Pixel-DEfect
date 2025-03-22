using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private StageData stageData;
    [Header("조작키")]
    [SerializeField]
    private KeyCode jumpKeyCode = KeyCode.Space;
    [SerializeField]
    private KeyCode healKeyCode = KeyCode.E;
    [SerializeField]
    private KeyCode meleeAttack = KeyCode.Mouse0; // 근접 공격
    [SerializeField]
    private KeyCode magicAttack = KeyCode.Mouse1; // 원거리 공격
    [SerializeField]
    private KeyCode selectMagicBack = KeyCode.Q; // 뒷칸의 마법 선택
    [SerializeField]
    private KeyCode selectMagicFront = KeyCode.E; // 앞칸의 마법 선택
    
    private int currentSkillNumber; //선택된 스킬 넘버
    
    private MovementRigidbody2D movement;
    private PlayerAnimator playerAnimator;
    private PlayerHp playerHp;
    private PlayerAttack playerAttack;
    private PlayerInteraction playerInteraction;

    private void Awake()
    {
        movement = GetComponent<MovementRigidbody2D>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHp = GetComponent<PlayerHp>();
        playerInteraction = GetComponent<PlayerInteraction>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float offset = 0.5f + Input.GetAxisRaw("Sprint") * 0.5f;

        x *= offset;
        
        UpdateMove(x);
        UpdateSkillNumber();
        HealPlayer();
        
        if (playerInteraction.IsConnected == false) //밀고 당기기 상태인지 확인
        {
            playerAnimator.UpdateAnimation(x);
            UpdateJump();
            UpdateAttack();
        }
    }
    
    private void UpdateMove(float x)
    {
        movement.MoveTo(x);

        float xPos = Mathf.Clamp(transform.position.x, stageData.PlayerLimitMinX, stageData.PlayerLimitMaxX);
        transform.position = new Vector2(xPos, transform.position.y);
    }

    private void UpdateJump()
    {
        if (Input.GetKeyDown(jumpKeyCode))
        {
            movement.Jump();
        }
        if (Input.GetKey(jumpKeyCode))
        {
            movement.IsLongJump = true;
        }
        else if (Input.GetKeyUp(jumpKeyCode))
        {
            movement.IsLongJump = false;
        }

    }
    
    private void UpdateAttack() // 공격
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

    private void HealPlayer()
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
