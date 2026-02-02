using System;
using JetBrains.Annotations;
using UnityEngine;

public class State<T>
{
    public T iD;

    public Action OnEnter;
    public Action OnExit;
    public Action OnUpdate;
    public Action OnFixedUpdate;
    public Action OnLateUpdate;

    public State(T iD, Action _onEnter = null, Action _onExit = null, Action _onUpdate = null, Action _onFixedUpdate = null, Action _onLateUpdate = null)
    {
        this.iD = iD;
        OnEnter = _onEnter;
        OnExit = _onExit;
        OnUpdate = _onUpdate;
        OnFixedUpdate = _onFixedUpdate;
        OnLateUpdate = _onLateUpdate;
    }

    public void Enter()
    {
        OnEnter?.Invoke();
    }

    public void Exit()
    { 
        OnExit?.Invoke();
    }

    public void FixedUpdate()
    {
        OnFixedUpdate?.Invoke();
    }

    public void Update()
    {
        OnUpdate?.Invoke();
    }

    public void LateUpdate()
    {
        OnLateUpdate?.Invoke();
    }
}
