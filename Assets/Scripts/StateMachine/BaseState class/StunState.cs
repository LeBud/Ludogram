using Player;
using StateMachine.Finite_State_Machine_class;

namespace StateMachine.BaseState_class
{
	public class StunState : BaseState
	{
		public StunState(Controller player) : base(player)
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