using System.Collections.Generic;
using Manager;
using Player;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies {
    public class EnemyMovementController : MonoBehaviour {
        private NavMeshAgent agent;
        private MoneyBag moneyBag; //Réf pour avoir le money bag a attraper

        [Header("Movement")] 
        [SerializeField] private float rotationSpeed = 12f;

        [HideInInspector]
        public List<Controller> playerInRange = new();
        
        public void Initialize(NavMeshAgent agent) {
            this.agent = agent;
        }

        public void ResetMovement() {
            agent.ResetPath();
        }
        
        public void MoveAt(Vector3 position, bool lookAtWhereItMove = false) {
            agent.SetDestination(position);
            
            if(lookAtWhereItMove)
                LookAt(position);
        }

        public void GetPlayers() {
            playerInRange.Clear();

            foreach (var player in GameManager.instance.GetPlayers()) {
                playerInRange.Add(player);
            }
        }
        
        private void LookAt(Vector3 position) {
            var dir = (position - transform.position).normalized;
            if (dir == Vector3.zero) return;
            dir.y = 0;
            dir.Normalize();
            var lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

    }
}