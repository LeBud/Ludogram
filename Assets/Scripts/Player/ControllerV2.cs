using System;
using System.Collections;
using CarScripts;
using GadgetSystem;
using StateMachine.BaseState_class;
using StateMachine.Finite_State_Machine_class;
using StateMachine.Finite_State_Machine_Interaces;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Player
{
	public class ControllerV2 : MonoBehaviour
	{
		private FiniteStateMachine stateMachine;
		public InputsBrain        pInput;

		[SerializeField] private Camera playerCamera;
		[SerializeField] private Rigidbody rb;
		[SerializeField] private Transform modelTransform;
		[SerializeField] private Transform groundRayPosition;
		[SerializeField] private Transform cameraPosition;
		[SerializeField] private LayerMask groundLayerMask;

		[Header("Move Settings")]
		[SerializeField]  private float          groundCheckDistance    = 0.2f;
		[SerializeField]  private float          groundSphereCastRadius = 0.5f;
		[SerializeField]  public  AnimationCurve gravityForceOverTime;
		[SerializeField]  private AnimationCurve movementSpeedOverTime;
		[HideInInspector] public  float          movementTime;
		private                   Vector2        movementInput;
		
		[Header("Deceleration Settings")]
		[SerializeField] private float decelerationTime = 0.3f;
		private AnimationCurve decelerationCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
		private                  float          decelerationTimer;
		private                  bool           isDecelerating;
		private                  Vector3        lastMovementDirection; 
		private                  float          lastMovementSpeed;     

		
		[Header("Jump Settings")]
		public AnimationCurve jumpForceOverTime;
		[SerializeField]  private float minJumpTime;
		[HideInInspector] public  bool  canReleaseJump;
		[HideInInspector] public  bool  isJumping;
		[HideInInspector] public  float jumpTime;
		[HideInInspector] public  float maxJumpTime;
		[HideInInspector] public  float fallTime;
		[HideInInspector] public  bool  isGrounded;
		
		[Header("Camera Settings")]
		[SerializeField] private float lookSensitivity;
		[SerializeField] private float lookVerticalLimit;
		[SerializeField] private float cameraSpeed;
		
		[Header("Headbob Settings")]
		[SerializeField] private bool enableHeadbob = true;
		[SerializeField] private float headbobFrequency           = 2f;    
		[SerializeField] private float headbobHorizontalAmplitude = 0.05f; 
		[SerializeField] private float headbobVerticalAmplitude   = 0.08f; 
		[SerializeField] private float headbobSmoothness          = 10f;   

		
		Transform       playerCameraTransform;
		private float   yaw;
		private float   pitch;
		private float   headbobTimer;
		private Vector3 headbobOffset;
		private Vector2 lookInput;
		
		
		private bool isknockedOut;
		public  bool isInCar;
		
		private Action<InputAction.CallbackContext> onMove;
		private Action<InputAction.CallbackContext> onLook;

		private Action        onJump;
		private Action        stopJump;
		private Action        stopMove;
		public CarController currentCar;
		
		void Awake()
		{
			Initialize();
			SetupStateMachine();
		}

		private void Start()
		{
			playerCameraTransform = playerCamera.transform;
			AssignActions();
			SubscribeInputSystemActions();
			GetInputs().DisableCarInput();
		}

		#region AWAKE METHODS

		void Initialize()
		{
			if (TryGetComponent(out pInput))
			{
				pInput.Initialize();
				Debug.Log("Found PLAYER INPUT");
			}
			else Debug.LogError("PlayerInput not found");

			if (TryGetComponent(out GadgetPickup g))
			{
				Debug.Log("Found GADGET INPUT and reference this as Controller");
				//g.Initialize(this);
			}
		}

		void SetupStateMachine()
		{
			stateMachine = new FiniteStateMachine();

			MovementState movementState = new MovementState(this);
			JumpState     jumpState     = new JumpState(this);
			StunState     stunState     = new StunState(this);
			CarState      carState      = new CarState(this);
			
			At(movementState, jumpState, new FuncPredicate(() => isJumping && isGrounded));
			At(jumpState, movementState, new FuncPredicate(() =>  StopJumpCheck()));
			//Any(movementState, new FuncPredicate(GoToMovementState));
			Any(stunState, new FuncPredicate(()=> isknockedOut));
			Any(carState, new FuncPredicate(()=> isInCar));
			At(carState, movementState, new FuncPredicate(()=> !isInCar));
			
			stateMachine.SetState(movementState);
		}

		#endregion
		
		#region INPUT MANAGER & START METHODS

		private void OnDisable()
		{
			UnsubscribeInputSystemActions();
		}
		
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
		
		#region INPUTS METHODS
		
		private void PlayerMovementInputs(InputAction.CallbackContext ctx)
		{
			movementInput = ctx.ReadValue<Vector2>();
			if (movementInput.magnitude > 0.01f)
			{
				isDecelerating    = false;
				decelerationTimer = 0f;
			}
		}

		private void ResetPlayerMovementInputs()
		{
			if (!isDecelerating && rb.linearVelocity.magnitude > 0.1f)
			{
				Vector3 currentVel = rb.linearVelocity;
				lastMovementDirection = new Vector3(currentVel.x, 0, currentVel.z).normalized;
				lastMovementSpeed     = new Vector3(currentVel.x, 0, currentVel.z).magnitude;
				isDecelerating        = true;
				decelerationTimer     = 0f;
			}
    
			movementInput = Vector2.zero;
		}
		
		private void CameraMovementsInputs(InputAction.CallbackContext ctx)
		{
			lookInput = ctx.ReadValue<Vector2>();
		}

		
		private void StopJumpInput()
		{
			isJumping = false;
		}

		private void JumpInput()
		{
			if(isGrounded)isJumping = true;
		}
		
		#endregion

		#region METHODS
		
		public void HandleMovement()
		{
			Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
			Vector3    forward        = targetRotation * Vector3.forward;
			Vector3    right          = targetRotation * Vector3.right;

			Vector3 move = (forward * movementInput.y + right * movementInput.x).normalized;
			Vector3 horizontalVelocity;
			
			if (movementInput.magnitude > 0.01f)
			{
				movementTime           += Time.fixedDeltaTime;
				horizontalVelocity     =  move * movementSpeedOverTime.Evaluate(movementTime);
				modelTransform.forward =  forward;
			}
			else if (isDecelerating && decelerationTimer < decelerationTime)
			{
				decelerationTimer += Time.fixedDeltaTime;
				float normalizedTime = decelerationTimer / decelerationTime;
				float curveValue     = decelerationCurve.Evaluate(normalizedTime);
				
				horizontalVelocity = lastMovementDirection * (lastMovementSpeed * curveValue);
				
				movementTime = Mathf.Lerp(movementTime, 0f, normalizedTime);
			}
			else
			{
				horizontalVelocity = Vector3.zero;
				movementTime       = 0f;
				isDecelerating     = false;
			}
			
			Vector3 finalVelocity = horizontalVelocity;
    
			if (!isGrounded && !isJumping)
			{
				fallTime        += Time.fixedDeltaTime;
				finalVelocity.y =  -gravityForceOverTime.Evaluate(fallTime);
			}
			else
			{
				finalVelocity.y = rb.linearVelocity.y;
			}

			rb.linearVelocity = finalVelocity;
		}

		

		public void HandleJump()
		{
			Debug.Log(maxJumpTime);
			if (jumpTime >= minJumpTime)
			{
				canReleaseJump = true;
			}
			
			if (jumpTime < maxJumpTime)
			{
				Vector3 jumpVel = rb.linearVelocity;
				jumpVel.y         =  jumpForceOverTime.Evaluate(jumpTime);;
				rb.linearVelocity =  jumpVel;
				jumpTime          += Time.fixedDeltaTime;
			}
		}

		public void CheckGround()
		{
			isGrounded = Physics.Raycast(groundRayPosition.position, Vector3.down, groundCheckDistance, groundLayerMask);
			if (isGrounded)
			{
				fallTime = 0;
			}
		}
		
		void HandleCamera()
		{
		    yaw   += lookInput.x * lookSensitivity;
		    pitch -= lookInput.y * lookSensitivity;
		    pitch =  Mathf.Clamp(pitch, -lookVerticalLimit, lookVerticalLimit);
		    
		    playerCameraTransform.position = cameraPosition.position;
		    playerCameraTransform.rotation = Quaternion.Lerp(
		        playerCameraTransform.rotation, 
		        Quaternion.Euler(pitch, yaw, 0), 
		        Time.deltaTime * cameraSpeed
		    );
		}

		public void HandleHeadbob()
		{
		    Vector3 targetPosition = playerCameraTransform.position;
		    
		    if (enableHeadbob && isGrounded && movementInput.magnitude > 0.01f && !isJumping)
		    {
		        float currentSpeed    = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
		        float speedMultiplier = Mathf.Clamp01(currentSpeed / movementSpeedOverTime.Evaluate(5f));
		        
		        headbobTimer += Time.deltaTime * headbobFrequency * speedMultiplier;
		        
		        float horizontalOffset = Mathf.Sin(headbobTimer) * headbobHorizontalAmplitude * speedMultiplier;
		        float verticalOffset = Mathf.Abs(Mathf.Sin(headbobTimer * 2f)) * headbobVerticalAmplitude * speedMultiplier;
		        
		        Vector3 targetHeadbobOffset = new Vector3(horizontalOffset, verticalOffset, 0);
		        headbobOffset = Vector3.Lerp(headbobOffset, targetHeadbobOffset, Time.deltaTime * headbobSmoothness);
		        
		        targetPosition += playerCameraTransform.right * headbobOffset.x;
		        targetPosition += playerCameraTransform.up * headbobOffset.y;
		    }
		    else
		    {
		        headbobOffset = Vector3.Lerp(headbobOffset, Vector3.zero, Time.deltaTime * headbobSmoothness);
		        
		        targetPosition += playerCameraTransform.right * headbobOffset.x;
		        targetPosition += playerCameraTransform.up * headbobOffset.y;
		        
		        headbobTimer = Mathf.Lerp(headbobTimer, 0f, Time.deltaTime * headbobSmoothness);
		    }
		    
		    playerCameraTransform.position = targetPosition;
		}

		public void ResetHeadbob()
		{
		    headbobTimer = 0f;
		    headbobOffset = Vector3.zero;
		}
		
		void HandleCar()
		{
			//Je sais po;
		}
		
		#endregion
	
		#region BOOL

		bool StopJumpCheck()
		{
			return (!isJumping && canReleaseJump) || jumpTime >= maxJumpTime;
		}

		bool GoToMovementState()
		{
			return !isJumping && !isknockedOut;
		}

		#endregion
		
		void Update()
		{
			stateMachine.Update();
		}
		
		void FixedUpdate()
		{
			stateMachine.FixedUpdate();
		}
		
		void LateUpdate()
		{
			HandleCamera();
			
			stateMachine.LateUpdate();
		}
		
		
		#region HELPERS

		void At(IState from, IState to, IPredicate condition)
		{
			stateMachine.AddTransition(from, to, condition);
		}

		void Any(IState to, IPredicate  condition)
		{
			stateMachine.AddAnyTransition(to, condition);
		}
		
		public void SetCarController(CarController car)
		{
			currentCar = car;
		}
		
		public InputsBrain GetInputs()
		{
			return pInput;
		}

		#endregion
		
		
	}
}