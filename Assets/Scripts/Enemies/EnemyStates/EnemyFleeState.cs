using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace EnemyStates {
    public class EnemyFleeState : EnemyBaseState {
        public EnemyFleeState(EnemyController ia, Animator animator) : base(ia, animator) {
        }
        
        public override void OnEnter() {
            animator.CrossFade(enemyLocomotionHash, crossFadeDuration, baseLayer);
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