using Player;
using StateMachine.Finite_State_Machine_class;

namespace StateMachine.BaseState_class
{
    public class SeatedState : BaseState
    {
        public SeatedState(Controller player) : base(player)
        {
        }

        public override void OnEnter()
        {
            player.UnbindLook();
            player.GetInputs().SetLookCar(true);
            player.RebindLook();
            
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