using Player;
using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace StateMachine.BaseState_class
{
	public class StunState : BaseState
	{
		public StunState(Controller player, Animator animator) : base(player, animator)
		{
		}

		public override void OnEnter()
		{
			
		}

		public override void Update()
		{
			player.HandleKnockTimer();
		}

		public override void OnExit()
		{
			
		}
		
	}
}