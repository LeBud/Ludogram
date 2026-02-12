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

        private void Update() {
            float forwardAmount = 0f;
            float turnAmount = 0f;
            
            var distance = Vector3.Distance(transform.position, target.position);

            if (distance > distanceToReachTarget) {
                var dot = Vector3.Dot(transform.forward, target.position);
                if (dot > 0f) forwardAmount = 1f;
                else forwardAmount = -1f;
                
                var angleToDir = Vector3.SignedAngle(transform.forward, target.position, Vector3.up);
                if(angleToDir > 0f) turnAmount = 1f;
                else turnAmount = -1f;
            }
            
            carController.SetAiInputs(forwardAmount, turnAmount);
        }
    }
}