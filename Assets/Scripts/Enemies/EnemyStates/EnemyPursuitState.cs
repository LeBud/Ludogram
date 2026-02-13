using System.Linq;
using Player;
using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace EnemyStates {
    public class EnemyPursuitState : EnemyBaseState {
        public EnemyPursuitState(EnemyController ia, Animator animator) : base(ia, animator) {
        }

        public override void OnEnter() {
            animator.CrossFade(enemyLocomotionHash, crossFadeDuration, baseLayer);
        }

        public override void Update() {
            if(ia.InCar) return;
            //L'IA voiture c'est full prio sur le vanne que si il y a 1 sac ou plus dedans
            
            if(ia.money.HasTargetBag)
                ia.movement.MoveAt(ia.money.targetedBag.transform.position, true);
        }

        public override void OnExit() {
            if(ia.InCar) return;
            
            ia.movement.ResetMovement();
        }
    }
}

