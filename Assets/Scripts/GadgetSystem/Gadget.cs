using System;
using UnityEngine;

public abstract class Gadget : MonoBehaviour, IGadget
{
	[SerializeField] protected string gadgetName;
	[SerializeField] protected Sprite icon;
	[SerializeField] protected int    maxUses = 1; // -1 pour infini
    
	protected int currentUses;
    
	// Events pour notifier l'extérieur
	public event Action<Gadget> OnGadgetDepleted;
	public event Action<Gadget> OnUsesChanged;
    
	public string Name        => gadgetName;
	public Sprite Icon        => icon;
	public int    CurrentUses => currentUses;
	public int    MaxUses     => maxUses;
	public bool   IsInfinite  => maxUses == -1;
	public bool   IsDepleted  => !IsInfinite && currentUses <= 0;
	
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
		OnUsesChanged?.Invoke(this);
        
		if (currentUses <= 0) HandleDepletion();
	}
	
	private void HandleDepletion()
	{
		OnDepleted();
		OnGadgetDepleted?.Invoke(this);
	}
	
	protected virtual void OnDepleted()
	{
		Destroy(gameObject);
	}
	
	
	public virtual bool CanUse()
	{
		return IsInfinite || currentUses > 0;
	}
	
	public void OnPickup()
	{
		Debug.Log(this.name);
	}
	
	public virtual void Select()
	{
		gameObject.SetActive(true);
	}
}
