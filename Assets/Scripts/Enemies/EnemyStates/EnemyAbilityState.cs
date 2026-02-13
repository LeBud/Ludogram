using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace EnemyStates {
    public class EnemyAbilityState : EnemyBaseState {
        private float stateDuration = 0f;
        public EnemyAbilityState(EnemyController ia, Animator animator) : base(ia, animator) {
        }

        public override void OnEnter() {
            animator.CrossFade(enemyOpenMouthHash, crossFadeDuration, mouthLayer);
            animator.CrossFade(enemyGrabTongueHash, crossFadeDuration, torsoLayer);
            animator.CrossFade(enemyGrabArmsHash, crossFadeDuration, torsoLayer);
            
            stateDuration = ia.ability.abilityStateDuration;
            ia.ability.triggerAbility = false;
            ia.ability.canUseTongue = false;

            if (ia.InCar)
            {
                return;
            }
            
            ia.movement.ResetMovement();
            Debug.Log("Enter Ability State");
        }

        public override void Update() {
            stateDuration -= Time.deltaTime;
        }

        public override void OnExit() {
            animator.CrossFade(enemyMouthClosedHash, crossFadeDuration, mouthLayer);
            ia.ability.canUseTongue = true;
        }

        public bool IsStateFinished() {
            return stateDuration <= 0f;
        }

    }
}