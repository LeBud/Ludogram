using Player;
using StateMachine.Finite_State_Machine_Interaces;

namespace StateMachine.Finite_State_Machine_class
{
	public class BaseState : IState
	{
		protected readonly ControllerV2 player;

		protected BaseState(ControllerV2 player)
		{
			this.player = player;
		}
	
		public virtual void OnEnter() { }
		public virtual void Update() { }
		public virtual void FixedUpdate() {}
		public virtual void LateUpdate(){}
		public virtual void OnExit(){}
	}
}