namespace StateMachine.Finite_State_Machine_Interaces
{
	public interface IState
	{
		void OnEnter();
		void Update();
		void FixedUpdate();
		void LateUpdate();
		void OnExit();
	}
}