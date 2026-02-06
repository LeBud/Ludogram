using Player;
using StateMachine.Finite_State_Machine_class;

namespace StateMachine.BaseState_class
{
	public class CarState : BaseState
	{
		public CarState(Controller player) : base(player) { }

		public override void OnEnter()
		{
			//Unbind et rebind look
			player.GetInputs().SetLookCar(true);
			//player.Rebind
			player.currentCar.BindInput(player.pInput, player);
			player.GetInputs().DisablePlayerInput();
			player.GetInputs().EnableCarInput();
		}
		public override void OnExit()
		{
			player.GetInputs().DisableCarInput();
			player.GetInputs().EnablePlayerInput();
		}
	}
}