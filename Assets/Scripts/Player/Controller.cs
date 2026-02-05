using System;
using System.Collections;
using CarScripts;
using GadgetSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class Controller : MonoBehaviour
    {
        //InputSystem_Actions playerInputActions;
        private InputsBrain pInput;

        [Header("General Settings")] [SerializeField]
        TMP_Text currentStateTxt;

        [SerializeField]        Rigidbody rb;
        [SerializeField] public Camera    playerCamera;
        [SerializeField]        Transform playerTransform;
        [SerializeField]        Transform cameraTarget;
        [SerializeField]        Transform groundRayPosition;
        [SerializeField]        Transform footRayPosition;
        [SerializeField]        LayerMask groundLayerMask;

        [Header("Movement State Settings")] [SerializeField]
        AnimationCurve movementSpeedCurve;

        [SerializeField] AnimationCurve airMovementSpeedCurve;
        [SerializeField] AnimationCurve decelerationSpeedCurve;

        [Header("Fall State Settings")] [SerializeField]
        AnimationCurve fallSpeedCurve;

        [Header("Jump State Settings")] [SerializeField]
        AnimationCurve jumpSpeedCurve;

        [SerializeField] float minJumpTime = 0.2f;
        [SerializeField] float maxJumpTime = 0.5f;
        [SerializeField] int   maxJumpNumber;
        [SerializeField] float coyoteTime;
        [SerializeField] float bufferTime;

        [Header("Camera Settings")] [SerializeField]
        float cameraSpeed;

        [SerializeField] float lookSensitivity = 2f;
        [SerializeField] float verticalLimit   = 80f;

        [Header("Head Bob Settings")] [SerializeField]
        float headBobSmoothSpeed;

        [SerializeField] AnimationCurve headBobFrequencyCurve;
        [SerializeField] AnimationCurve headBobAmplitudeCurve;


        private CarController currentCar;

        public float headBobAmmount;
        public float headBobFrequency;


        public bool  isGrounded;
        public float velocityDebug;

        Transform cameraTransform;

        //MOVEMENTS
        float movementTimer;
        float stopTimer;
        public float horizontalInput;
        public float verticalInput;
        public bool  isMovementActive;

        //JUMP
        public int         currentJumpNumber;
        public float       jumpTimer;
        public bool isOnJump;
        public bool        canStopJump;

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

        private Coroutine bufferJumpCoroutine;

        [Header("State Machine")] public StateMachine<ControlerState> PlayerStateMachine = new();

        public enum ControlerState
        {
            Idle,
            Moving,
            Falling,
            Jumping,
            Driving
        }

        private void Awake()
        {
            if (TryGetComponent(out pInput))
            {
                pInput.Initialize();
                Debug.Log("Found PLAYER INPUT");
            }
            else Debug.LogError("PlayerInput not found");

            if (TryGetComponent(out GadgetPickup g))
            {
                g.Initialize(this);
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            cameraTransform  = playerCamera.transform;
            rb.constraints   = RigidbodyConstraints.FreezeRotation;

            CreateState();
            AssignActions();
            SubscribeInputSystemActions();

            GetInputs().DisableCarInput();
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
            UnsubscribeInputSystemActions();
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
                _onUpdate: JumpUpdate,
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

            PlayerStateMachine.Add(new State<ControlerState>(
                ControlerState.Driving,
                _onEnter: EnterDriving,
                _onUpdate: UpdateDriving,
                _onExit: ExitDriving
            ));

            PlayerStateMachine.ChangeState(ControlerState.Idle);
        }

        #region STATE-MACHINE

        #region FUNCTION CALL

        void Update()
        {
            PlayerStateMachine?.Update();
            DebugStateMachine();
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
            if (isMovementActive) PlayerStateMachine.ChangeState(ControlerState.Moving);
            //movementTimer = 0;
        }

        void IdleUpdate()
        {
            if (!IsGrounded() && !isOnJump)
            {
                PlayerStateMachine.ChangeState(ControlerState.Falling);
            }
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
            Vector3    forward        = targetRotation * Vector3.forward;
            Vector3    right          = targetRotation * Vector3.right;

            Vector3 move     = (forward * verticalInput + right * horizontalInput).normalized;
            Vector3 velocity = move * decelerationSpeedCurve.Evaluate(stopTimer);
            velocity.y = rb.linearVelocity.y;

            playerTransform.forward = forward;
            rb.linearVelocity       = velocity;
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
            //horizontalInput = 0;
            //verticalInput = 0;
        }

        #endregion

        #region MOVING

        void MoveEnter()
        {
            stopTimer = 0;
        }

        void MoveUpdate()
        {
            CheckFoot();
            if (!IsGrounded())
            {
                PlayerStateMachine?.ChangeState(ControlerState.Falling);
            }
        }

        void MoveFixedUpdate()
        {
            movementTimer += Time.fixedDeltaTime;

            Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
            Vector3    forward        = targetRotation * Vector3.forward;
            Vector3    right          = targetRotation * Vector3.right;

            Vector3 move     = (forward * verticalInput + right * horizontalInput).normalized;
            Vector3 velocity = move * movementSpeedCurve.Evaluate(movementTimer);
            velocity.y = rb.linearVelocity.y;

            playerTransform.forward = forward;
            rb.linearVelocity       = velocity;
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
            keyframes[0].value  = -jumpSpeedCurve.Evaluate(jumpTimer);
            fallSpeedCurve.keys = keyframes;
        }

        void FallUpdate()
        {
            if (IsGrounded())
            {
                isOnJump          = false;
                currentJumpNumber = 0;
                PlayerStateMachine.ChangeState(ControlerState.Idle);
            }

            CheckFoot();
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
            velocity.y += -fallSpeedCurve.Evaluate(fallTimer);

            playerTransform.forward = forward;
            rb.linearVelocity       = velocity;
        }

        void FallLateUpdate()
        {
            CameraMovement();
        }

        #endregion

        #region JUMPING

        void JumpEnter()
        {
            if (bufferJumpCoroutine != null) StopCoroutine(bufferJumpCoroutine);
            if (!isMovementActive)
            {
                horizontalInput = 0;
                verticalInput   = 0;
            }

            isOnJump    = true;
            canStopJump = false;
            jumpTimer   = 0;
            fallTimer   = 0;
        }

        void JumpUpdate()
        {
            CheckFoot();
        }

        void JumpFixedUpdate()
        {
            jumpTimer += Time.fixedDeltaTime;
            
            if (!isMovementActive)
            {
                horizontalInput = 0;
                verticalInput   = 0;
            }
            else
            {
                movementTimer += Time.fixedDeltaTime;
            }
           


            Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
            Vector3    forward        = targetRotation * Vector3.forward;
            Vector3    right          = targetRotation * Vector3.right;

            Vector3 move     = (forward * verticalInput + right * horizontalInput).normalized;
            Vector3 velocity = move * airMovementSpeedCurve.Evaluate(movementTimer);

            velocity.y += jumpSpeedCurve.Evaluate(jumpTimer);

            playerTransform.forward = forward;
            rb.linearVelocity       = velocity;

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

        #region Driving

        private void EnterDriving()
        {
            //Bind les inputs au véhicules
            currentCar.BindInput(pInput, this);
            GetInputs().DisablePlayerInput();
            GetInputs().EnableCarInput();
        }

        private void UpdateDriving()
        {
            CameraMovement();
        }

        private void ExitDriving()
        {
            //Unbind
            GetInputs().DisableCarInput();
            GetInputs().EnablePlayerInput();
        }

        #endregion

        #endregion

        #region INPUT SYSTEM SETUP

        void SubscribeInputSystemActions()
        {
            pInput.jump.started += _ => onJump?.Invoke();

            pInput.move.performed += onMove;
            pInput.look.performed += onLook;

            pInput.move.canceled += _ => stopMove?.Invoke();
            pInput.jump.canceled += _ => stopJump?.Invoke();
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
            pInput.jump.started -= _ => onJump?.Invoke();

            pInput.move.performed -= onMove;
            pInput.look.performed -= onLook;

            pInput.move.canceled -= _ => stopMove?.Invoke();
            pInput.jump.canceled -= _ => onJump?.Invoke();
        }

        #endregion

        #region INPUT FUNCTIONS

        void PlayerMovementInputs(InputAction.CallbackContext context)
        {
            if (IsGrounded() && !isOnJump) PlayerStateMachine.ChangeState(ControlerState.Moving);
            horizontalInput  = context.ReadValue<Vector2>().x;
            verticalInput    = context.ReadValue<Vector2>().y;
            isMovementActive = true;
        }

        void ResetPlayerMovementInputs()
        {
            isMovementActive = false;
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
            if (IsGrounded() || currentJumpNumber < maxJumpNumber ||
                (fallTimer < coyoteTime && currentJumpNumber == 0))
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
                if (bufferJumpCoroutine != null) StopCoroutine(bufferJumpCoroutine);
                bufferJumpCoroutine = StartCoroutine(BufferJump());
            }
        }

        void StopJumpInput()
        {
            if (IsGrounded()) return;
            canStopJump = true;
            if (jumpTimer < minJumpTime) return;
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
            yaw              += cameraHorizontalInput * lookSensitivity;
            pitch            -= cameraVerticalInput   * lookSensitivity;
            pitch            =  Mathf.Clamp(pitch, -verticalLimit, verticalLimit);
            headBobAmmount   =  headBobAmplitudeCurve.Evaluate(rb.linearVelocity.magnitude);
            headBobFrequency =  headBobFrequencyCurve.Evaluate(rb.linearVelocity.magnitude);
            HeadBobMovement();
            cameraTransform.position = cameraTarget.position;
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, Quaternion.Euler(pitch, yaw, 0),
                Time.deltaTime * cameraSpeed);
        }

        #endregion

        #region GROUND

        bool IsGrounded()
        {
            Ray ray = new Ray(groundRayPosition.position, -groundRayPosition.up);
            return Physics.SphereCast(ray, 0.45f, out RaycastHit ground, 0.15f, groundLayerMask);
        }

        void CheckFoot()
        {
            Ray ray = new Ray(footRayPosition.position, playerTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit ground, 0.5f, groundLayerMask))
            {
                rb.AddForce(transform.up * 2, ForceMode.Impulse);
            }
        }

        #endregion

        void DebugStateMachine()
        {
            if (currentStateTxt)
                currentStateTxt.text = PlayerStateMachine.currentState.iD.ToString();
            isGrounded = IsGrounded();
            Debug.DrawRay(groundRayPosition.position, -groundRayPosition.up * 0.5f, Color.red);
            Debug.DrawRay(footRayPosition.position, playerTransform.forward * 0.5f, Color.red);
            velocityDebug = rb.linearVelocity.magnitude;
        }

        IEnumerator BufferJump()
        {
            float elapsedTime = 0;
            while (elapsedTime < bufferTime)
            {
                elapsedTime += Time.deltaTime;
                if (IsGrounded()) PlayerStateMachine.ChangeState(ControlerState.Jumping);
                yield return null;
            }
        }


        void HeadBobMovement()
        {
            Vector3 pos = Vector3.zero;
            pos.x = Mathf.Lerp(pos.x, Mathf.Sin(Time.time * headBobFrequency) * headBobAmmount * 1.4f,
                Time.deltaTime                                                * headBobSmoothSpeed);
            pos.y = Mathf.Lerp(pos.y, Mathf.Sin(Time.time * headBobFrequency / 2) * headBobAmmount * 1.6f,
                Time.deltaTime                                                    * headBobSmoothSpeed);
            pos.z                      =  0;
            cameraTarget.localPosition += pos;
        }

        public void SetCarController(CarController car)
        {
            currentCar = car;
        }

        public InputsBrain GetInputs()
        {
            return pInput;
        }
    }
}