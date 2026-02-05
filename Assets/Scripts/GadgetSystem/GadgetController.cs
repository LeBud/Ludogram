using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GadgetController : MonoBehaviour
{
	//public static GadgetInventory     instance;
	[SerializeField] private Controller          player;
	public static            IGadget             selectedGadget;
	public                   GameObject          gadgetObject;
	private                  InputSystem_Actions playerActions;
	
	
	private Action<InputAction.CallbackContext> dropGadget;
	private Action<InputAction.CallbackContext> useGadgetAction;
	
	public bool AddGadget(IGadget gadget)
	{
		Debug.Log("Add Gadget");
		//if (selectedGadget == gadget) return false;
		if (selectedGadget != null)
		{
			selectedGadget.Drop();
			selectedGadget = null;
		}
		selectedGadget          =  gadget;
		return true; 
	}
    
	public void UseGadget()
	{
		if (selectedGadget == null) return; 
		
		if (selectedGadget.CanUse())
		{
			selectedGadget.Use();
//			Debug.Log($"{selectedGadget.Name} is used : {selectedGadget.CurrentUses}/{selectedGadget.MaxUses} usage restant");
		}
	}

	public void DropGadget()
	{
		selectedGadget?.Drop();
		selectedGadget = null;
		gadgetObject = null;
	}
	
	
	#region INPUT SYSTEM

	void OnEnable()
	{
		useGadgetAction += _ => UseGadget();
		dropGadget += _ => DropGadget();
		player.GetInputs().use.started += useGadgetAction;
		player.GetInputs().drop.started += dropGadget;
	}
	
	void OnDisable()
	{
		//TEMP input assignation
		player.GetInputs().use.started -= useGadgetAction;
		player.GetInputs().use.started -= dropGadget;
	}
	
	#endregion
	
}
