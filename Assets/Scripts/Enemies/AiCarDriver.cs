using System;
using CarScripts;
using UnityEngine;

namespace Enemies {
    public class AiCarDriver : MonoBehaviour {
        private CarController carController;
        private Transform target;

        [SerializeField] private float distanceToReachTarget;
        
        private void Awake() {
            if(TryGetComponent(out carController)) Debug.Log("AiCarDriver Awake");
            else Debug.LogError("No car controller found");
            
            carController.SetAiCar(true);
        }

        private void Start() {
            target = FindAnyObjectByType<AttachedPlayer>().transform;
        }

        private void Update() {
            float forwardAmount = 0f;
            float turnAmount = 0f;
            
            var distance = Vector3.Distance(transform.position, target.position);

            if (distance > distanceToReachTarget) {
                var dirToMovePos = (target.position - transform.position).normalized;
                var dot = Vector3.Dot(transform.forward, dirToMovePos);
                if (dot > 0f) forwardAmount = 1f;
                else forwardAmount = -1f;
                
                var angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePos, Vector3.up);
                if(angleToDir > 5f) turnAmount = 1f;
                else if(angleToDir < -5f) turnAmount = -1f;
                else  turnAmount = 0f;
            }
            
            carController.SetAiInputs(forwardAmount, turnAmount);
        }
    }
}