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
            float input = player.HandleInput();
            animator.MovementAnim(input);
            
            if (input != 0f)
            {
                player.ChangeState(new Run());
            }
            
            player.HandleJump();
            player.HandleHold();
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
            float input = player.HandleInput();
            animator.MovementAnim(input);
            
            if (input == 0f)
            {
                player.ChangeState(new Idle());
            }
            
            player.HandleJump();
            player.HandleHold();
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
            movement = player.gameObject.GetComponent<MovementRigidbody2D>();
            animator = player.GetComponentInChildren<PlayerAnimator>();
            movement.Jump();
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HandleInput();
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
        }

        public override void Execute(PlayerController player)
        {
            float input = player.HandleInput();
            animator.PushAndPullAnim(input);
            if (playerInteraction.IsConnected == false)
            {
                player.RevertToPreviousState();
            }
            
            player.UpdateMove(input);
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
        }

        public override void Exit(PlayerController player)
        {
            
        }
    }
}
