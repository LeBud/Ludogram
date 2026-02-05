using CarScripts;
using UnityEngine;

namespace Player {
    public class ApplyVehiculePhysics : MonoBehaviour {
        private Controller player;
        private CarMotionTracker tracker;

        void FixedUpdate() {
            if(tracker == null) return;

            ApplyVehiculeMotion();
        }

        private void ApplyVehiculeMotion() {
            player.GetRB().linearVelocity += tracker.LinearVelocity;

            var relativePos = player.GetRB().position - tracker.GetRB().worldCenterOfMass;
            var rotationalVel = Vector3.Cross(tracker.AngularVelocity, relativePos);
            
            player.GetRB().linearVelocity += rotationalVel;
        }

        public void SetTracker(CarMotionTracker tracker) {
            this.tracker = tracker;
        }

        public void RemoveTracker() {
            tracker = null;
        }
    }
}