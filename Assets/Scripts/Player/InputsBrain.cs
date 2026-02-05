using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class InputsBrain : MonoBehaviour
    {
        private PlayerInput pInput;
        private InputActionAsset playerActions;
        private InputActionMap playerMap;
        [HideInInspector] public InputAction    move, look, jump, pickUp, use, drop;
        
        private                 InputActionMap carMap;
        [HideInInspector] public InputAction    Steering, Throttle, Brake, LeaveCar;

        public void Initialize()
        {
            if (TryGetComponent(out pInput)) {
                playerActions = pInput.actions;
                playerMap = playerActions.FindActionMap("Player", true);
                carMap = playerActions.FindActionMap("Car", true);

                move = playerMap.FindAction("Move", true);
                look = playerMap.FindAction("Look", true);
                jump = playerMap.FindAction("Jump", true);
                pickUp = playerMap.FindAction("PickUp", true);
                use = playerMap.FindAction("Use", true);
                drop = playerMap.FindAction("DropGadget", true);
                
                Steering = carMap.FindAction("Steering");
                Throttle = carMap.FindAction("Throttle");
                Brake = carMap.FindAction("Brake");
                LeaveCar = carMap.FindAction("LeaveCar");
            }
            else {
                Debug.LogError("Player Input component not found!");
            }
        }

        public void DisablePlayerInput() {
            playerMap.Disable();
        }

        public void EnablePlayerInput() {
            playerMap.Enable();
        }

        public void EnableCarInput() {
            carMap.Enable();
        }
        
        public void DisableCarInput() {
            carMap.Disable();
        }
        
        public PlayerInput GetPlayerInput() {
            return pInput;
        }
    }
}