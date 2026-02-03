using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [Header("General Settings")]
    InputSystem_Actions playerInputActions;
    [SerializeField]         TMP_Text  currentStateTxt;
    [SerializeField]         Rigidbody rb;
    [SerializeField]         Camera    playerCamera;
    [SerializeField] private Transform cameraTarget;
    [SerializeField]         Transform groundRayPosition;
    [SerializeField]         LayerMask groundLayerMask;

    [Header("Movement State Settings")] 
    [SerializeField] AnimationCurve movementSpeedCurve;
    [SerializeField] AnimationCurve airMovementSpeedCurve;
    [SerializeField] AnimationCurve decelerationSpeedCurve;
    
    [Header("Jump State Settings")]
    [SerializeField] AnimationCurve jumpSpeedCurve;
    [SerializeField] float minJumpTime = 0.2f;
    [SerializeField] float maxJumpTime = 0.5f;
    [SerializeField] int   maxJumpNumber;

    [Header("Fall State Settings")]
    [SerializeField] AnimationCurve fallSpeedCurve;

    [Header("Camera Settings")]
    [SerializeField] float cameraSpeed;
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float verticalLimit    = 80f;

    public bool isGrounded;

    Transform cameraTransform;
    Transform playerTransform;

    //MOVEMENTS
    public float movementTimer;
    public float stopTimer;
    public float horizontalInput;
    public float verticalInput;
    
    //JUMP
    int currentJumpNumber;
    float jumpTimer;
    bool isOnJump;

    //FALL
    public float fallTimer;

    //CAMERA
    float cameraHorizontalInput;
    float cameraVerticalInput;
    float yaw;
    float pitch;
    
    
    private Action<InputAction.CallbackContext> onMove;
    private Action<InputAction.CallbackContext> onLook;
    
    private Action onJump;
    private Action stopJump;
    private Action stopMove;
    
    private Coroutine jumpCoroutine;
    
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
            _onUpdate: IdleUpdate,
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
        
        //JUMP
        PlayerStateMachine.Add(new State<ControlerState>(
            ControlerState.Jumping,
            _onEnter: JumpEnter,
            _onFixedUpdate: JumpFixedUpdate,
            _onLateUpdate: JumpLateUpdate,
            _onExit: JumpExit
        ));
        
        //FALL
        PlayerStateMachine.Add(new State<ControlerState>(
            ControlerState.Falling,
            _onEnter: FallEnter,
            _onUpdate: FallUpdate,
            _onFixedUpdate: FallFixedUpdate,
            _onLateUpdate: FallLateUpdate
        ));
        
        PlayerStateMachine.ChangeState(ControlerState.Idle);
    }

    #region STATE-MACHINE

    #region FUNCTION CALL

    void Update()
    {
        PlayerStateMachine?.Update();
        Debug();
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
    }

    void IdleUpdate()
    {
       
    }
    
    void IdleFixedUpdate()
    {
        if(horizontalInput == 0 || verticalInput == 0) return;
        stopTimer += Time.fixedDeltaTime;
        Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
        Vector3 forward = targetRotation * Vector3.forward;
        Vector3 right = targetRotation * Vector3.right;
    
        Vector3 move = (forward * verticalInput + right * horizontalInput).normalized;
        Vector3 velocity = move * decelerationSpeedCurve.Evaluate(stopTimer);
        velocity.y = rb.linearVelocity.y;
        
        rb.linearVelocity = velocity;
        if (rb.linearVelocity.magnitude > 0.01f)
        {
            movementTimer = 0;
        }
    }
    
    void IdleLateUpdate()
    {
        CameraMovement();
    }

    void IdleExit()
    {
        horizontalInput = 0;
        verticalInput = 0;
    }

    #endregion
    
    #region MOVING

    void MoveEnter()
    {
        stopTimer       = 0;
    }
    void MoveUpdate()
    {
        if (!IsGrounded())
        {
            PlayerStateMachine?.ChangeState(ControlerState.Falling);
        }
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

    void FallEnter()
    {
        fallTimer       = 0;
    }
    void FallUpdate()
    {
        if (IsGrounded())
        {
            isOnJump = false;
            currentJumpNumber = 0;
            PlayerStateMachine.ChangeState(ControlerState.Idle);
            
        }
    }

    void FallFixedUpdate()
    {
        fallTimer     += Time.fixedDeltaTime;
        movementTimer += Time.fixedDeltaTime;
        Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
        Vector3    forward        = targetRotation * Vector3.forward;
        Vector3    right          = targetRotation * Vector3.right;
    
        Vector3 move     = (forward * verticalInput + right * horizontalInput).normalized;
        Vector3 velocity = move * airMovementSpeedCurve.Evaluate(movementTimer);
        velocity.y = -fallSpeedCurve.Evaluate(fallTimer);
        
        rb.linearVelocity = velocity;
    }

    void FallLateUpdate()
    {
        CameraMovement();
    }
    
    #endregion
    
    #region JUMPING

    void JumpEnter()
    {
        isOnJump  = true;
        jumpTimer = 0;
    }

    void JumpFixedUpdate()
    {
        jumpTimer     += Time.fixedDeltaTime;
        movementTimer += Time.fixedDeltaTime;
        
        Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
        Vector3    forward        = targetRotation * Vector3.forward;
        Vector3    right          = targetRotation * Vector3.right;
    
        Vector3 move     = (forward * verticalInput + right * horizontalInput).normalized;
        Vector3 velocity = move * airMovementSpeedCurve.Evaluate(movementTimer);
        velocity.y = jumpSpeedCurve.Evaluate(jumpTimer);
        
        rb.linearVelocity = velocity;

        if (jumpTimer > minJumpTime && !isOnJump)
        {
            PlayerStateMachine.ChangeState(ControlerState.Falling);
        }

        if (jumpTimer > maxJumpTime)
        {
            PlayerStateMachine.ChangeState(ControlerState.Falling);
        }
        
    }

    void JumpExit()
    {
        
    }

    void JumpLateUpdate()
    {
        CameraMovement();
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
        playerInputActions.Player.Jump.started += _ => onJump?.Invoke();
        
        playerInputActions.Player.Move.performed += ctx => onMove?.Invoke(ctx);
        playerInputActions.Player.Look.performed += ctx => onLook?.Invoke(ctx);
        
        playerInputActions.Player.Move.canceled += _ => stopMove?.Invoke();
        playerInputActions.Player.Jump.canceled  += _ => stopJump?.Invoke();
    }

    void AssignActions()
    {
        onMove   += PlayerMovementInputs;
        onLook   += CameraMovementsInputs;
        stopMove += ResetPlayerMovementInputs;
        onJump   += JumpInput;
        stopJump += StopJumpInput;
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
        if(IsGrounded() && !isOnJump)PlayerStateMachine.ChangeState(ControlerState.Moving);
        horizontalInput = context.ReadValue<Vector2>().x;
        verticalInput = context.ReadValue<Vector2>().y;
    }
    
    void ResetPlayerMovementInputs()
    {
        if (IsGrounded() && !isOnJump)
        {
            PlayerStateMachine.ChangeState(ControlerState.Idle);
        }
    }
    
    void CameraMovementsInputs(InputAction.CallbackContext context)
    {
        cameraHorizontalInput = context.ReadValue<Vector2>().x * mouseSensitivity;
        cameraVerticalInput = context.ReadValue<Vector2>().y * mouseSensitivity; 
    }

    void JumpInput()
    {
        if(IsGrounded() || currentJumpNumber < maxJumpNumber)
        {
            currentJumpNumber++;
            PlayerStateMachine.ChangeState(ControlerState.Jumping);
        }
    }

    void StopJumpInput()
    {
        if(IsGrounded()) return;
        isOnJump = false;
        if(jumpTimer < minJumpTime) return;
        if (!IsGrounded())
        {
            PlayerStateMachine.ChangeState(ControlerState.Falling);
        }
        else
        {
            PlayerStateMachine.ChangeState(ControlerState.Idle);
        }
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
        Ray ray = new Ray(groundRayPosition.position, -groundRayPosition.up);
        return Physics.Raycast(ray, out RaycastHit ground, 0.5f, groundLayerMask);
    }

    #endregion

    void Debug()
    {
        currentStateTxt.text = PlayerStateMachine.currentState.iD.ToString();
        isGrounded = IsGrounded();
        UnityEngine.Debug.DrawRay(groundRayPosition.position, -groundRayPosition.up * 0.5f, Color.red);
    }
    //bool isGrounded() => Physics.Raycast(cameraTransform.position, Vector3.down, verticalLimit);
    
}
