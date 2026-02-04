using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace {
    public class InputsBrain : MonoBehaviour {
        
        private InputActionAsset inputs;
        private InputActionMap inputMap;
        public InputAction Steering, Throttle, Brake;

        public void BindInput(PlayerInput pInput) {
            if (pInput) {
                inputs = pInput.actions;
                inputMap = inputs.FindActionMap("Car");
            }
            else Debug.LogError("No PlayerInput found");
        }

        private void EnableInputs() {
            Steering = inputMap.FindAction("Steering");
            Throttle = inputMap.FindAction("Throttle");
            Brake = inputMap.FindAction("Brake");
            
            inputs.Enable();
        }

        public void DisableInputs() {
            inputs.Disable();
        }
    }
}