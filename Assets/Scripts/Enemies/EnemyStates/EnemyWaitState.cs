using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace EnemyStates {
    public class EnemyWaitState : EnemyBaseState {
        private float timer;
        private Vector3 randomPos;
        
        public EnemyWaitState(EnemyController ia, Animator animator) : base(ia, animator) {
        }

        public override void OnEnter() {
            if (ia.InCar) {
                animator.CrossFade(enemySittingHash, crossFadeDuration, baseLayer);
                return;
            }
            
            ia.movement.ResetMovement();
            randomPos = ia.transform.position + Random.insideUnitSphere * 5f;
        }

        public override void Update() {
            if(ia.InCar) return;
            timer += Time.deltaTime;
            if(timer >= 5f)
                randomPos = ia.transform.position + Random.insideUnitSphere * 5f;
            
            ia.movement.MoveAt(randomPos);
        }
        

        public override void OnExit() {
            animator.CrossFade(enemyLocomotionHash, crossFadeDuration, baseLayer);
        }

    }
}