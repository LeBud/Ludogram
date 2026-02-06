using System;
using UnityEngine;

public abstract class Gadget : MonoBehaviour, IGadget
{
	[SerializeField] protected string gadgetName;
	[SerializeField] protected Sprite icon;
	[SerializeField] protected int    maxUses = 1;
	protected int currentUses;
    
	public string Name         => gadgetName;
	public Sprite Icon         => icon;
	public int    CurrentUses  => currentUses;
	public int    MaxUses      => maxUses;
	public bool   IsHandled    { get; }
	public bool   IsInfinite   => maxUses == -1;
	public bool   IsDepleted   => !IsInfinite && currentUses <= 0;

	protected virtual void Awake() => currentUses = maxUses;

	public void Use()
	{
		if (!CanUse()) return;
		OnUse();
		ConsumeUse();
	}

	
	protected abstract void OnUse();
	
	private void ConsumeUse()
	{
		if (IsInfinite) return;
        
		currentUses--;
        Debug.Log(Name + " used " + currentUses);
		if (currentUses <= 0) HandleDepletion();
	}
	
	private void HandleDepletion()
	{
		OnDepleted();
	}


	public virtual void Release()
	{
		//
	}

	public virtual bool CanUse()
	{
		return IsInfinite || currentUses > 0;
	}
	
	public virtual void OnPickup()
	{
		Debug.Log(this.name);
	}
	
	public virtual void Drop() 
	{
		Debug.Log(name + " is dropped");
	}
	
	public virtual void OnDepleted()
	{
		GadgetController.selectedGadget = null;
		Debug.Log(name + " is depleted");
	}

	private void OnDestroy()
	{
		if(GadgetController.selectedGadget == GetComponent<IGadget>())
		{
			GadgetController.selectedGadget = null;
		}
	}
}
