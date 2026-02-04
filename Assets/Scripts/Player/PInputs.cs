using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace.Player
{
    public class PInputs : MonoBehaviour
    {
        private PlayerInput pInput;
        private InputActionAsset playerActions;
        private InputActionMap playerMap;
        [HideInInspector] public InputAction    move, look, jump, pickUp;

        void Start()
        {
            if (TryGetComponent(out pInput)) {
                playerActions = pInput.actions;
                playerMap = playerActions.FindActionMap("Player", true);
                
                move = playerMap.FindAction("Move", true);
                look = playerMap.FindAction("Look", true);
                jump = playerMap.FindAction("Jump", true);
                pickUp = playerMap.FindAction("PickUpGadget", true);
            }
            else {
                Debug.LogError("Player Input component not found!");
            }
        }

        void OnEnable() {
            //EnablePlayerInput();
        }

        void OnDisable() {
            //DisablePlayerInput();
        }

        public void DisablePlayerInput() {
            playerMap.Disable();
        }

        public void EnablePlayerInput() {
            playerMap.Enable();
        }
        
        public PlayerInput GetPlayerInput() {
            return pInput;
        }
    }
}