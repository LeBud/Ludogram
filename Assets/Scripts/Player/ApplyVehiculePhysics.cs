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

        void FixedUpdate() {
            if(tracker == null) return;

            ApplyVehiculeMotion();
        }

        private void ApplyVehiculeMotion() {
            var playerLocalVel = player.GetRB().linearVelocity - tracker.LinearVelocity;
            player.GetRB().linearVelocity = playerLocalVel + tracker.LinearVelocity;
            
            var relativePos = player.GetRB().position - tracker.GetRB().worldCenterOfMass;
            var rotationalVel = Vector3.Cross(tracker.AngularVelocity, relativePos);
            
            player.GetRB().linearVelocity += rotationalVel;
        }

        public void SetTracker(CarMotionTracker tracker) {
            this.tracker = tracker;
        }

        public void RemoveTracker() {
            var relativePos = player.GetRB().position - tracker.GetRB().worldCenterOfMass;
            
            player.GetRB().linearVelocity -= tracker.LinearVelocity;
            player.GetRB().linearVelocity -= Vector3.Cross(tracker.AngularVelocity, relativePos);
            
            tracker = null;
        }
    }
}