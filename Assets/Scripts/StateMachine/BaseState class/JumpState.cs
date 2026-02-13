using Player;
using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace StateMachine.BaseState_class
{
	public class JumpState : BaseState
	{
		public JumpState(Controller player, Animator animator) : base(player, animator)
		{
		}

		public override void OnEnter()
		{
			animator.CrossFade(jumpHash, crossFadeDuration, baseLayer);
			
			player.maxJumpTime = player.jumpForceOverTime.keys[player.jumpForceOverTime.length - 1].time;
			player.ResetHeadbob();
		}

		public override void Update()
		{
			player.CheckGround();
		}

		public override void FixedUpdate()
		{
			player.HandleMovement();
			player.HandleJump();
			
		}

		public override void OnExit()
		{
			animator.CrossFade(locomotionHash, crossFadeDuration, baseLayer);
			player.canReleaseJump = false;
			player.fallTime       = 0;
			player.isJumping = false;
			player.havePressedJump = false;
			
		}
	}
}