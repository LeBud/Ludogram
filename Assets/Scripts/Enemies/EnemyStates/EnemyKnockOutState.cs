using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace EnemyStates {
    public class EnemyKnockOutState : EnemyBaseState {
        private float timer = 0f;
        public EnemyKnockOutState(Robber ia) : base(ia) {
        }

        public override void OnEnter() {
            ia.movement.ResetMovement();
            
            ia.UnKnockOut();
            ia.rigidbody.isKinematic = false;
            ia.rigidbody.constraints = RigidbodyConstraints.None;
        }

        public override void Update() {
            timer += Time.deltaTime;
        }

        public override void OnExit() {
            ia.rigidbody.isKinematic = true;
            ia.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            ia.transform.rotation = Quaternion.Euler(0,ia.transform.eulerAngles.y,0);
        }
        
        public bool IsTimerFinished() => timer >= ia.knockOutTime;
    }
}