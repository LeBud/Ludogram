using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [Header("General Settings")]
    InputSystem_Actions playerInputActions;
    [SerializeField] TMP_Text  currentStateTxt;
    [SerializeField] Rigidbody rb;
    [SerializeField] Camera    playerCamera;
    [SerializeField] Transform cameraTarget;
    [SerializeField] Transform groundRayPosition;
    [SerializeField] LayerMask groundLayerMask;

    [Header("Movement State Settings")] 
    [SerializeField] AnimationCurve movementSpeedCurve;
    [SerializeField] AnimationCurve airMovementSpeedCurve;
    [SerializeField] AnimationCurve decelerationSpeedCurve;
    
    [Header("Fall State Settings")]
    [SerializeField] AnimationCurve fallSpeedCurve;
    
    [Header("Jump State Settings")]
    [SerializeField] AnimationCurve jumpSpeedCurve;
    [SerializeField] float minJumpTime = 0.2f;
    [SerializeField] float maxJumpTime = 0.5f;
    [SerializeField] int   maxJumpNumber;
    [SerializeField] float coyoteTime;
    [SerializeField] float bufferTime;

    [Header("Camera Settings")]
    [SerializeField] float cameraSpeed;
    [SerializeField] float lookSensitivity = 2f;
    [SerializeField] float verticalLimit    = 80f;

    public bool isGrounded;

    Transform cameraTransform;
    Transform playerTransform;

    //MOVEMENTS
    float movementTimer;
    float stopTimer;
    float horizontalInput;
    float verticalInput;
    
    //JUMP
    int   currentJumpNumber;
    float jumpTimer;
    bool  isOnJump;
    bool  canStopJump;

    //FALL
    public float      fallTimer;

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
    
    private Coroutine bufferJumpCoroutine;
    
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
        cameraTransform                      = playerCamera.transform;
        rb.constraints                       = RigidbodyConstraints.FreezeRotation;
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
        Keyframe[] keyframes = decelerationSpeedCurve.keys;
        keyframes[0].value          = movementSpeedCurve.Evaluate(movementTimer);
        decelerationSpeedCurve.keys = keyframes;
        //movementTimer = 0;
    }

    void IdleUpdate()
    {
        // if (!IsGrounded() && !isOnJump)
        // {
        //     PlayerStateMachine.ChangeState(ControlerState.Falling);
        // }
    }
    
    void IdleFixedUpdate()
    {
        stopTimer += Time.fixedDeltaTime;
        if (horizontalInput == 0 && verticalInput == 0)
        {
            movementTimer = 0;
            return;
        }
        
        Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
        Vector3 forward = targetRotation * Vector3.forward;
        Vector3 right = targetRotation * Vector3.right;
    
        Vector3 move = (forward * verticalInput + right * horizontalInput).normalized;
        Vector3 velocity = move * decelerationSpeedCurve.Evaluate(stopTimer);
        velocity.y = rb.linearVelocity.y;
        
        rb.linearVelocity = velocity;
        // if (rb.linearVelocity.magnitude > 0.01f)
        // {
        //     movementTimer = 0;
        // }
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
        stopTimer = 0;
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
        Keyframe[] keyframes = fallSpeedCurve.keys;
        keyframes[0].value = -jumpSpeedCurve.Evaluate(jumpTimer);
        fallSpeedCurve.keys = keyframes;
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
        fallTimer         += Time.fixedDeltaTime;
        movementTimer     += Time.fixedDeltaTime;
        Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
        Vector3    forward        = targetRotation * Vector3.forward;
        Vector3    right          = targetRotation * Vector3.right;
    
        Vector3 move     = (forward * verticalInput + right * horizontalInput).normalized;
        Vector3 velocity = move * airMovementSpeedCurve.Evaluate(movementTimer);
        velocity.y += -fallSpeedCurve.Evaluate(fallTimer);
        
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
        if(bufferJumpCoroutine != null) StopCoroutine(bufferJumpCoroutine);
        isOnJump    = true;
        canStopJump = false;
        jumpTimer   = 0;
        fallTimer   = 0;
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
        velocity.y += jumpSpeedCurve.Evaluate(jumpTimer);
        
        rb.linearVelocity = velocity;

        if (jumpTimer > minJumpTime && canStopJump)
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
        playerInputActions.Player.Jump.started -= _ => onJump?.Invoke();
        
        playerInputActions.Player.Move.performed -= onMove;
        playerInputActions.Player.Look.performed -= onLook;
        
        playerInputActions.Player.Move.canceled -= _ => stopMove?.Invoke();
        playerInputActions.Player.Jump.canceled -= _ => stopJump?.Invoke();
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
        cameraHorizontalInput = context.ReadValue<Vector2>().x;
        cameraVerticalInput   = context.ReadValue<Vector2>().y;
    }

    void JumpInput()
    {
        if(IsGrounded() || currentJumpNumber < maxJumpNumber || (fallTimer < coyoteTime && currentJumpNumber == 0))
        {
            currentJumpNumber++;
            if (isOnJump)
            {
                jumpTimer = 0;
            }
            else
            {
                PlayerStateMachine.ChangeState(ControlerState.Jumping);
            }
        }
        else
        {
            if(bufferJumpCoroutine != null) StopCoroutine(bufferJumpCoroutine);
            bufferJumpCoroutine = StartCoroutine(BufferJump());
        }
    }

    void StopJumpInput()
    {
        if(IsGrounded()) return;
        canStopJump = true;
        if(jumpTimer < minJumpTime) return;
        isOnJump = false;
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
        yaw += cameraHorizontalInput * lookSensitivity;
        pitch -= cameraVerticalInput *  lookSensitivity;
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

    IEnumerator BufferJump()
    {
        float elapsedTime = 0;
        while (elapsedTime < bufferTime)
        {
            elapsedTime += Time.deltaTime;
            if(IsGrounded()) PlayerStateMachine.ChangeState(ControlerState.Jumping);
            yield return null;
        }
        
    }
   
}
