using StateMachine.BaseState_class;
using StateMachine.Finite_State_Machine_Interaces;

namespace Player
{
	public class ControllerV2
	{
		private FiniteStateMachine stateMachine;

		void Awake()
		{
			stateMachine = new FiniteStateMachine();

			MovementState movementState = new MovementState(this);
			JumpState     jumpState     = new JumpState(this);
			StunState     stunState     = new StunState(this);
			CarState      carState      = new CarState(this);
		}

		void At(IState from, IState to, IPredicate condition)
		{
			stateMachine.AddTransition(from, to, condition);
		}

		void Any(IState to, IPredicate  condition)
		{
			stateMachine.AddAnyTransition(to, condition);
		}
	}
}