using UnityEngine;

namespace PlayerStates
{
    public class Idle : State<PlayerController>
    {
        private PlayerAnimator animator;
        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();
            
            animator.MovementAnim(input);
            
            if (input != 0f)
            {
                player.ChangeState(new Run());
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
            float input = player.HorizontalInput();
            
            animator.MovementAnim(input);
            
            if (input == 0f)
            {
                player.ChangeState(new Idle());
            }
            
            player.UpdateMove(input);
            player.SpriteFlipX(input);
        }
        
        public override void Exit(PlayerController player)
        {
            
        }
    }

    public class Jump : State<PlayerController>
    {
        private MovementRigidbody2D movement;
        private PlayerAnimator animator;
        
        public override void Enter(PlayerController player)
        {
            movement = player.GetComponent<MovementRigidbody2D>();
            animator = player.GetComponentInChildren<PlayerAnimator>();
            
            InputManager.Instance.OnCrouchPressed -= player.OnCrouch;
            InputManager.Instance.OnHoldPressed -= player.OnHold;
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();
            
            animator.MovementAnim(input);
            
            if (movement.IsGrounded && movement.Velocity.y <= 0.01f)
            {
                player.ChangeState(new Idle());
            }
            
            player.UpdateMove(input);
            player.SpriteFlipX(input);
        }
        
        public override void Exit(PlayerController player)
        {
            InputManager.Instance.OnCrouchPressed += player.OnCrouch;
            InputManager.Instance.OnHoldPressed += player.OnHold;
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
            
            animator.SetCrouchAnim(player.IsCrouching);
            
            originalColliderSize = boxCollider.size;
            originalColliderOffset = boxCollider.offset;
            
            crouchColliderSize = new Vector2(originalColliderSize.x,originalColliderSize.y * colliderSizeFactor);
            boxCollider.size = crouchColliderSize;

            float offsetY = (originalColliderSize.y - crouchColliderSize.y) / 2;
            boxCollider.offset = new Vector2(boxCollider.offset.x, boxCollider.offset.y - offsetY);
            
            InputManager.Instance.OnCrouchReleased += player.UnCrouch;
            InputManager.Instance.OnHoldPressed -= player.OnHold;
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();
            
            animator.CrawlAnim(input);
            
            player.UpdateMove(input);
            player.SpriteFlipX(input);

            // 키 입력 X 상태에서 일어서기 가능할 때
            if (!InputManager.Instance.IsCrouchKeyPressed() && player.HasSpaceAbove())
            {
                player.UnCrouch();
            }
        }

        public override void Exit(PlayerController player)
        {
            boxCollider.size = originalColliderSize;
            boxCollider.offset = originalColliderOffset;
            
            player.IsCrouching = false;
            animator.SetCrouchAnim(player.IsCrouching);
            
            InputManager.Instance.OnCrouchReleased -= player.UnCrouch;
            InputManager.Instance.OnHoldPressed += player.OnHold;
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
        
        // 회피 관련 레이어
        private int enemyLayer;
        private int enemyProjectileLayer;
        private int obstacleLayer; // 밤해물 레이어
        
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
            
            // 무적 처리 (레이어)
            playerLayer = player.gameObject.layer;
            enemyProjectileLayer = LayerMask.NameToLayer("EnemyBullet");
            wallLayer = LayerMask.GetMask("Ground", "Platform", "Object");
            
            // 충돌 레이어 비활성화
            if (enemyProjectileLayer != -1)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, enemyProjectileLayer, true);
            }
            
            InputManager.Instance.OnJumpPressed -= player.OnJump;
            InputManager.Instance.OnCrouchPressed -= player.OnCrouch;
            InputManager.Instance.OnHoldPressed -= player.OnHold;
            
            // 구르기 애니메이션
            animator.StartRollAnim();
            player.IsRolling = true;
            
            // 구르기
            player.StartCoroutine(player.StartRollCoroutine());
            
