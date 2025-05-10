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
            // 지면 체크
            if (!player.GetComponent<MovementRigidbody2D>().IsGrounded)
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
        public override void Enter(PlayerController player)
        {
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();
            
            // 움직임이 멈추면 Idle 상태로 전환
            if (Mathf.Approximately(input, 0f))
            {
                player.ChangeState(new Idle());
            }
            
            // 지면 체크
            if (!player.GetComponent<MovementRigidbody2D>().IsGrounded)
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
        
        public override void Enter(PlayerController player)
        {
            movement = player.GetComponent<MovementRigidbody2D>();
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();
            
            player.UpdateMove(input);
            
            // 방향 설정
            if (input != 0)
            {
                player.SpriteFlipX(input);
            }
            
            // 착지 감지
            if (movement.IsGrounded && movement.Velocity.y <= 0.01f)
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
        
        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            
            animator.SetHoldAnim(player.transform.localScale.x);
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();
            
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
        
        public override void Enter(PlayerController player)
        {
            movement = player.GetComponent<MovementRigidbody2D>();
            animator = player.GetComponentInChildren<PlayerAnimator>();
            
            // 사다리 모드 설정
            animator.SetClimbAnim(true);
            movement.DisableGravity();
        }
        
        public override void Execute(PlayerController player)
        {
            float vertical = player.VerticalInput();
            
            //사다리 이동
            movement.Climb(vertical);
            
            //애니메이션
            animator.ClimbAnim(vertical);

            if (player.IsOnLadder)
            {
                if (movement.IsGrounded && vertical < 0f)
                    player.ChangeState(new Idle());
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

            if (movement.IsGrounded)
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

    public class Teleport : State<PlayerController>
    {
        private PlayerAnimator animator;
        private float teleportDuration = 0.5f;
        private float teleportTimer;
        
        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            
            teleportTimer = teleportDuration;

            // 이동 중지
            player.UpdateMove(0f);
            
            // 임시 애니메이션
            animator.MovementAnim(0f);
        }

        public override void Execute(PlayerController player)
        {
            teleportTimer -= Time.deltaTime;
            
            // 텔레포트 완료
            if (teleportTimer <= 0)
            {
                player.ChangeState(new Idle());
            }
        }

        public override void Exit(PlayerController player)
        {
        }
    }

    public class Valve : State<PlayerController>
    {
        private MovementRigidbody2D movement;
        private PlayerAnimator animator;
        
        public override void Enter(PlayerController player)
        {
            movement = player.GetComponent<MovementRigidbody2D>();
            animator = player.GetComponentInChildren<PlayerAnimator>();
            
            movement.MoveTo(0f);
        }

        public override void Execute(PlayerController player)
        {
            
        }

        public override void Exit(PlayerController player)
        {
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
