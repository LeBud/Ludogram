using Player;
using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace StateMachine.BaseState_class
{
	public class MovementState : BaseState
	{
		public MovementState(Controller player, Animator animator) : base(player, animator) {
		}
		
		public override void OnEnter() {
			animator.CrossFade(locomotionHash, crossFadeDuration, baseLayer);
			Keyframe[] keyframes = player.gravityForceOverTime.keys;
			keyframes[0].value               = -player.jumpForceOverTime.Evaluate(player.jumpTime);
			player.gravityForceOverTime.keys = keyframes;
			player.jumpTime                  = 0f;
		}

		public override void Update()
		{
			player.CheckGround();
		}

		public override void FixedUpdate() {
			player.HandleMovement();
		}

		public override void LateUpdate()
		{
			player.HandleHeadbob();
		}

		public override void OnExit()
		{
		}
	}
}