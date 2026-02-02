using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [Header("General Settings")]
    InputSystem_Actions playerInputActions;
    [SerializeField]  Rigidbody rb;
    [SerializeField]  Camera    playerCamera;
    [SerializeField]  Transform cameraTarget;
    
    Transform cameraTransform;
    Transform playerTransform;

    [Header("Movement Settings")] 
    [SerializeField] AnimationCurve movementSpeedCurve;
    float horizontalInput;
    float verticalInput;
    float movementTimer;

    [Header("Camera Settings")]
    [SerializeField] float cameraSpeed;
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float verticalLimit    = 80f;

    float cameraHorizontalInput;
    float cameraVerticalInput;
    float yaw;
    float pitch;
    
    
    private Action<InputAction.CallbackContext> onMove;
    private Action<InputAction.CallbackContext> onLook;

    [Header("State Machine")]
    public StateMachine<ControlerState> playerStateMachine = new ();
    public enum ControlerState
    {
        Idle,
        Moving,
        Falling,
        Jumping,
    }
    
    void CreateState()
    {
        
    }

    #region STATE-MACHINE

    #region IDLE

    void IdleUpdate()
    {
        
    }

    void IdleFixedUpdate()
    {
        
    }

    #endregion
    
    #region MOVING

    void MovingUpdate()
    {
        
    }
    
    void MovingFixedUpdate()
    {
        
    }

    #endregion
    
    #region FALLING

    void FallUpdate()
    {
        
    }

    void FallFixedUpdate()
    {
        
    }
    
    #endregion
    
    #region JUMPING

    void JumpUpdate()
    {
        
    }

    void JumpFixedUpdate()
    {
        
    }

    #endregion

    #endregion
    
    

    #region INPUT SYSTEM SETUP

    private void OnEnable()
    {
        playerInputActions = new InputSystem_Actions();
        playerInputActions.Enable();
        SubscribeInputSystemActions();
    }
    
    private void OnDisable()
    {
        UnsubscribeInputSystemActions();
        playerInputActions.Disable();
    }

    void SubscribeInputSystemActions()
    {
        playerInputActions.Player.Move.performed += ctx => onMove?.Invoke(ctx);
        playerInputActions.Player.Look.performed += ctx => onLook?.Invoke(ctx);
    }

    void AssignActions()
    {
        onMove += PlayerMovementInputs;
        onLook += CameraMovementsInputs;
    }
    
    void UnsubscribeInputSystemActions()
    {
        playerInputActions.Player.Move.performed -= ctx => onMove?.Invoke(ctx);
        playerInputActions.Player.Look.performed -= ctx => onLook?.Invoke(ctx);
    }

    #endregion


    void FixedUpdate()
    {
        PlayerMovements();
    }

    private void LateUpdate()
    {
        CameraMovement();
    }

    void PlayerMovementInputs(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<Vector2>().x;
        verticalInput = context.ReadValue<Vector2>().y;
    }

    void PlayerMovements()
    {
        Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
        Vector3 forward = targetRotation * Vector3.forward;
        Vector3 right = targetRotation * Vector3.right;
    
        Vector3 move = (forward * verticalInput + right * horizontalInput).normalized;
        Vector3 velocity = move * movementSpeedCurve.Evaluate(movementTimer);
        velocity.y = rb.linearVelocity.y;
        
        rb.linearVelocity = velocity;
    }


    void CameraMovementsInputs(InputAction.CallbackContext context)
    {
        cameraHorizontalInput = context.ReadValue<Vector2>().x;
        cameraVerticalInput = context.ReadValue<Vector2>().y;
    }

    void CameraMovement()
    {
        yaw += cameraHorizontalInput;
        pitch -= cameraVerticalInput;
        pitch = Mathf.Clamp(pitch, -verticalLimit, verticalLimit);
        
        transform.rotation    = Quaternion.Euler(0, yaw, 0);

        cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.Euler(pitch, yaw, 0), Time.deltaTime * cameraSpeed);
        cameraTransform.position = cameraTarget.position;
    }
    
}
