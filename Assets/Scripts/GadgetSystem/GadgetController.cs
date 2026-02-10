using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GadgetController : MonoBehaviour
{
	//public static GadgetInventory     instance;
	public        Controller          player;
	public        IGadget             selectedGadget;
	public        GameObject          gadgetObject;
	private       InputSystem_Actions playerActions;
	public static Camera              concernedPlayerCamera;
	public LayerMask buttonLayerMask;
	
	[HideInInspector] public bool       isInShop;
	[HideInInspector] public GadgetSeller gadgetSeller;
	[HideInInspector] public GameObject buttonToBuy;

	
	private Action<InputAction.CallbackContext> dropGadget;
	public  Action<InputAction.CallbackContext> useGadgetAction;
	public  Action<InputAction.CallbackContext> releaseGadgetAction;

	
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
		if(isInShop) BuyGadget();
		if (selectedGadget == null) return; 
		
		if (selectedGadget.CanUse())
		{
			selectedGadget.Use();
//			Debug.Log($"{selectedGadget.Name} is used : {selectedGadget.CurrentUses}/{selectedGadget.MaxUses} usage restant");
		}
	}

	public void ReleaseGadget()
	{
		if(!isInShop) selectedGadget.Release();
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

	public void BuyGadget()
	{
		Ray ray = new Ray(player.playerCameraTransform.position, player.playerCamera.transform.forward);
		if (Physics.Raycast(ray, out RaycastHit hit, 10, buttonLayerMask))
		{
			if (hit.transform.gameObject == buttonToBuy)
			{
				Debug.Log("BuyGadget");
				gadgetSeller.BuyGadget();
			}
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
		useGadgetAction                  -= _ => UseGadget();
		dropGadget                       -= _ => DropGadget();
		releaseGadgetAction              -= _ => ReleaseGadget();
		player.GetInputs().use.started   -= useGadgetAction;
		player.GetInputs().use.canceled  -= releaseGadgetAction;
		player.GetInputs().drop.canceled -= dropGadget;
	}
	
	#endregion
	
}
