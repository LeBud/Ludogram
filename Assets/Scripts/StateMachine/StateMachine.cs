using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : Enum
{
	Dictionary<T, State<T>> states = new Dictionary<T, State<T>>();
	public State<T>         currentState;

	public void Add(State<T> state)
	{
		states.Add(state.iD, state);
	}

	public State<T> GetState(T stateID)
	{
		return states.TryGetValue(stateID, out State<T> state)? state : null;
	}

	public void ChangeState(T stateID)
	{
		State<T> state = GetState(stateID);
		if (state == null || currentState == state)
		{
			return;
		}
        
		currentState?.Exit();
		currentState = state;
		currentState?.Enter();
	}

	public void Update()
	{
		currentState?.Update();
	}

	public void FixedUpdate()
	{
		currentState?.FixedUpdate();
	}

	public void LateUpdate()
	{
		currentState?.LateUpdate();
	}
}
