using Player;
using UnityEngine;

namespace CarScripts {
    public class CarMotionTracker : MonoBehaviour {
        private CarController carController;

        public Vector3 LinearVelocity { get; private set; }
        public Vector3 AngularVelocity{ get; private set; }

        private void Awake() {
            if (TryGetComponent(out carController)) Debug.Log($"Car Controller Assigned");
            else Debug.LogWarning($"Car Controller Not Found");
        }

        private void FixedUpdate() {
            LinearVelocity = carController.GetRB().linearVelocity;
            AngularVelocity = carController.GetRB().angularVelocity;
        }

        public Rigidbody GetRB() {
            return carController.GetRB();
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Player"))return;

            if (other.TryGetComponent(out ApplyVehiculePhysics physics)) {
                physics.SetTracker(this);
            }else Debug.Log("No physics found!");
        }

        private void OnTriggerExit(Collider other) {
            if(!other.CompareTag("Player")) return;
            
            if (other.TryGetComponent(out ApplyVehiculePhysics physics)) {
                physics.RemoveTracker();
            }else Debug.Log("No physics found!");
        }
    }
}