using CarScripts;
using Player;
using StateMachine.Finite_State_Machine_class;
using UnityEngine;

namespace StateMachine.BaseState_class
{
	public class CarState : BaseState
	{
		public CarState(Controller player, Animator animator) : base(player, animator) { }

		public override void OnEnter()
		{
			animator.CrossFade(sittingHash, crossFadeDuration, baseLayer);
			
			player.currentCar.BindInput(player.GetInputs(), player);
			
			player.UnbindLook();
			player.GetInputs().SetLookCar(true);
			player.RebindLook();
			
			player.GetInputs().DisablePlayerInput();
			player.GetInputs().EnableCarInput();
			
			player.transform.parent = player.currentCar.transform;
			player.playerCameraTransform.parent = player.currentCar.transform;
			player.DisableCollider();
			player.GetRB().isKinematic = true;
			player.GetRB().interpolation = RigidbodyInterpolation.None;
			player.transform.position = player.seat.GetPlayerPos().position;
		}
		
		public override void OnExit()
		{
			animator.CrossFade(locomotionHash, crossFadeDuration, baseLayer);
			
			player.UnbindLook();
			player.GetInputs().SetLookCar(false);
			player.RebindLook();
			
			player.GetInputs().DisableCarInput();
			player.GetInputs().EnablePlayerInput();
			
			player.transform.parent = player.originalParent;
			player.playerCameraTransform.parent = player.originalParent;
			player.transform.position = player.seat.GetExitPos().position;
			player.GetRB().isKinematic = false;
			// player.GetRB().interpolation = RigidbodyInterpolation.Interpolate;
			player.EnableCollider();
			
			player.seat.UnSeatDriver();
		}
	}
}