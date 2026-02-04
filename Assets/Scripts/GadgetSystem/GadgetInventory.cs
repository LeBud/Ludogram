using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GadgetInventory : MonoBehaviour
{
	public  int                 maxSlots = 5;
	private IGadget[]           gadgets;
	public List<string>         gadgetsNames = new();
	public  int                 currentSlot = 0;
	private IGadget             selectedGadget;
	private InputSystem_Actions playerActions;
	
	private Action<InputAction.CallbackContext> onNextGadgetAction;
	private Action<InputAction.CallbackContext> onPreviousGadgetAction;
	private Action<InputAction.CallbackContext> useGadgetAction;

	
	private void Awake()
	{
		gadgets = new IGadget[maxSlots];
	}
	
	public int MaxSlots 
	{ 
		get => maxSlots;
		set 
		{
			maxSlots = Mathf.Max(1, value);
			ResizeInventory();
		}
	}
    
	private void ResizeInventory()
	{
		var newGadgets = new IGadget[maxSlots];
		if (gadgets != null)
		{
			int copyCount = Mathf.Min(gadgets.Length, maxSlots);
			Array.Copy(gadgets, newGadgets, copyCount);
		}
		gadgets = newGadgets;
	}
	
	public bool AddGadget(IGadget gadget)
	{
		for (int i = 0; i < gadgets.Length; i++)
		{
			if (gadgets[i] == null)
			{
				gadgets[i] = gadget;
				gadgetsNames.Insert(i, gadgets[i].Name);
				selectedGadget = gadget;
				currentSlot = i;
				gadget.OnGadgetDepleted += OnGadgetDepleted;
				gadget.OnUsesChanged += OnGadgetUsesChanged;
				return true;
			}
		}
		return false; 
	}
    
	public void UseGadget()
	{
		if (selectedGadget == null) return; 
		
		if (selectedGadget.CanUse())
		{
			selectedGadget.Use();
		}
	}

	void SelectGadget(IGadget gadget)
	{
		if (selectedGadget == gadget) return;
		selectedGadget.Select();
	}

	public void ReplaceGadgetAt(int slotIndex, IGadget gadget)
	{
		if (slotIndex >= 0 && slotIndex < gadgets.Length)
		{
			gadgets[slotIndex] = null;
		}
	}
    
	public IGadget GetGadget(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= gadgets.Length) return null;
		return gadgets[slotIndex];
	}
    
	public bool IsSlotEmpty(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= gadgets.Length) return true;
		return gadgets[slotIndex] == null;
	}
	
	private void OnGadgetDepleted(Gadget gadget)
	{
		for (int i = 0; i < gadgets.Length; i++)
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
		currentSlot++;
		SelectGadget(GetGadget(currentSlot));
	}
	
	void PreviousGadget()
	{
		currentSlot--;
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
