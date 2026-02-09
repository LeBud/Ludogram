using System.Linq;
using Player;
using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace EnemyStates {
    public class EnemyPursuitState : EnemyBaseState {
        private Controller closestPlayer;
        public EnemyPursuitState(EnemyController ia) : base(ia) {
        }

        public override void OnEnter() {
            closestPlayer = SortClosestPlayerWithMoney();
        }

        public override void Update() {
            //If too far from a money bag -> Chase the car or a player if not all in the car - depend on what is the closest
            
            //Check for player if he has money
            //Else chase a money bag

            //Checker si tout les joueurs sont dans une voiture
            //Checker les sacs d'argents
            
            if(closestPlayer != null)
                ia.movement.MoveAt(closestPlayer.transform.position, true);
            
            if (GetDistanceToPlayer() > 10f) {
                //Soit changer de cible joueur
                //Soit abandonner la poursuite si il est trop loin
            }
        }

        public override void FixedUpdate() {
            
        }

        public override void OnExit() {
            ia.movement.ResetMovement();
        }

        private float GetDistanceToPlayer() {
            return Vector3.Distance(closestPlayer.transform.position, ia.transform.position); 
        }
        
        private Controller SortClosestPlayerWithMoney() {
            var index = 0;
            var distance = float.MaxValue;
            var foundPlayerWithMoney = false;
            
            for (var i = 0; i < ia.movement.playerInRange.Count; i++) { //Surement revoir le Get pour le money Bag
                if (ia.movement.playerInRange[i].GetGadget() as MoneyBag) {
                    var dist = Vector3.Distance(ia.transform.position, ia.movement.playerInRange[i].transform.position);
                    if (dist < distance) {
                        distance = dist;
                        index = i;
                    }
                    
                    foundPlayerWithMoney = true;
                }
            }
            
            if (foundPlayerWithMoney) return ia.movement.playerInRange[index];
            
            return null;
        }
    }
}

