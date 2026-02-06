using StateMachine.Finite_State_Machine_Interaces;

namespace StateMachine.Finite_State_Machine_class
{
	public class Transition : ITransition
	{
		public IState     TargetState { get; }
		public IPredicate Condition   { get; }

		public Transition(IState targetState,
			IPredicate condition)
		{
			TargetState = targetState;
			Condition = condition;
		}
	}
}