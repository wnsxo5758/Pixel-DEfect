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
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HorizontalInput();
            
            animator.MovementAnim(input);
            
            if (movement.IsGrounded && movement.Velocity.y <= 0.01f)
            {
                player.RevertToPreviousState();
            }
            
            player.UpdateMove(input);
            player.SpriteFlipX(input);
        }
        
        public override void Exit(PlayerController player)
        {
            
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
            
            // 점프 비활성화
            InputManager.Instance.OnJumpPressed -= player.OnJump;
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
            InputManager.Instance.OnJumpPressed -= player.OnJump;
            InputManager.Instance.OnLadderJumpPressed += player.OnLadderJump;
            
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
            InputManager.Instance.OnJumpPressed += player.OnJump;
            InputManager.Instance.OnLadderJumpPressed -= player.OnLadderJump;
            
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
