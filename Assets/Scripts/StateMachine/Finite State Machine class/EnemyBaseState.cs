using StateMachine.Finite_State_Machine_Interaces;

namespace StateMachine.Finite_State_Machine_class {
    public class EnemyBaseState : IState {
        protected readonly Robber ia;

        protected EnemyBaseState(Robber ia) {
            this.ia = ia;
        }
	
        public virtual void OnEnter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() {}
        public virtual void LateUpdate(){}
        public virtual void OnExit(){}

    }
}