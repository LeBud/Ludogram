using System;
using Enemies;
using EnemyStates;
using Manager;
using StateMachine.Finite_State_Machine_class;
using StateMachine.Finite_State_Machine_Interaces;
using UnityEngine;
using UnityEngine.AI;

public class Robber : MonoBehaviour, IKnockable
{
    private FiniteStateMachine stateMachine;
    public NavMeshAgent agent { get; private set; }
    public EnemyMovementController movement { get; private set; }

    private bool knockOut = false;
    
    private void Awake() {
        Initialize();
        SetupStateMachine();
    }

    private void Initialize() {
        if(TryGetComponent(out NavMeshAgent agent)) this.agent = agent;
        else Debug.LogError("No NavMeshAgent found");
        if(TryGetComponent(out EnemyMovementController move)) movement = move;
        else Debug.LogError("No EnemyMovementController found");
        
        movement.Initialize(agent);
    }

    private void Start() {
        GameManager.instance.enemyManager.RegisterEnemy(this);
    }

    private void SetupStateMachine() {
        stateMachine = new FiniteStateMachine();

        var waitState = new EnemyWaitState(this);
        var pursuitState = new EnemyPursuitState(this);
        var knockOutState = new EnemyKnockOutState(this);
        var ambushState = new EnemyAmbushState(this);
        var fleeState = new EnemyFleeState(this);
        
        //Set At State
        At(waitState, pursuitState, new FuncPredicate(() => movement.playerInRange.Count > 0));
        At(pursuitState, waitState, new FuncPredicate(() => movement.playerInRange.Count == 0));
        
        At(knockOutState, pursuitState, new FuncPredicate(() => !knockOut));
        
        //Set Any State
        Any(knockOutState, new FuncPredicate(() => knockOut));
        
        stateMachine.SetState(waitState);
    }
    
    void Update() {
        stateMachine.Update();
    }
		
    void FixedUpdate() {
        stateMachine.FixedUpdate();
    }
    
    void At(IState from, IState to, IPredicate condition)
    {
        stateMachine.AddTransition(from, to, condition);
    }

    void Any(IState to, IPredicate  condition)
    {
        stateMachine.AddAnyTransition(to, condition);
    }

    public void KnockOut(float time)
    {
        Debug.Log($"KnockOut for {time} seconds");
    }

    [ContextMenu("Attack")]
    public void KnockOutPlayer()
    {
        Debug.Log("KnockOut");
        Collider[] col = Physics.OverlapSphere(transform.position, 5f);

        foreach (Collider coll in col)
        {
            if (coll.TryGetComponent(out IKnockable knockable))
            {
                knockable.KnockOut(2);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}