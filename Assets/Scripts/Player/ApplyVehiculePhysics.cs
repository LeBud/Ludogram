using System;
using CarScripts;
using UnityEngine;

namespace Player {
    public class ApplyVehiculePhysics : MonoBehaviour {
        private Controller player;
        private CarMotionTracker tracker;

        private Vector3 relativeVel;

        private void Awake() {
            if (TryGetComponent(out player)) Debug.Log($"Controller Assigned");
            else Debug.LogWarning($"Controller Not Found");
        }

        public Vector3 CalculateAngular() {
            if(tracker == null) return Vector3.zero;
            
            var relativePos = player.GetRB().position - tracker.CarRb().worldCenterOfMass;
            var rotationalVel = Vector3.Cross(tracker.AngularVelocity, relativePos);
            
            return rotationalVel;
        }

        public Vector3 CalculateVel() {
            if(tracker == null) return Vector3.zero;
            
            var playerLocalVel = player.GetRB().linearVelocity - tracker.LinearVelocity;
            return playerLocalVel + tracker.LinearVelocity;
        }

        public void SetTracker(CarMotionTracker tracker) {
            this.tracker = tracker;
            //player.PlayerStateMachine.ChangeState(Controller.ControlerState.InCar);
        }

        public void RemoveTracker() {
            var relativePos = player.GetRB().position - tracker.CarRb().worldCenterOfMass;
            
            player.GetRB().linearVelocity -= tracker.LinearVelocity;
            player.GetRB().linearVelocity -= Vector3.Cross(tracker.AngularVelocity, relativePos);
            
            tracker = null;
            //player.PlayerStateMachine.ChangeState(Controller.ControlerState.Idle);
        }
    }
}