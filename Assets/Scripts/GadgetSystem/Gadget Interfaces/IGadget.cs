using System;
using UnityEngine;

public interface IGadget 
{
	string Name        { get; }
	Sprite Icon        { get; }
	int    CurrentUses { get; }
	int    MaxUses     { get; }
	bool   IsInfinite  { get; }
	bool   IsDepleted  { get; }
	bool   IsLaunchable { get; }
    
	//effect when use
	void Use();
	//
	void Drop();
	void Select();
	void Unselect();
	bool CanUse();
    
	event Action<Gadget> OnGadgetDepleted;
	event Action<Gadget> OnUsesChanged;

}
