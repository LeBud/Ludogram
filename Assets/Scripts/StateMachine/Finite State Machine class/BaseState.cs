using Player;
using StateMachine.Finite_State_Machine_Interaces;

namespace StateMachine.Finite_State_Machine_class
{
	public class BaseState : IState
	{
		protected readonly Controller player;

		protected BaseState(Controller player)
		{
			this.player = player;
		}
	
		public void OnEnter()
		{
			//
		}

		public void Update()
		{
			//
		}

		public void FixedUpdate()
		{
			//
		}

		public void LateUpdate()
		{
			//
		}

		public void OnExit()
		{
			//
		}
	}
}