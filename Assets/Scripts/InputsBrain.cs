using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace {
    [RequireComponent(typeof(PlayerInput))]
    public class InputsBrain : MonoBehaviour {
        
        private InputActionAsset inputs;
        private InputActionMap inputMap;
        public InputAction Steering, Throttle, ShiftGear;

        public void Awake() {
            if (TryGetComponent(out PlayerInput pInput)) {
                inputs = pInput.actions;
                inputMap = inputs.FindActionMap("Car");
            }
            else Debug.LogError("No PlayerInput found");
        }

        public void OnEnable() {
            Steering = inputMap.FindAction("Steering");
            Throttle = inputMap.FindAction("Throttle");
            ShiftGear = inputMap.FindAction("ShiftGear");
            
            inputs.Enable();
        }

        private void OnDisable() {
            inputs.Disable();
        }
        
    }
}