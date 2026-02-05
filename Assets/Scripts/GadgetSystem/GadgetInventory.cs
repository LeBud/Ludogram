using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GadgetInventory : MonoBehaviour
{
	public static GadgetInventory     instance;
	public        int                 maxSlots = 5;
	public        List<IGadget>       gadgets;
	public        List<string>        gadgetsNames = new();
	public        int                 currentSlot  = 0;
	private       IGadget             selectedGadget;
	private       InputSystem_Actions playerActions;
	
	private Action<InputAction.CallbackContext> onNextGadgetAction;
	private Action<InputAction.CallbackContext> onPreviousGadgetAction;
	private Action<InputAction.CallbackContext> useGadgetAction;

	
	private void Awake()
	{
		gadgets      = new List<IGadget>();
		gadgetsNames = new List<string>();

		if (instance == null)
		{
			instance = this;
		}
	}
	
	public int MaxSlots 
	{ 
		get => maxSlots;
		set 
		{
			maxSlots = Mathf.Max(1, value);
		}
	}
    
	
	public bool AddGadget(IGadget gadget)
	{
		Debug.Log("Add");
		if (gadgets.Count < maxSlots)
		{
			gadgets.Add(gadget);
			gadgetsNames.Add(gadget.Name);
			selectedGadget          =  gadget;
			currentSlot             =  gadgets.Count;
			gadget.OnGadgetDepleted += OnGadgetDepleted;
			gadget.OnUsesChanged    += OnGadgetUsesChanged;
			return true;
		}
		return false; 
	}
    
	public void UseGadget()
	{
		if (selectedGadget == null) return; 
		
		if (selectedGadget.CanUse())
		{
			selectedGadget.Use();
			if (selectedGadget.IsLaunchable)
			{
				gadgets[currentSlot] = null;
				gadgetsNames.RemoveAt(currentSlot);
				
			}
		}
	}

	void SelectGadget(IGadget gadget)
	{
		if (selectedGadget == gadget) return;
		selectedGadget.Select();
	}
	
	void UnselectGadget(IGadget gadget)
	{
		if (selectedGadget == gadget) return;
		selectedGadget.Unselect();
	}

	public void ReplaceGadgetAt(int slotIndex, IGadget gadget)
	{
		if (slotIndex >= 0 && slotIndex < gadgets.Count)
		{
			gadgets[slotIndex] = null;
		}
	}
    
	public IGadget GetGadget(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= gadgets.Count) return null;
		return gadgets[slotIndex];
	}
    
	// public bool IsSlotEmpty(int slotIndex)
	// {
	// 	if (slotIndex < 0 || slotIndex >= gadgets.Count) return true;
	// 	return gadgets[slotIndex] == null;
	// }
	
	private void OnGadgetDepleted(Gadget gadget)
	{
		for (int i = 0; i < gadgets.Count; i++)
		{
			if (gadgets[i].Name == gadget.Name)
			{
				gadgets[i] = null;
				break;
			}
		}
		gadget.OnGadgetDepleted -= OnGadgetDepleted;
		gadget.OnUsesChanged -= OnGadgetUsesChanged;
	}
	
	private void OnGadgetUsesChanged(Gadget gadget)
	{
		Debug.Log($"{gadget.Name} : {gadget.CurrentUses}/{gadget.MaxUses}");
	}

	
	void NextGadget()
	{
		UnselectGadget(GetGadget(currentSlot));
		currentSlot++;
		if (currentSlot > maxSlots)
		{
			currentSlot = 0;
		}
		SelectGadget(GetGadget(currentSlot));
	}
	
	void PreviousGadget()
	{
		UnselectGadget(GetGadget(currentSlot));
		currentSlot--;
		if (currentSlot < 0)
		{
			currentSlot = maxSlots;
		}
		SelectGadget(GetGadget(currentSlot));
	}

	#region INPUT SYSTEM

	void OnEnable()
	{
		playerActions = new InputSystem_Actions();
		playerActions.Enable();
		AssignActionFunctions();
		SubscribeInputSystem();
	}
	
	void OnDisable()
	{
		UnsubscribeInputActions();
		playerActions.Disable();
	}
	
	private void AssignActionFunctions()
	{
		onNextGadgetAction += _ => NextGadget();
		onPreviousGadgetAction += _ => PreviousGadget();
		useGadgetAction += _ => UseGadget();
	}

	private void UnsubscribeInputActions()
	{
		playerActions.Player.NextGadget.started -= onNextGadgetAction;
		playerActions.Player.PreviousGadget.started -= onPreviousGadgetAction;
		playerActions.Player.UseGadget.started -= useGadgetAction;
	}

	private void SubscribeInputSystem()
	{
		playerActions.Player.NextGadget.started += onNextGadgetAction;
		playerActions.Player.PreviousGadget.started += onPreviousGadgetAction;
		playerActions.Player.UseGadget.started += useGadgetAction;
	}

	#endregion
	
}
