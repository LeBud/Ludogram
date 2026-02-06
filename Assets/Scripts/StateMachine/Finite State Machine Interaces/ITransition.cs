namespace StateMachine.Finite_State_Machine_Interaces
{
	public interface ITransition
	{
		IState     TargetState { get; }
		IPredicate Condition   { get; }
	}
}