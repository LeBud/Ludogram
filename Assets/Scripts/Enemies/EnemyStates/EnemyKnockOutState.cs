using StateMachine.Finite_State_Machine_class;

namespace EnemyStates {
    public class EnemyKnockOutState : EnemyBaseState {
        public EnemyKnockOutState(Robber ia) : base(ia) {
        }

        public override void OnEnter() {
            
        }

        public override void Update() {
            
        }

        public override void OnExit() {
            
        }
        
        bool IsTimerFinished(){ return false; }
    }
}