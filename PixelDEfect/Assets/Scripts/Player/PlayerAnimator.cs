using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    // 파라미터 상수
    private readonly int velocityX = Animator.StringToHash("VelocityX");
    private readonly int velocityY = Animator.StringToHash("VelocityY");
    private readonly int jump = Animator.StringToHash("Jump");
    private readonly int isGrounded = Animator.StringToHash("IsGrounded");
    private readonly int isCrouching = Animator.StringToHash("IsCrouching");
    private readonly int roll = Animator.StringToHash("Roll");
    private readonly int stopRoll = Animator.StringToHash("StopRoll");
    private readonly int isClimbing = Animator.StringToHash("IsClimbing");
    private readonly int isConnected = Animator.StringToHash("IsConnected");
    private readonly int turnValve = Animator.StringToHash("TurnValve");
    private readonly int hasWeapon = Animator.StringToHash("HasWeapon");
    private readonly int attack = Animator.StringToHash("Attack");
    private readonly int throwWeapon = Animator.StringToHash("Throw");
    private readonly int hit = Animator.StringToHash("Hit");
    private readonly int revive = Animator.StringToHash("Revive");
    
    // 무기 뽑기 애니메이션
    private readonly int pullGround = Animator.StringToHash("PullGround");
    private readonly int pullAir = Animator.StringToHash("PullAir");
    private readonly int afterPull = Animator.StringToHash("AfterPull");
    
    // 텔레포트 애니메이션
    private readonly int teleportPre = Animator.StringToHash("TeleportPre");
    private readonly int teleportPost = Animator.StringToHash("TeleportPost");
    
    // 사망 원인
    private readonly int death = Animator.StringToHash("Death");
    private readonly int deathMelee = Animator.StringToHash("DeathMelee");
    private readonly int deathRanged = Animator.StringToHash("DeathRanged");
    private readonly int deathPress = Animator.StringToHash("DeathPress");
    private readonly int deathLaser = Animator.StringToHash("DeathLaser");
    private readonly int deathDrown = Animator.StringToHash("DeathDrown");

    private Animator animator; // 애니메이션 
    private PlayerController controller;
    private MovementRigidbody2D movement; // 움직임
    private PlayerAttack playerAttack; // 플레이어 공격
    private PlayerInteraction playerInteraction; // 상호작용
    private PlayerHp playerHp; // HP

    private float pnpDirection;
    
    // 상태 추적
    private bool isPlayingClimbingAnimation = false;
    private bool isPlayingPullAnimation = false;
    private bool isPlayingTeleportAnimation = false;

    private DeathData currentDeathData;
    private WeaponPullContext currentPullContext;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponentInParent<PlayerController>();
        movement = GetComponentInParent<MovementRigidbody2D>();
        playerAttack = GetComponentInParent<PlayerAttack>();
        playerInteraction = GetComponentInParent<PlayerInteraction>();
    }

    private void LateUpdate()
    {
        if (movement != null)
        {
            // 수직 속도 및 지면 상태 업데이트
            float verticalVelocity = movement.Velocity.y;
            bool currentlyGrounded = movement.IsGrounded;
            
            // 사다리 상태가 아닐 때만 업데이트
            if (!isPlayingClimbingAnimation)
            {
                animator.SetFloat(velocityY, verticalVelocity);
                
                // 지면 상태 설정
                animator.SetBool(isGrounded, currentlyGrounded);
            }
        }
    }
    
    // 이동 애니메이션 설정
    public void MovementAnim(float x)
    {
        animator.SetFloat(velocityX, Mathf.Abs(x));
    }

    public void JumpAnim()
    {
        animator.SetTrigger(jump);
    }

    public void LadderJumpAnim()
    {
        SetClimbAnim(false);
        
        animator.SetTrigger(jump);
    }
    
    public void SetClimbAnim(bool isOnLadder)
    {
        animator.SetBool(isClimbing, isOnLadder);
        isPlayingClimbingAnimation = isOnLadder;
        
        // 사다리를 타고 있을 때는 파라미터 초기화
        if (isOnLadder)
        {
            animator.SetBool(isGrounded, false);
        }
    }
    
    public void ClimbAnim(float y)
    {
        if (isPlayingClimbingAnimation)
        {
            animator.SetFloat(velocityY, Mathf.Abs(y));
        }
    }
    
    public void SetCrouchAnim(bool crouching)
    {
        animator.SetBool(isCrouching, crouching);
    }
    
    public void CrawlAnim(float x)
    {
        animator.SetFloat(velocityX, Mathf.Abs(x));
    }

    public void StartRollAnim()
    {
        animator.SetTrigger(roll);
        animator.ResetTrigger(stopRoll);
    }

    public void StopRollAnim()
    {
        animator.SetTrigger(stopRoll);
        animator.ResetTrigger(roll);
    }
    
    public void SetHoldAnim(float dir)
    {
        animator.SetBool(isConnected, playerInteraction.IsHolding());
        pnpDirection = dir;
    }
    
    public void PushAndPullAnim(float x)
    {
        animator.SetFloat(velocityX, pnpDirection * x);
    }

    public void SetValveAnim(bool valveState)
    {
        animator.SetBool(turnValve, valveState);
    }

    public void SetHasWeapon(bool weapon)
    {
        animator.SetBool(hasWeapon, weapon);
    }

    public void TriggerAttackAnim()
    {
        animator.SetTrigger(attack);
    }

    public void TriggerThrowAnim()
    {
        animator.SetTrigger(throwWeapon);
    }

    public void StartPullAnim(WeaponPullContext context)
    {
        if (isPlayingPullAnimation) return;
        
        currentPullContext = context;
        isPlayingPullAnimation = true;
        
        ResetPullAnimationTriggers();

        if (context.isTeleportPull)
        {
            if (context.isPlayerAirborne)
            {
                animator.SetTrigger(pullAir);
            }
            else
            {
                animator.SetTrigger(pullGround);
            }
        }
        else if (context.isGrounded)
        {
            animator.SetTrigger(pullGround);
        }
        else
        {
            animator.SetTrigger(pullAir);
        }
    }

    private void FinishedPullAnim()
    {
        if (currentPullContext == null) return;
        
        if (currentPullContext.isPlayerAirborne)
        {
            ApplyAirPullKnockBack();
            animator.SetTrigger(afterPull);
        }
        
        playerAttack?.FinishedPullAnim(currentPullContext);
        
        isPlayingPullAnimation = false;
        currentPullContext = null;
    }

    private void ApplyAirPullKnockBack()
    {
        if (movement == null || currentPullContext == null) return;
        
        // 뒤로 날아가는 힘 계산
        float knockBackDirection = -Mathf.Sign(controller.transform.localScale.x);
        Vector2 knockBackForce = new Vector2(knockBackDirection* currentPullContext.knockBackForce.x,
            currentPullContext.knockBackForce.y);
        
        // 물리 효과 적용
        movement.SetVelocity(knockBackForce);
    }

    public void StartTeleportAnim()
    {
        if (isPlayingTeleportAnimation) return;
        
        isPlayingTeleportAnimation = true;
        ResetTeleportAnimationTriggers();
        
        animator.SetTrigger(teleportPre);
    }

    public void EndTeleportAnim(bool willPullWeapon)
    {
        if (!isPlayingTeleportAnimation) return;
        
        if (willPullWeapon)
        {
            isPlayingTeleportAnimation = false;
        }
        else
        {
            animator.SetTrigger(teleportPost);
        }
    }

    // 텔레포트 시작 애니메이션 완료
    private void OnTeleportStartAnim()
    {
        playerAttack?.OnTeleportStartAnim();
    }

    // 텔레포트 종료 애니메이션 완료
    private void OnTeleportEndAnim()
    {
        isPlayingTeleportAnimation = false;
        playerAttack?.OnTeleportEndAnim();
    }
    
    public void TriggerHitAnim()
    {
        animator.SetTrigger(hit);
    }
    
    public void TriggerDeathAnim(DeathData deathData)
    {
        currentDeathData = deathData;
        
        TriggerSpecificDeathAnim();
    }

    private void TriggerSpecificDeathAnim()
    {
        ResetAllDeathTrigger();

        switch (currentDeathData.cause)
        {
            case DeathCause.MeleeAttack:
                controller.SpriteFlipX(-currentDeathData.direction);
                animator.SetTrigger(deathMelee);
                break;
            
            case DeathCause.RangedAttack:
                animator.SetTrigger(deathRanged);
                break;
            
            case DeathCause.Press:
                animator.SetTrigger(deathPress);
                break;
            
            case DeathCause.Laser:
                animator.SetTrigger(deathLaser);
                break;
            
            case DeathCause.Drowning:
                animator.SetTrigger(deathDrown);
                break;
            
            case DeathCause.Fall:
            case DeathCause.Environmental:
            default:
                animator.SetTrigger(death);
                break;
        }
    }

    // 공격 타이밍 이벤트
    private void HandleAttackEvent()
    {
        playerAttack.HandleAttackCollision();
    }

    private void FinishedMeleeAttackEvent()
    {
        playerAttack.FinishedMeleeAttackAnim();
    }

    private void FinishedThrowWeaponEvent()
    {
        playerAttack.FinishedThrowAnim();
    }

    public void ResetAllAnimationStates()
    {
        animator.SetBool(isGrounded, true);
        animator.SetBool(isCrouching, false);
        animator.SetBool(isClimbing, false);
        animator.SetBool(isConnected, false);
        animator.SetBool(hasWeapon, playerAttack.HasWeapon());
        
        animator.ResetTrigger(jump);
        animator.ResetTrigger(roll);
        animator.ResetTrigger(stopRoll);
        animator.ResetTrigger(attack);
        animator.ResetTrigger(throwWeapon);
        animator.ResetTrigger(hit);
        animator.ResetTrigger(death);
        
        ResetPullAnimationTriggers();
        ResetTeleportAnimationTriggers();
        
        animator.SetTrigger(revive);
    }

    private void ResetAllDeathTrigger()
    {
        animator.ResetTrigger(death);
        animator.ResetTrigger(deathMelee);
        animator.ResetTrigger(deathRanged);
        animator.ResetTrigger(deathPress);
        animator.ResetTrigger(deathLaser);
        animator.ResetTrigger(deathDrown);
    }

    private void ResetPullAnimationTriggers()
    {
        animator.ResetTrigger(pullGround);
        animator.ResetTrigger(pullAir);
        animator.ResetTrigger(afterPull);
    }

    private void ResetTeleportAnimationTriggers()
    {
        animator.ResetTrigger(teleportPre);
        animator.ResetTrigger(teleportPost);
    }
    
    private void RestartGameEvent()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.PlayerDied();
        }
    }
}
