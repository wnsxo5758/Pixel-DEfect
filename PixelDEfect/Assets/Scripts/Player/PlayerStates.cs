using UnityEditor.Experimental.GraphView;
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
            movement.Jump();
            
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
            
            player.IsOnLadder = true;
            
            movement.DisableGravity();
            animator.SetClimbAnim(player.IsOnLadder);
        }
        
        public override void Execute(PlayerController player)
        {
            float vertical = player.VerticalInput();
            
            //사다리 이동
            movement.Climb(vertical);
            //애니메이션
            animator.ClimbAnim(vertical);
        }

        public override void Exit(PlayerController player)
        {
            InputManager.Instance.OnLadderJumpPressed -= player.OnLadderJump;
            InputManager.Instance.OnJumpPressed += player.OnJump;
            InputManager.Instance.OnCrouchPressed += player.OnCrouch;
            InputManager.Instance.OnHoldPressed += player.OnHold;
            
            player.IsOnLadder = false;
            
            movement.EnableGravity();
            animator.SetClimbAnim(player.IsOnLadder);
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
