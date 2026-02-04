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
    
	
	void Use();
	void Drop();
	void OnDepleted();
	bool CanUse();

}
