using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace EnemyStates {
    public class EnemyWaitState : EnemyBaseState {
        public EnemyWaitState(EnemyController ia, Animator animator) : base(ia, animator) {
        }

        public override void OnEnter() {
            animator.CrossFade(enemySittingHash, crossFadeDuration, baseLayer);
            if (ia.InCar)
            {
                return;
            }
            
            ia.movement.ResetMovement();
        }

    }
}