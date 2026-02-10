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
	public                   IGadget             selectedGadget;
	public                   GameObject          gadgetObject;
	private                  InputSystem_Actions playerActions;
	public static            Camera              concernedPlayerCamera;
	
	
	
	private Action<InputAction.CallbackContext> dropGadget;
	private Action<InputAction.CallbackContext> useGadgetAction;
	private Action<InputAction.CallbackContext> releaseGadgetAction;
	
	
	public bool AddGadget(IGadget gadget)
	{
		//Debug.Log("Add Gadget");
		//if (selectedGadget == gadget) return false;
		if (selectedGadget != null)
		{
			return false;
		}
		selectedGadget          =  gadget;
		return true; 
	}

	void Update()
	{
		selectedGadget?.IsTaken();
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

	public void ReleaseGadget()
	{
		selectedGadget.Release();
	}
	
	public void DropGadget()
	{
		if (gadgetObject != null)
		{
			selectedGadget?.Drop();
			selectedGadget = null;
			gadgetObject   = null;	
		}
	}
	
	#region INPUT SYSTEM

	void Start()
	{
		concernedPlayerCamera            =  player.playerCamera;
		useGadgetAction                  += _ => UseGadget();
		dropGadget                       += _ => DropGadget();
		releaseGadgetAction              += _ => ReleaseGadget();
		player.GetInputs().use.started   += useGadgetAction;
		player.GetInputs().use.canceled  += releaseGadgetAction;
		player.GetInputs().drop.canceled += dropGadget;
	}
	
	void OnDisable()
	{
		player.GetInputs().use.started   -= useGadgetAction;
		player.GetInputs().use.canceled  -= releaseGadgetAction;
		player.GetInputs().drop.canceled -= dropGadget;
	}
	
	#endregion
	
}
