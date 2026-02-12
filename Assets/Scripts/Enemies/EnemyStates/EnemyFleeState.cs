using StateMachine.Finite_State_Machine_class;

namespace EnemyStates {
    public class EnemyFleeState : EnemyBaseState {
        public EnemyFleeState(EnemyController ia) : base(ia) {
        }
        
        public override void OnEnter() {
            if(ia.InCar) return;
            
            ia.movement.ResetMovement();
        }

        public override void Update() {
            if(ia.InCar) return;
            
            if(ia.money.closestManhole)
                ia.movement.MoveAt(ia.money.closestManhole.transform.position, true);
        }

        public override void OnExit() {
            
        }
    }
}