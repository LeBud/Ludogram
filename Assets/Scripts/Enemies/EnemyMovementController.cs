using System.Collections.Generic;
using Manager;
using Player;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies {
    public class EnemyMovementController : MonoBehaviour {
        private EnemyController controller;
        private NavMeshAgent agent;
        private MoneyBag moneyBag; //Réf pour avoir le money bag a attraper

        [Header("Movement")] 
        [SerializeField] private float rotationSpeed = 12f;

        [HideInInspector]
        public List<Controller> playerInRange = new();
        
        public void Initialize(NavMeshAgent agent, EnemyController  controller) {
            this.controller =  controller;
            this.agent = agent;
        }

        void Update()
        {
            controller.animator.SetFloat("Blend",  agent.velocity.magnitude);
        }
        public void ResetMovement() {
            agent.ResetPath();
        }
        
        public void MoveAt(Vector3 position, bool lookAtWhereItMove = false) {
            agent.SetDestination(position);
            
            if(lookAtWhereItMove)
                LookAt(position);
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