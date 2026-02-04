using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GadgetInventory : MonoBehaviour
{
	//public static GadgetInventory     instance;
	[SerializeField] private Controller          player;
	public                  IGadget             selectedGadget;
	private                  InputSystem_Actions playerActions;
	
	private Action<InputAction.CallbackContext> onNextGadgetAction;
	private Action<InputAction.CallbackContext> onPreviousGadgetAction;
	private Action<InputAction.CallbackContext> useGadgetAction;
	
	public bool AddGadget(IGadget gadget)
	{
		Debug.Log("Add Gadget");
		if (selectedGadget != null) gadget.Drop();
		selectedGadget          =  gadget;
		return true; 
	}
    
	public void UseGadget()
	{
		if (selectedGadget == null) return; 
		
		if (selectedGadget.CanUse())
		{
			selectedGadget.Use();
			Debug.Log($"{selectedGadget.Name} is used : {selectedGadget.CurrentUses}/{selectedGadget.MaxUses} usage restant");
		}
	}

	public void DropGadget()
	{
		selectedGadget.Drop();
	}
	
	
	#region INPUT SYSTEM

	void OnEnable()
	{
		//TEMP input assignation
		useGadgetAction                 += _ => UseGadget();
		player.GetInputs().jump.started += useGadgetAction;
	}
	
	void OnDisable()
	{
		//TEMP input assignation
		player.GetInputs().jump.started -= useGadgetAction;
		playerActions.Disable();
	}
	
	#endregion
	
}
