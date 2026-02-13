using System;
using UnityEngine;

public abstract class Gadget : MonoBehaviour, IGadget
{
	[SerializeField] protected string gadgetName;
	[SerializeField] protected Sprite icon;
	[SerializeField] protected int    maxUses = 1;
	[SerializeField] protected int    price = 1;
	protected                  int    currentUses;
    
	protected Collider col;
	
	public string Name        => gadgetName;
	public int  Price       => price;
	public int    CurrentUses => currentUses;
	public int    MaxUses     => maxUses;
	public bool   IsInfinite  => maxUses == -1;
	public bool   IsDepleted  => !IsInfinite && currentUses <= 0;

	[Header("Follow Player")]
	public Vector3 offset;
	public                  float   smoothTime;
	[HideInInspector] public Vector3 velref = Vector3.zero;

	public Transform target = null;
	public GadgetController gadgetController;

	protected virtual void Awake() {
		currentUses = maxUses;
		if (TryGetComponent(out Collider c)) col = c;
		else Debug.LogError(name + " has no collider");
	}

	public void Use()
	{
		if (!CanUse()) return;
		OnUse();
	}
	
	
	
	public virtual void IsTaken()
	{
		if (target == null) return;
		//transform.forward = target.forward;
		Vector3 targetPos = target.position + offset;
		transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velref, smoothTime);
		transform.rotation = Quaternion.LookRotation(target.forward);

	}


	protected abstract void OnUse();
	
	private void ConsumeUse()
	{
		if (IsInfinite) return;
        
		currentUses--;
//        Debug.Log(Name + " used " + currentUses);
		if (currentUses <= 0) HandleDepletion();
	}
	
	private void HandleDepletion()
	{
		OnDepleted();
	}


	public virtual void Release()
	{
		ConsumeUse();
	}

	public virtual bool CanUse()
	{
		return IsInfinite || currentUses > 0;
	}
	
	public virtual void OnPickup(GadgetController gadgetController)
	{
		col.enabled = false;
		Debug.Log(this.name);
	}
	
	public virtual void Drop()
	{
		col.enabled = true;
		target            = null;
	}
	
	public virtual void OnDepleted()
	{
		gadgetController.selectedGadget = null;
		Debug.Log(name + " is depleted");
	}

	private void OnDestroy() {
		if(gadgetController == null) return;
		
		if(gadgetController.selectedGadget == GetComponent<IGadget>()) {
			gadgetController.selectedGadget = null;
		}
	}
}
