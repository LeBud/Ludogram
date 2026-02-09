using System.Linq;
using Player;
using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace EnemyStates {
    public class EnemyPursuitState : EnemyBaseState {
        private Transform closestPlayer;
        public EnemyPursuitState(EnemyController ia) : base(ia) {
        }

        public override void OnEnter() {
            closestPlayer = SortClosestPlayer();
        }

        public override void Update() {
            ia.movement.MoveAt(closestPlayer.position, true);

            if (GetDistanceToPlayer() > 10f) {
                //Soit changer de cible joueur
                //Soit abandonner la poursuite si il est trop loin
            }
        }

        public override void FixedUpdate() {
            
        }

        public override void OnExit() {
            
        }

        private float GetDistanceToPlayer() {
            return Vector3.Distance(closestPlayer.position, ia.transform.position); 
        }
        
        private Transform SortClosestPlayer() {
            return ia.movement.playerInRange.OrderBy(player => Vector3.Distance(player.position, ia.transform.position)).First();
        }
    }
}

