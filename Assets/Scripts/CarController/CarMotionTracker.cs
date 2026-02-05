using System;
using UnityEngine;

namespace CarScripts {
    public class CarMotionTracker : MonoBehaviour {
        private CarController carController;

        public Vector3 LinearVelocity { get; private set; }
        public Vector3 AngularVelocity{ get; private set; }

        private void Awake() {
            if (TryGetComponent(out carController)) {
            }
            else Debug.LogError("No CarController found!");
        }

        private void FixedUpdate() {
            LinearVelocity = carController.GetRB().linearVelocity;
            AngularVelocity = carController.GetRB().angularVelocity;
        }

        public Rigidbody GetRB() {
            return carController.GetRB();
        }
    }
}