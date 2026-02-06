using System;
using System.Collections;
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
		private InputsBrain        pInput;

		[SerializeField] private Camera playerCamera;
		[SerializeField] private Rigidbody rb;
		[SerializeField] private Transform modelTransform;
		[SerializeField] private Transform groundRayPosition;
		[SerializeField] private Transform cameraPosition;
		[SerializeField] private LayerMask groundLayerMask;

		[Header("Move Settings")]
		[SerializeField] private float          groundCheckDistance    = 0.2f;
		[SerializeField] private float          groundSphereCastRadius = 0.5f;
		[SerializeField] public AnimationCurve gravityForceOverTime;
		[SerializeField] private AnimationCurve movementSpeedOverTime;
		public                   float          movementTime;
		
		[Header("Jump Settings")]
		[Tooltip("le temps max de jump est celui de la courbe")]
		public AnimationCurve jumpForceOverTime;
		[SerializeField]  private float minJumpTime;
		public  float jumpTime;
		[HideInInspector] public  float maxJumpTime;
		public  bool  canReleaseJump;
		public                    bool  isJumping;

		[SerializeField] private float decelerationTime;
		[SerializeField] private float lookSensitivity;
		[SerializeField] private float lookVerticalLimit;
		[SerializeField] private float cameraSpeed;
		
		Transform playerCameraTransform;

		
		
		private float yaw;
		private float pitch;
		
		
		public  bool  isGrounded;
		public float fallTime;
		
		
		private bool isStuned;
		
		public Vector3 velocity;
		
		private Vector2 movementInput;
		private Vector2 lookInput;
		
		
		private Action<InputAction.CallbackContext> onMove;
		private Action<InputAction.CallbackContext> onLook;

		private Action onJump;
		private Action stopJump;
		private Action stopMove;
		
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
			Any(stunState, new FuncPredicate(()=> isStuned));
			
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
		}
		
		private void ResetPlayerMovementInputs()
		{
			movementInput = Vector2.zero;
			movementTime  = 0;
			
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
			movementTime += Time.fixedDeltaTime;

			Quaternion targetRotation = Quaternion.Euler(0, yaw, 0);
			Vector3    forward        = targetRotation * Vector3.forward;
			Vector3    right          = targetRotation * Vector3.right;

			Vector3 move = (forward * movementInput.y + right * movementInput.x).normalized;
			Vector3 velocity        = move * movementSpeedOverTime.Evaluate(movementTime);

			if (!isGrounded && !isJumping)
			{
				fallTime   += Time.deltaTime;
				velocity.y =  -gravityForceOverTime.Evaluate(fallTime);
			}
			
			rb.linearVelocity      = velocity;
			modelTransform.forward = forward;
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
			pitch -= lookInput.y   * lookSensitivity;
			pitch =  Mathf.Clamp(pitch, -lookVerticalLimit, lookVerticalLimit);
			
			playerCameraTransform.position = cameraPosition.position;
			playerCameraTransform.rotation = Quaternion.Lerp(playerCameraTransform.rotation, Quaternion.Euler(pitch, yaw, 0), Time.deltaTime * cameraSpeed);
		}
		
		void HandleCar()
		{
			//Je sais pas trop;
		}
		
		#endregion
	
		#region BOOL

		bool StopJumpCheck()
		{
			return (!isJumping && canReleaseJump) || jumpTime >= maxJumpTime;
		}

		bool GoToMovementState()
		{
			return !isJumping && !isStuned;
		}

		#endregion
		

		void Update()
		{
			velocity = rb.linearVelocity;
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
		
		public InputsBrain GetInputs()
		{
			return pInput;
		}

		#endregion
		
		
	}
}