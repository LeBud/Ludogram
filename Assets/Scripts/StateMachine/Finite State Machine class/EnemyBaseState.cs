using StateMachine.Finite_State_Machine_Interaces;
using UnityEngine;

namespace StateMachine.Finite_State_Machine_class {
    public class EnemyBaseState : IState {
        protected readonly EnemyController ia;
        protected readonly Animator animator;
        
        protected static readonly int enemyLocomotionHash = Animator.StringToHash("Locomotion");
        protected static readonly int enemySittingHash = Animator.StringToHash("Enemy_Sitting");
        protected static readonly int enemyWalkHash = Animator.StringToHash("Enemy_Walk");
        
        protected static readonly int enemyGrabTongueHash = Animator.StringToHash("Enemy_GrabTongue");
        protected static readonly int enemyGrabArmsHash    = Animator.StringToHash("Enemy_GrabArms");
        
        protected static readonly int enemyMouthClosedHash = Animator.StringToHash("Enemy_MouthClosed");
        protected static readonly int enemyOpenMouthHash   = Animator.StringToHash("Enemy_OpenMouth");

        protected const int   baseLayer         = 0;
        protected const int   torsoLayer        = 1;
        protected const int   mouthLayer             = 2;
        protected const float crossFadeDuration = 0.2f;

        protected EnemyBaseState(EnemyController ia, Animator animator) {
            this.ia = ia;
            this.animator = animator;
        }
	
        public virtual void OnEnter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() {}
        public virtual void LateUpdate(){}
        public virtual void OnExit(){}

    }
}