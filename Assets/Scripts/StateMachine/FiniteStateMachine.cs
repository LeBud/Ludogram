using System;
using System.Collections.Generic;
using StateMachine.Finite_State_Machine_class;
using StateMachine.Finite_State_Machine_Interaces;
using UnityEngine;

public class FiniteStateMachine : MonoBehaviour
{
    StateNode                   currentState;
	Dictionary<Type, StateNode> nodes          = new Dictionary<Type, StateNode>();
	HashSet<ITransition>        anyTransitions = new HashSet<ITransition>();

	public void Update()
	{
		ITransition transitions = GetTransition();
		if(transitions != null) ChangeState(transitions.TargetState);
		
		currentState.State?.Update();
	}

	public void FixedUpdate()
	{
		currentState.State?.FixedUpdate();
	}

	public void LateUpdate()
	{
		currentState.State?.LateUpdate();
	}

	public void SetState(IState newState)
	{
		currentState = nodes[newState.GetType()];
		currentState.State?.OnEnter();
	}

	void ChangeState(IState state)
	{
		if(state == currentState.State) return;
		
		IState previousState = currentState.State;
		IState nextState = nodes[state.GetType()].State;
		
		previousState?.OnExit();
		nextState?.OnEnter();
		currentState = nodes[state.GetType()];
	}

	ITransition GetTransition()
	{
		foreach (ITransition transition in anyTransitions)
		{
			if(transition.Condition.Evaluate())
				return transition;
		}

		foreach (ITransition stateTransition in currentState.Transitions)
		{
			if(stateTransition.Condition.Evaluate())
				return stateTransition;
		}
		
		return null;
	}

	public void AddTransition(IState from, IState to, IPredicate condition)
	{
		GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
	}

	public void AddAnyTransition( IState to, IPredicate condition)
	{
		anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));

	}

	private StateNode GetOrAddNode(IState state)
	{
		StateNode node = nodes.GetValueOrDefault(state.GetType());

		if (node == null)
		{
			node = new StateNode(state);
			nodes.Add(state.GetType(), node);
		}
		return node;
	}

	class StateNode
	{
		public IState               State       { get; }
		public HashSet<ITransition> Transitions { get; }

		public StateNode(IState state)
		{
			State = state;
			Transitions = new HashSet<ITransition>();
		}

		public void AddTransition(IState nextState, IPredicate cond)
		{
			Transitions.Add(new Transition(nextState, cond));
		}
	}
}