            isWallDetected = false;
            wallHitTimer = 0f;
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
            // 충돌 복원
            if (enemyProjectileLayer != -1)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, enemyProjectileLayer, false);
            }
            
            InputManager.Instance.OnJumpPressed += player.OnJump;
            InputManager.Instance.OnCrouchPressed += player.OnCrouch;
            InputManager.Instance.OnHoldPressed += player.OnHold;
            
            // 구르기 종료
            player.IsRolling = false;
            player.UpdateMove(0);
        }

        private bool CheckForWall(PlayerController player)
        {
            Vector2 rayOrigin = player.transform.position + new Vector3(0f, 0.3f, 0f);
            Vector2 rayDirection = new Vector2(rollDirection, 0);
            
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, wallCheckDistance, wallLayer);
            
            Debug.DrawRay(rayOrigin, rayDirection* wallCheckDistance, hit ? Color.red : Color.green, 0.5f);

            return hit.collider != null;
        }
    }
    
    public class Hold : State<PlayerController>
    {
        private PlayerAnimator animator;
        private PlayerInteraction playerInteraction;
        
        public override void Enter(PlayerController player)
        {
            animator = player.GetComponentInChildren<PlayerAnimator>();
            playerInteraction = player.GetComponent<PlayerInteraction>();
            animator.EnterHoldAnim(player.transform.localScale.x);
            playerInteraction.ConnectObject();
            
            // 입력 비활성화
            InputManager.Instance.OnJumpPressed -= player.OnJump;
            InputManager.Instance.OnCrouchPressed -= player.OnCrouch;
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();
            
            animator.PushAndPullAnim(input);
            
            player.UpdateMove(input);
        }
        
        public override void Exit(PlayerController player)
        {
            playerInteraction.DisconnectObject();
            
            //애니메이션 트랜지션 처리(임시)
            animator.PushAndPullAnim(player.transform.localScale.x);
            
            // 점프 활성화
            InputManager.Instance.OnJumpPressed += player.OnJump;
            InputManager.Instance.OnCrouchPressed += player.OnCrouch;
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
            
            // 입력 관리
            InputManager.Instance.OnLadderJumpPressed += player.OnLadderJump;
            InputManager.Instance.OnJumpPressed -= player.OnJump;
            InputManager.Instance.OnCrouchPressed -= player.OnCrouch;
            InputManager.Instance.OnHoldPressed -= player.OnHold;
            
            animator.SetClimbAnim(player.IsOnLadder);
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
            InputManager.Instance.OnLadderJumpPressed -= player.OnLadderJump;
            InputManager.Instance.OnJumpPressed += player.OnJump;
            InputManager.Instance.OnCrouchPressed += player.OnCrouch;
            InputManager.Instance.OnHoldPressed += player.OnHold;
            
            player.IsOnLadder = false;
            
            animator.SetClimbAnim(player.IsOnLadder);
            movement.EnableGravity();
        }
    }

    public class Attack : State<PlayerController>
    {
        MovementRigidbody2D movement;
        
        public override void Enter(PlayerController player)
        {
            movement = player.GetComponent<MovementRigidbody2D>();
            
            InputManager.Instance.OnJumpPressed -= player.OnJump;
            InputManager.Instance.OnCrouchPressed -= player.OnCrouch;
            InputManager.Instance.OnHoldPressed -= player.OnHold;
        }

        public override void Execute(PlayerController player)
        {
            if (movement.IsGrounded)
            {
                movement.MoveTo(0);
            }
        }

        public override void Exit(PlayerController player)
        {
            InputManager.Instance.OnJumpPressed += player.OnJump;
            InputManager.Instance.OnCrouchPressed += player.OnCrouch;
            InputManager.Instance.OnHoldPressed += player.OnHold;
            
            
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

            InputManager.Instance.OnJumpPressed -= player.OnJump;
            InputManager.Instance.OnCrouchPressed -= player.OnCrouch;
            InputManager.Instance.OnHoldPressed -= player.OnHold;
            
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
            
            
            InputManager.Instance.OnJumpPressed += player.OnJump;
            InputManager.Instance.OnCrouchPressed += player.OnCrouch;
            InputManager.Instance.OnHoldPressed += player.OnHold;
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
