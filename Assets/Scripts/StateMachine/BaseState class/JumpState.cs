using Player;
using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace StateMachine.BaseState_class
{
	public class JumpState : BaseState
	{
		public JumpState(Controller player) : base(player)
		{
		}

		public override void OnEnter()
		{
			Debug.Log("EnterJump");
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
			Debug.Log("ExitJump");
			player.canReleaseJump = false;
			player.fallTime       = 0;
			player.isJumping = false;
		}
	}
}