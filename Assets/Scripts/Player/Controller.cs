using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [Header("General Settings")]
    InputSystem_Actions playerInputActions;
    [SerializeField] TMP_Text currentStateTXT;
    [SerializeField] Rigidbody rb;
    [SerializeField] Camera    playerCamera;
    [SerializeField] Transform cameraTarget;
    [SerializeField] Transform groundRayPosiiton;
    
    
    

    [Header("Movement Settings")] 
    [SerializeField] AnimationCurve movementSpeedCurve;
    [SerializeField] AnimationCurve decelerationSpeedCurve;

    

    [Header("Camera Settings")]
    [SerializeField] float cameraSpeed;
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float verticalLimit    = 80f;

    Transform cameraTransform;
    Transform playerTransform;
    float     stopTimer;
    float     horizontalInput;
    float     verticalInput;
    float     movementTimer;
    float     cameraHorizontalInput;
    float     cameraVerticalInput;
    float     yaw;
    float     pitch;
    
    
    private Action<InputAction.CallbackContext> onMove;
    private Action<InputAction.CallbackContext> onLook;
    
    private Action onJump;
    private Action stopMove;
    
    
    [Header("State Machine")]
    public StateMachine<ControlerState> PlayerStateMachine = new ();
    public enum ControlerState
    {
        Idle,
        Moving,
        Falling,
        Jumping,
    }

    private void Start()
    {
        cameraTransform = playerCamera.transform;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        CreateState();
    }

    void CreateState()
    {
        //IDLE
        PlayerStateMachine.Add(new State<ControlerState>(
            ControlerState.Idle,
            _onEnter: IdleEnter,
            _onFixedUpdate: IdleFixedUpdate,
            _onLateUpdate: IdleLateUpdate,
            _onExit: IdleExit
        ));
        
        //MOVEMENT
        PlayerStateMachine.Add(new State<ControlerState>(
            ControlerState.Moving,
            _onEnter: MoveEnter,
            _onUpdate: MoveUpdate,
            _onFixedUpdate: MoveFixedUpdate,
            _onLateUpdate: MoveLateUpdate
        ));
        
        PlayerStateMachine.ChangeState(ControlerState.Idle);
    }

    #region STATE-MACHINE

    #region FUNCTION CALL

    void Update()
    {
        PlayerStateMachine?.Update();
        UpdateStateText();
    }

    void FixedUpdate()
    {
        PlayerStateMachine?.FixedUpdate();
    }

    void LateUpdate()
    {
        PlayerStateMachine?.LateUpdate();
    }

    #endregion

    #region IDLE

    void IdleEnter()
    {
        stopTimer = 0;
        movementTimer = 0;
    }

    // void IdleUpdate()
    // {
    //     
    // }
    //
    void IdleFixedUpdate()
    {
        stopTimer += Time.fixedDeltaTime;
        Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
        Vector3 forward = targetRotation * Vector3.forward;
        Vector3 right = targetRotation * Vector3.right;
    
        Vector3 move = (forward * verticalInput + right * horizontalInput).normalized;
        Vector3 velocity = move * decelerationSpeedCurve.Evaluate(stopTimer);
        velocity.y = rb.linearVelocity.y;
        
        rb.linearVelocity = velocity;
        
    }
    
    void IdleLateUpdate()
    {
        CameraMovement();
    }

    void IdleExit()
    {
        
    }

    #endregion
    
    #region MOVING

    void MoveEnter()
    {
        movementTimer = 0f;
        stopTimer = 0;
    }
    void MoveUpdate()
    {
        
    }
    
    void MoveFixedUpdate()
    {
        movementTimer += Time.fixedDeltaTime;
        
        Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
        Vector3 forward = targetRotation * Vector3.forward;
        Vector3 right = targetRotation * Vector3.right;
    
        Vector3 move = (forward * verticalInput + right * horizontalInput).normalized;
        Vector3 velocity = move * movementSpeedCurve.Evaluate(movementTimer);
        velocity.y = rb.linearVelocity.y;
        
        rb.linearVelocity = velocity;
    }

    void MoveLateUpdate()
    {
        CameraMovement();
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
        AssignActions();
        SubscribeInputSystemActions();
    }
    
    private void OnDisable()
    {
        UnsubscribeInputSystemActions();
        playerInputActions.Disable();
    }

    void SubscribeInputSystemActions()
    {
        //playerInputActions.Player.Jump.started += ctx => onLook?.Invoke(ctx);
        
        playerInputActions.Player.Move.performed += ctx => onMove?.Invoke(ctx);
        playerInputActions.Player.Look.performed += ctx => onLook?.Invoke(ctx);
        
        playerInputActions.Player.Move.canceled += _ => stopMove?.Invoke();
    }

    void AssignActions()
    {
        onMove += PlayerMovementInputs;
        onLook += CameraMovementsInputs;
        stopMove += ResetPlayerMovementInputs;
    }
    
    void UnsubscribeInputSystemActions()
    {
        playerInputActions.Player.Move.performed -= ctx => onMove?.Invoke(ctx);
        playerInputActions.Player.Look.performed -= ctx => onLook?.Invoke(ctx);
    }

    #endregion

    #region INPUT FUNCTIONS

    void PlayerMovementInputs(InputAction.CallbackContext context)
    {
        PlayerStateMachine.ChangeState(ControlerState.Moving);
        horizontalInput = context.ReadValue<Vector2>().x;
        verticalInput = context.ReadValue<Vector2>().y;
    }
    
    void ResetPlayerMovementInputs()
    {
        PlayerStateMachine.ChangeState(ControlerState.Idle);
    }
    
    void CameraMovementsInputs(InputAction.CallbackContext context)
    {
        cameraHorizontalInput = context.ReadValue<Vector2>().x * mouseSensitivity;
        cameraVerticalInput = context.ReadValue<Vector2>().y * mouseSensitivity; 
    }

    #endregion
    
    
    #region CAMERA

    void CameraMovement()
    {
        yaw += cameraHorizontalInput;
        pitch -= cameraVerticalInput;
        pitch = Mathf.Clamp(pitch, -verticalLimit, verticalLimit);
        
        cameraTransform.position = cameraTarget.position;
        cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.Euler(pitch, yaw, 0), Time.deltaTime * cameraSpeed);
        
    }

    #endregion

    #region GROUND

    bool IsGrounded()
    {
        Ray ray = new Ray(groundRayPosiiton.position, -groundRayPosiiton.up);
        return Physics.Raycast(ray, out RaycastHit hit, 1f);
    }

    #endregion

    void UpdateStateText()
    {
        currentStateTXT.text = PlayerStateMachine.currentState.iD.ToString();
    }
    
    //bool isGrounded() => Physics.Raycast(cameraTransform.position, Vector3.down, verticalLimit);
    
}
