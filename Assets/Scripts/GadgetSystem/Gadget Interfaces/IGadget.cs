using System;
using UnityEngine;

public interface IGadget 
{
	string Name        { get; }
	int  Price       { get; }
	int    CurrentUses { get; }
	int    MaxUses     { get; }
	bool   IsInfinite  { get; }
	bool   IsDepleted  { get; }
    
	
	void Use();
	void IsTaken();
	void Drop();
	void Release();
	bool CanUse();

	
}
