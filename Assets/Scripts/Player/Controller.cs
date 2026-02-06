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
	public class Controller : MonoBehaviour, IKnockable
	{
		private FiniteStateMachine stateMachine;
		private InputsBrain        pInput;

		public Camera playerCamera;
		[SerializeField] private Rigidbody rb;
		[SerializeField] private Transform modelTransform;
		[SerializeField] private Transform groundRayPosition;
		[SerializeField] private Transform cameraPosition;
		[SerializeField] private LayerMask groundLayerMask;

		[Header("Move Settings")]
		[SerializeField]  private float          groundCheckDistance    = 0.2f;
		[SerializeField]  public  AnimationCurve gravityForceOverTime;
		[SerializeField]  private AnimationCurve movementSpeedOverTime;
		[HideInInspector] public  float          movementTime;
		private                   Vector2        movementInput;
		private Vector3 groundNormal;
		
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
		
		[Header("Coyote Settings")] 
		[SerializeField] private float coyoteTime;
		[SerializeField] private float bufferTime;
		private                  float bufferTimer;
		public                   bool  havePressedJump;
		private                  bool  canCoyoteJump;
		
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

		
		public Transform       playerCameraTransform { get; private set; }
		private float   yaw;
		private float   pitch;
		private float   headbobTimer;
		private Vector3 headbobOffset;
		private Vector2 lookInput;

		public Transform originalParent { get; private set; }
		
		private bool  isknockedOut;
		private float knockOutTime;
		private float knockOutTimer;

		[HideInInspector] public bool          isDriving = false;
		[HideInInspector] public bool          isSeated = false;
		[HideInInspector] public CarController currentCar;
		public CarSeat seat { get; private set; }
		private ApplyVehiculePhysics vehiclePhysics;
		private Collider collider;
		
		private Action<InputAction.CallbackContext> onMove;
		private Action<InputAction.CallbackContext> onLook;

		private Action        onJump;
		private Action        stopJump;
		private Action        stopMove;
		private Action        leaveCar;

		public float debugFall;
		
		
		void Awake()
		{
			Initialize();
			SetupStateMachine();
		}

		private void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
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
				g.Initialize(this);
			}
			
			originalParent = transform.parent;
			
			if(TryGetComponent(out collider)) Debug.Log("Found collider");
			else Debug.LogError("Collider not found");
		}

		void SetupStateMachine()
		{
			stateMachine = new FiniteStateMachine();

			var movementState = new MovementState(this);
			var     jumpState     = new JumpState(this);
			var     stunState     = new StunState(this);
			var      carState      = new CarState(this);
			var seatedState = new SeatedState(this);
			
			At(movementState, jumpState, new FuncPredicate(() => isJumping || (havePressedJump && isGrounded)));
			At(jumpState, movementState, new FuncPredicate(() =>  StopJumpCheck()));
			
			Any(stunState, new FuncPredicate(()=> isknockedOut));
			At(stunState, movementState, new FuncPredicate(()=> !isknockedOut));
			
			Any(carState, new FuncPredicate(()=> isDriving));
			Any(seatedState, new FuncPredicate(()=> isSeated));
			
			At(carState, movementState, new FuncPredicate(()=> !isDriving));
			At(seatedState, movementState, new FuncPredicate(()=> !isSeated));
			
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
			pInput.LeaveCar.started += _ => leaveCar?.Invoke();
		}

		void AssignActions()
		{
			onMove   += PlayerMovementInputs;
			onLook   += CameraMovementsInputs;
			stopMove += ResetPlayerMovementInputs;
			onJump   += JumpInput;
			stopJump += StopJumpInput;
			leaveCar += LeaveCar;
		}

		void UnsubscribeInputSystemActions()
		{
			pInput.jump.started -= _ => onJump?.Invoke();

			pInput.move.performed -= onMove;
			pInput.look.performed -= onLook;

			pInput.move.canceled -= _ => stopMove?.Invoke();
			pInput.jump.canceled -= _ => onJump?.Invoke();
			pInput.LeaveCar.started -= _ => leaveCar?.Invoke();
		}

		public void UnbindLook() {
			pInput.look.performed -= onLook;
		}

		public void RebindLook() {
			pInput.look.performed += onLook;
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
			isJumping     = false;
			canCoyoteJump = false;
		}

		private void JumpInput()
		{
			if (fallTime != 0) havePressedJump = true;
			if (isGrounded || (fallTime < coyoteTime && canCoyoteJump))
			{
				isJumping     = true;
			}
		}

		private void LeaveCar() {
			if (isSeated) isSeated = false;
		}
		
		#endregion

		#region METHODS
		
		public void HandleMovement()
		{
			Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
			Vector3    forward        = targetRotation * Vector3.forward;
			Vector3    right          = targetRotation * Vector3.right;

			Vector3 move         = (forward * movementInput.y + right * movementInput.x).normalized;
			Vector3 orientedMove = Vector3.ProjectOnPlane(move, groundNormal);
			Vector3 horizontalVelocity;
			modelTransform.forward =  forward;
			
			if (movementInput.magnitude > 0.01f)
			{
				movementTime       += Time.fixedDeltaTime;
				horizontalVelocity =  orientedMove * movementSpeedOverTime.Evaluate(movementTime);
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
				if(havePressedJump) bufferTimer += Time.fixedDeltaTime;
				finalVelocity.y =  -gravityForceOverTime.Evaluate(fallTime);
				debugFall       =  -gravityForceOverTime.Evaluate(fallTime);
			}
			else
			{
				finalVelocity.y = rb.linearVelocity.y;
			}

			rb.linearVelocity = finalVelocity;
		}

		

		public void HandleJump()
		{
			Debug.Log("JUMP");
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
			isGrounded = Physics.Raycast(groundRayPosition.position, Vector3.down, out var hit, groundCheckDistance, groundLayerMask);
			
			if (isGrounded)
			{
				groundNormal =  hit.normal;
				if(bufferTimer > bufferTime) havePressedJump = false;
				bufferTimer   = 0;
				canCoyoteJump = true;
				fallTime      = 0;
				Keyframe[] keyframes = gravityForceOverTime.keys;
				keyframes[0].value               = 0;
				gravityForceOverTime.keys = keyframes;
				jumpTime                  = 0f;
			}
			else
			{
				groundNormal = Vector3.up;
			}
		}
		
		void HandleCamera() {
			var orientation = 0f;
			if (isSeated || isDriving) {
				orientation = transform.parent.eulerAngles.y;
			}
			
			//Test
			
			yaw   += lookInput.x * lookSensitivity;
		    pitch -= lookInput.y * lookSensitivity;
		    pitch =  Mathf.Clamp(pitch, -lookVerticalLimit, lookVerticalLimit);
		    
		    playerCameraTransform.position = cameraPosition.position;
		    playerCameraTransform.rotation = Quaternion.Lerp(
		        playerCameraTransform.rotation, 
		        Quaternion.Euler(pitch, orientation + yaw, 0), 
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
		
		public void SetCarController(CarController car, CarSeat carSeat)
		{
			currentCar = car;
			seat = carSeat;
		}

		public void EnableCollider() {
			collider.enabled = true;
		}
		
		public void DisableCollider() {
			collider.enabled = false;
		}
		
		public InputsBrain GetInputs()
		{
			return pInput;
		}

		public Rigidbody GetRB()
		{
			return rb;
		}

		#endregion

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawRay(groundRayPosition.position, Vector3.down * groundCheckDistance);
		}

		public void KnockOut(float time)
		{
			if (isknockedOut) return;
			isknockedOut  = true;
			knockOutTime  = 0;
			knockOutTimer = time;
		}

		public void HandleKnockTimer()
		{
			knockOutTime+= Time.deltaTime;
			if (knockOutTime >= knockOutTimer)
			{
				isknockedOut =  false;
			}
		}
	}
}