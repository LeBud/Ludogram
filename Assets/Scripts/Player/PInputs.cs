using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace.Player
{
    public class PInputs : MonoBehaviour
    {
        InputActionAsset       playerActions;
        private InputActionMap inputMap;
        [HideInInspector] public InputAction    move, look, jump, pickUp;

        void Awake()
        {
            playerActions = GetComponent<PlayerInput>().actions;
            inputMap = playerActions.FindActionMap("Player");
            
        }

        void OnEnable()
        {
            move = playerActions.FindAction("Move");
            look = playerActions.FindAction("Look");
            jump = playerActions.FindAction("Jump");
            pickUp = playerActions.FindAction("PickUpGadget");
            
            playerActions.Enable();
        }

        void OnDisable()
        {
            playerActions.Disable();
        }
    }
}