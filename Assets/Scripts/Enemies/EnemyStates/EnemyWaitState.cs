using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace EnemyStates {
    public class EnemyWaitState : EnemyBaseState {
        public EnemyWaitState(EnemyController ia, Animator animator) : base(ia, animator) {
        }

        public override void OnEnter() {
            if (ia.InCar) {
                animator.CrossFade(enemySittingHash, crossFadeDuration, baseLayer);
                return;
            }
            
            ia.movement.ResetMovement();
        }

        public override void OnExit() {
            animator.CrossFade(enemyLocomotionHash, crossFadeDuration, baseLayer);
        }

    }
}