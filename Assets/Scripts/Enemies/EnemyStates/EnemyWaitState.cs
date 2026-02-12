using StateMachine.Finite_State_Machine_class;

namespace EnemyStates {
    public class EnemyWaitState : EnemyBaseState {
        public EnemyWaitState(EnemyController ia) : base(ia) {
        }

        public override void OnEnter() {
            if(ia.InCar) return;
            
            ia.movement.ResetMovement();
        }

    }
}