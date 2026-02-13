using Player;
using StateMachine.Finite_State_Machine_Interaces;
using UnityEngine;

namespace StateMachine.Finite_State_Machine_class
{
	public class BaseState : IState
	{
		protected readonly Controller player;
		protected readonly Animator animator;
		
		protected static readonly int locomotionHash = Animator.StringToHash("Locomotion");
		protected static readonly int jumpHash = Animator.StringToHash("Player_Jump");
		protected static readonly int sittingHash = Animator.StringToHash("Player_Sitting");
		
		protected static readonly int grabb = Animator.StringToHash("Player_Grabbing");
		protected static readonly int idle = Animator.StringToHash("Player_Idle");

		protected const int baseLayer = 0;
		protected const int torsoLayer = 1;
		protected const float crossFadeDuration = 0.2f;
		
		protected BaseState(Controller player, Animator animator)
		{
			this.player = player;
			this.animator = animator;
		}
	
		public virtual void OnEnter() { }
		public virtual void Update() { }
		public virtual void FixedUpdate() {}
		public virtual void LateUpdate(){}
		public virtual void OnExit(){}
	}
}