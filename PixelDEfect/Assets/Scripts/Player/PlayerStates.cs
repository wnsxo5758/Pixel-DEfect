using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerStates
{
    public class Idle : State<PlayerController>
    {
        private PlayerAnimator animator;
        
        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            
            animator?.MovementAnim(0f);
            
            player.UpdateMove(0);
        }

        public override void Execute(PlayerController player)
        {
            float horizontal = player.HorizontalInput();

            if (Mathf.Abs(horizontal) > 0.01f)
            {
                player.ChangeState(new Run());
                return;
            }
            
            // 지면 체크
            if (!player.IsGrounded())
            {
                player.ChangeState(new Jump());
            }
        }

        public override void Exit(PlayerController player)
        {
        }
    }

    public class Run : State<PlayerController>
    {
        private PlayerAnimator animator;
        
        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
        }

        public override void Execute(PlayerController player)
        {
            float horizontal = player.HorizontalInput();
            
            player.UpdateMove(horizontal);
            
            animator.MovementAnim(horizontal);

            if (horizontal != 0)
            {
                player.SpriteFlipX(horizontal);
            }

            if (Mathf.Abs(horizontal) < 0.01f)
            {
                player.ChangeState(new Idle());
                return;
            }
            
            // 지면 체크
            if (!player.IsGrounded())
            {
                player.ChangeState(new Jump());
            }
        }
        
        public override void Exit(PlayerController player)
        {
            
        }
    }

    public class Jump : State<PlayerController>
    {
        private MovementRigidbody2D movement;
        private PlayerAttack attack;
        
        public override void Enter(PlayerController player)
        {
            movement = player.GetComponent<MovementRigidbody2D>();
            attack = player.GetComponent<PlayerAttack>();
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();

            if (attack.AfterPulling && player.IsGrounded())
            {
                attack.AfterPulling = false;
            }

            if (!attack.AfterPulling)
            {
                player.UpdateMove(input);
            }
            
            // 방향 설정
            if (input != 0)
            {
                player.SpriteFlipX(input);
            }
            
            // 착지 감지
            if (player.IsGrounded() && movement.Velocity.y <= 0.01f)
            {
                player.ChangeState(new Idle());
            }
        }
        
        public override void Exit(PlayerController player)
        {
            
        }
    }
    
    public class Crawl : State<PlayerController>
    {
        private PlayerAnimator animator;
        private BoxCollider2D boxCollider;
        private Vector2 originalColliderSize;
        private Vector2 originalColliderOffset;
        private Vector2 crouchColliderSize;
        private float colliderSizeFactor = 0.65f;
        
        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            boxCollider = player.GetComponent<BoxCollider2D>();
            
            originalColliderSize = boxCollider.size;
            originalColliderOffset = boxCollider.offset;
            
            crouchColliderSize = new Vector2(originalColliderSize.x,originalColliderSize.y * colliderSizeFactor);
            boxCollider.size = crouchColliderSize;

            float offsetY = (originalColliderSize.y - crouchColliderSize.y) / 2;
            boxCollider.offset = new Vector2(boxCollider.offset.x, boxCollider.offset.y - offsetY);
            
            animator.SetCrouchAnim(true);
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();
            
            // 애니메이션 업데이트
            animator.CrawlAnim(input);
            
            // 이동 업데이트
            player.UpdateMove(input);
            
            // 방향 설정
            if (input != 0)
            {
                player.SpriteFlipX(input);
            }

            if (player.HasSpaceAbove() && player.WantToStand)
            {
                player.ChangeState(new Idle());
                player.WantToStand = false;
            }
        }

        public override void Exit(PlayerController player)
        {
            boxCollider.size = originalColliderSize;
            boxCollider.offset = originalColliderOffset;
            
            animator.SetCrouchAnim(false);
        }
    }

    public class Roll : State<PlayerController>
    {
        private MovementRigidbody2D movement;
        private PlayerAnimator animator;
        private float rollSpeed = 10f;
        private float rollDuration = 0.5f;
        private float rollTimer;
        private float rollDirection;
        private int playerLayer;
        private bool isInvulnerable = false;
        
        // 벽 감지 변수
        private bool isWallDetected = false; 
        private float wallCheckDistance = 0.5f; // 벽 감지 거리
        private float wallHitDelay = 0.15f; // 벽 충돌 후 지연 시간
        private float wallHitTimer = 0f; // 벽 충돌 후 경과시간
        private LayerMask wallLayer; // 벽 레이어
        
        
        public override void Enter(PlayerController player)
        {
            movement = player.GetComponent<MovementRigidbody2D>();
            animator = player.GetComponentInChildren<PlayerAnimator>();
            
            // 구르기 방향 설정 (현재 바라보는 방향)
            rollDirection = player.transform.localScale.x > 0 ? 1 : -1;
            rollTimer = rollDuration;
            
            wallLayer = LayerMask.GetMask("Ground", "Platform", "Object");
            
            // 구르기 애니메이션
            animator.StartRollAnim();
            
            // 구르기 코루틴
            player.StartCoroutine(player.StartRollCoroutine());
            
            isWallDetected = false;
            wallHitTimer = 0f;
            isInvulnerable = true;
        }

        public override void Execute(PlayerController player)
        {
            // 벽 감지 체크
            if (!isWallDetected)
            {
                isWallDetected = CheckForWall(player);

                if (isWallDetected)
                {
                    movement.Roll(rollDirection, rollSpeed * 0.5f);
                }
                else
                {
                    movement.Roll(rollDirection, rollSpeed);
                }
            }

            // 벽에 부딪힌 경우
            if (isWallDetected)
            {
                wallHitTimer += Time.deltaTime;

                if (wallHitTimer >= wallHitDelay)
                {
                    animator.StopRollAnim();
                    
                    player.ChangeState(new Idle());
                    return;
                }
                
                movement.Roll(rollDirection, rollSpeed * 0.2f);
            }
            
            // 구르기 타이머
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0)
            {
                player.ChangeState(new Idle());
            }
        }

        public override void Exit(PlayerController player)
        {
            // 구르기 종료
            player.UpdateMove(0);
            isInvulnerable = false;
        }

        private bool CheckForWall(PlayerController player)
        {
            Vector2 rayOrigin = player.transform.position + new Vector3(0f, 0.3f, 0f);
            Vector2 rayDirection = new Vector2(rollDirection, 0);
            
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, wallCheckDistance, wallLayer);
            
            Debug.DrawRay(rayOrigin, rayDirection* wallCheckDistance, hit ? Color.red : Color.green, 0.5f);

            return hit.collider != null;
        }

        public bool CheckDodgeAndTriggerTimeStop(PlayerController player)
        {
            if (isInvulnerable)
            {
                // 시간 정지 스킬이 있다면 발동
                if (TimeManager.Instance.HasTimeStopAbility())
                {
                    TimeManager.Instance.TriggerTimeStopOnDodge(player.transform.position);
                    return true;
                }
            }

            return false;
        }
    }
    
    public class Hold : State<PlayerController>
    {
        private PlayerAnimator animator;
        private MovementRigidbody2D movement;
        private PlayerHp playerHp;
        private PlayerInteraction interaction;
        
        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            movement = player.GetComponent<MovementRigidbody2D>();
            playerHp = player.GetComponent<PlayerHp>();
            interaction = player.GetComponent<PlayerInteraction>();
            
            animator.SetHoldAnim(player.transform.localScale.x);
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();

            if (playerHp != null && playerHp.IsHit)
            {
                return;
            }

            // 땅에 떨어질 시
            if (!player.IsGrounded())
            {
                interaction.StopHolding();
            }
            
            animator.PushAndPullAnim(input);
            
            // 이동 업데이트
            player.UpdateMove(input);
        }
        
        public override void Exit(PlayerController player)
        {
            //애니메이션 트랜지션 처리(임시)
            animator.SetHoldAnim(player.transform.localScale.x);
        }
    }

    public class Climb : State<PlayerController>
    {
        private MovementRigidbody2D movement;
        private PlayerAnimator animator;
        private PlayerHp playerHp;
        
        public override void Enter(PlayerController player)
        {
            movement = player.GetComponent<MovementRigidbody2D>();
            animator = player.GetComponentInChildren<PlayerAnimator>();
            playerHp = player.GetComponent<PlayerHp>();
            
            // 사다리 모드 설정
            animator.SetClimbAnim(true);
            movement.DisableGravity();
        }
        
        public override void Execute(PlayerController player)
        {
            float vertical = player.VerticalInput();
            
            //사다리 이동
            if (playerHp.IsHit)
            {
                movement.Climb(0f);
            }
            else
            {
                movement.Climb(vertical);
            }
            
            //애니메이션
            animator.ClimbAnim(vertical);

            if (player.IsOnLadder)
            {
                if (movement.IsGrounded && vertical < 0f)
                {
                    player.IsOnLadder = false;
                    player.ChangeState(new Idle());
                }
            }
        }

        public override void Exit(PlayerController player)
        {
            player.IsOnLadder = false;
            
            animator.SetClimbAnim(false);
            movement.EnableGravity();
        }
    }

    public class Attack : State<PlayerController>
    {
        MovementRigidbody2D movement;
        
        public override void Enter(PlayerController player)
        {
            movement = player.GetComponent<MovementRigidbody2D>();

            if (player.IsGrounded())
            {
                movement.MoveTo(0);
            }
        }

        public override void Execute(PlayerController player)
        {
            
        }

        public override void Exit(PlayerController player)
        {
            
        }
    }

    public class PullWeaponGround : State<PlayerController>
    {
        private PlayerAnimator animator;
        private MovementRigidbody2D movement;
        private WeaponPullContext pullContext;
        private bool animationFinished = false;

        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            movement = player.GetComponent<MovementRigidbody2D>();

            // 지면에서는 이동 완전 정지
            player.UpdateMove(0);
            
            animationFinished = false;
        }

        public override void Execute(PlayerController player)
        {
            if (animationFinished)
            {
                player.ChangeState(new Idle());
                animationFinished = false;
            }
        }

        public override void Exit(PlayerController player)
        {
            
        }

        public void OnAnimationFinished()
        {
            animationFinished = true;
        }
    }

    public class PullWeaponAir : State<PlayerController>
    {
        private PlayerAnimator animator;
        private MovementRigidbody2D movement;
        private WeaponPullContext pullContext;
        private bool animationFinished = false;

        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            movement = player.GetComponent<MovementRigidbody2D>();

            if (movement != null)
            {
                movement.DisableGravity();
                movement.DisableRigidbody();
            }
            
            animationFinished = false;
        }

        public override void Execute(PlayerController player)
        {
            if (animationFinished)
            {
                player.ChangeState(new AfterPull());
                animationFinished = false;
            }
        }

        public override void Exit(PlayerController player)
        {
            
        }

        public void OnAnimationFinished()
        {
            animationFinished = true;
        }
    }

    public class AfterPull : State<PlayerController>
    {
        private PlayerAnimator animator;
        private MovementRigidbody2D movement;
        private Rigidbody2D rb;
        private WeaponPullContext pullContext;
        
        private bool knockBackApplied = false;
        private bool hasLanded = false;
        private float minimumAirTime = 0.1f; // 최소 공중 시간
        private float airTimer = 0f;

        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            movement = player.GetComponent<MovementRigidbody2D>();
            rb = player.GetComponent<Rigidbody2D>();

            if (movement != null)
            {
                movement.EnableGravity();
            }
            
            // 넉백 힘 적용
            ApplyKnockBack(player);

            if (animator != null)
            {
                animator.StartAfterPullAnim();
            }

            knockBackApplied = true;
            hasLanded = false;
            airTimer = 0f;
        }

        public override void Execute(PlayerController player)
        {
            // 공중 시간 누적
            airTimer += Time.deltaTime;
            
            // 착지 체크 (최소 공중 시간 경과 후)
            if (airTimer >= minimumAirTime && player.IsGrounded() && !hasLanded && rb.velocity.y <= 0.1f)
            {
                hasLanded = true;
                
                // 착지 즉시 Idle 상태로 전환
                player.ChangeState(new Idle());
            }
        }

        public override void Exit(PlayerController player)
        {
            if (animator != null)
            {
                animator.StopAfterPullAnim(); // 루프 애니메이션 정지
            }
        }
        
        private void ApplyKnockBack(PlayerController player)
        {
            // 플레이어가 바라보는 방향의 반대로 넉백
            float currentDirection = player.transform.localScale.x;
            Vector2 knockBackDirection = new Vector2(-currentDirection, 0.5f).normalized;
            
            // 넉백 힘 적용 (WeaponPullContext에서 설정된 값 사용)
            Vector2 knockBackForce = new Vector2(8f, 3f); // 기본값, 필요시 context에서 가져오기
            
            if (rb != null)
            {
                rb.velocity = Vector2.zero; // 기존 속도 초기화
                rb.AddForce(new Vector2(knockBackDirection.x * knockBackForce.x, knockBackForce.y), ForceMode2D.Impulse);
            }
        }
    }
    
    public class TeleportStart : State<PlayerController>
    {
        private PlayerAnimator animator;
        private MovementRigidbody2D movement;
        private bool animationFinished;

        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            movement = player.GetComponent<MovementRigidbody2D>();

            if (movement != null)
            {
                movement.MoveTo(0);
            }
            
            // 텔레포트 시작 애니메이션
            if (animator != null)
            {
                animator.StartTeleportAnim();
            }
            
            animationFinished = false;
        }

        public override void Execute(PlayerController player)
        {
            // 애니메이션 완료 대기
            if (animationFinished)
            {
                PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
                if (playerAttack != null)
                {
                    
                    Debug.Log($"텔레포트 시작 애니메이션 완료, {player.IsGrounded()}");
                    playerAttack.ExecuteTeleportMovement();
                }
                animationFinished = false;
            }
        }

        public override void Exit(PlayerController player)
        {
            
        }

        // 텔레포트 시작 애니메이션 완료 시 호출
        public void OnTeleportStartAnimationFinished()
        {
            animationFinished = true;
        }
    }

    public class TeleportEnd : State<PlayerController>
    {
        private PlayerAnimator animator;
        private MovementRigidbody2D movement;
        private bool animationFinished = false;
        private bool shouldPullWeapon = false;
        private WeaponPullContext pendingPullContext;
        
        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            movement = player.GetComponent<MovementRigidbody2D>();

            if (movement != null)
            {
                movement.MoveTo(0);
            }
            
            // 텔레포트 후 무기 뽑기가 필요한지 확인
            PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
            if (playerAttack != null)
            {
                shouldPullWeapon = playerAttack.HasPendingWeaponPull();
                if (shouldPullWeapon)
                {
                    pendingPullContext = playerAttack.GetPendingPullContext();
                }
            }

            if (shouldPullWeapon && pendingPullContext != null)
            {
                if (animator != null)
                {
                    animator.FinishTeleportAnim(); // ← 텔레포트 애니메이션 상태 정리
                }
                // 무기 뽑기가 필요하면 텔레포트 종료 애니메이션 건너뛰고 바로 무기 뽑기 상태로 전환
                DetermineAndStartPullState(player, pendingPullContext);
            }
            else
            {
                // 일반적인 텔레포트 완료 - 텔레포트 종료 애니메이션 재생
                if (animator != null)
                {
                    animator.EndTeleportAnim();
                }
                animationFinished = false;
            }
        }

        public override void Execute(PlayerController player)
        {
            // 무기 뽑기가 필요한 경우에는 이미 Enter에서 상태 전환했으므로 여기서는 처리하지 않음
            if (shouldPullWeapon)
            {
                return;
            }
            
            // 일반적인 텔레포트 완료 처리
            if (animationFinished)
            {
                player.ChangeState(new Idle());
                animationFinished = false;
            }
        }

        public override void Exit(PlayerController player)
        {
            
        }

        private void DetermineAndStartPullState(PlayerController player, WeaponPullContext context)
        {
            // 현재 플레이어가 공중에 있는지 확인
            bool isPlayerInAir = !player.IsGrounded();
            
            context.isGrounded = !isPlayerInAir;
            
            if (isPlayerInAir)
            {
                // 공중 뽑기 상태로 전환
                player.ChangeState(new PullWeaponAir());
                
                if (animator != null)
                {
                    animator.StartPullAirAnim(context);
                }
            }
            else
            {
                // 지면 뽑기 상태로 전환
                player.ChangeState(new PullWeaponGround());
                
                if (animator != null)
                {
                    animator.StartPullGroundAnim(context);
                }
            }
        }
        
        // 텔레포트 종료 애니메이션 완료 시 호출
        public void OnTeleportEndAnimationFinished()
        {
            animationFinished = true;
        }
    }
    
    public class Valve : State<PlayerController>
    {
        private PlayerInteraction interaction;
        private MovementRigidbody2D movement;
        private PlayerAnimator animator;
        
        public override void Enter(PlayerController player)
        {
            interaction = player.GetComponent<PlayerInteraction>();
            movement = player.GetComponent<MovementRigidbody2D>();
            animator = player.GetComponentInChildren<PlayerAnimator>();
            
            movement.MoveTo(0f);

            animator.SetValveAnim(true);
        }

        public override void Execute(PlayerController player)
        {
            if (!interaction.IsNearValve())
            {
                player.ChangeState(new Idle());
            }
        }

        public override void Exit(PlayerController player)
        {
            animator.SetValveAnim(false);
        }
    }

    public class VendingMachineHeal : State<PlayerController>
    {
        private PlayerAnimator animator;
        private MovementRigidbody2D movement;
        private PlayerHp playerHp;
        private VendingMachineHealContext context;

        private float healTimer = 0f;
        private float nextHealTime = 0f;
        private bool isHealingComplete = false;

        public VendingMachineHeal(VendingMachineHealContext healContext)
        {
            context = healContext;
            context.CalculateHealTime();
        }

        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            movement = player.GetComponent<MovementRigidbody2D>();
            playerHp = player.GetComponent<PlayerHp>();

            if (movement != null)
            {
                movement.MoveTo(0);
            }
            
            // 회복 애니메이션 시작
            if (animator != null)
            {
                animator.SetHealingAnim(true);
            }

            healTimer = 0f;
            nextHealTime = 1f;
            isHealingComplete = false;
            context.elapsedTime = 0f;
            context.healedAmount = 0;
        }

        public override void Execute(PlayerController player)
        {
            healTimer += Time.deltaTime;
            context.elapsedTime += Time.deltaTime;
            
            // 회복 처리
            if (healTimer >= nextHealTime && !isHealingComplete)
            {
                PerformHeal();
                healTimer = 0f;
            }
            
            // 회복 완료 확인
            if (context.IsHealComplete() || isHealingComplete)
            {
                CompleteHealing(player);
            }
        }

        public override void Exit(PlayerController player)
        {
            if (animator != null)
            {
                animator.SetHealingAnim(false);
            }
        }

        private void PerformHeal()
        {
            if (playerHp == null) return;

            int currentHp = playerHp.GetCurrentHp();
            int maxHp = playerHp.GetMaxHp();

            if (currentHp > maxHp)
            {
                isHealingComplete = true;
                return;
            }
            
            // 회복할 양 계산
            int remainingHeal = context.healAmount - context.healedAmount;
            int maxPossibleHeal = maxHp - currentHp;
            int healAmount = Mathf.Min(remainingHeal, maxPossibleHeal, (int)context.healRate);

            if (healAmount > 0)
            {
                // 체력 회복
                playerHp.IncreaseHp(healAmount);
                context.healedAmount += healAmount;
            }
            
            // 회복 완료 확인
            if (context.healedAmount >= context.healAmount || playerHp.GetCurrentHp() >= maxHp)
            {
                isHealingComplete = true;
            }
        }

        private void CompleteHealing(PlayerController player)
        {
            isHealingComplete = true;
            
            player.ChangeState(new Idle());
        }
    }
    
    public class Hit : State<PlayerController>
    {
        private PlayerAnimator animator;
        private PlayerHp playerHp;
        
        private float hitStunDuration;
        private float stunTimer;
        
        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            playerHp = player.GetComponent<PlayerHp>();
            
            hitStunDuration = playerHp.GetCurrentHitStunDuration();
            stunTimer = hitStunDuration;
            

            if (animator != null)
            {
                animator.TriggerHitAnim();
            }
        }

        public override void Execute(PlayerController player)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0)
            {
                player.ChangeState(new Idle());
            }
        }

        public override void Exit(PlayerController player)
        {
            playerHp.IsHit = false;
        }
    }
    
    public class StateGlobal : State<PlayerController>
    {
        
        public override void Enter(PlayerController player)
        {
            
        }

        public override void Execute(PlayerController player)
        {
            player.UpdateBelowCollision();
        }

        public override void Exit(PlayerController player)
        {
            
        }
    }
}
