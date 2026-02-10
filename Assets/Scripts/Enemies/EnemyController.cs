using Enemies;
using EnemyStates;
using Manager;
using StateMachine.Finite_State_Machine_class;
using StateMachine.Finite_State_Machine_Interaces;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, IKnockable {
    private FiniteStateMachine stateMachine;
    private NavMeshAgent agent { get; set; }
    public Rigidbody rigidbody { get; private set; }
    public EnemyMovementController movement { get; private set; }
    public EnemyMoneyScan money { get; private set; }
    
    private bool enterKnockOut = false;
    public float knockOutTime { get; private set; }
    public Vector3 knockOutForce { get; private set; }

    public bool isKnockOut;
    
    private void Awake() {
        Initialize();
        SetupStateMachine();
    }

    private void Initialize() {
        if(TryGetComponent(out NavMeshAgent ag)) agent = ag;
        else Debug.LogError("No NavMeshAgent found");
        if(TryGetComponent(out Rigidbody rb)) rigidbody = rb;
        else Debug.LogError("No EnemyMovementController found");
        if(TryGetComponent(out EnemyMovementController move)) movement = move;
        else Debug.LogError("No EnemyMovementController found");
        if(TryGetComponent(out EnemyMoneyScan mo)) money = mo;
        else Debug.LogError("No EnemyMoneyScan found");
        
        movement.Initialize(agent);
        money.Initialize(this);
        
        //Set the rigidbody
        rigidbody.isKinematic = true;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
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
        At(waitState, pursuitState, new FuncPredicate(() => money.HasTargetBag));
        At(pursuitState, waitState, new FuncPredicate(() => !money.HasBag && !money.HasTargetBag));
        At(pursuitState, fleeState, new FuncPredicate(() => money.HasBag));
        
        At(knockOutState, waitState, new FuncPredicate(() => knockOutState.IsTimerFinished()));
        
        //Set Any State
        Any(knockOutState, new FuncPredicate(() => enterKnockOut));
        
        stateMachine.SetState(waitState);
    }
    
    void Update() {
        stateMachine.Update();
    }
		
    void FixedUpdate() {
        stateMachine.FixedUpdate();
    }

    void OnDestroy() {
        GameManager.instance.enemyManager.DeregisterEnemy(this);
    }
    
    void At(IState from, IState to, IPredicate condition)
    {
        stateMachine.AddTransition(from, to, condition);
    }

    void Any(IState to, IPredicate  condition)
    {
        stateMachine.AddAnyTransition(to, condition);
    }

    public void KnockOut(float time, Vector3 knockOutForce) {
        this.knockOutForce = knockOutForce;
        knockOutTime = time;
        enterKnockOut = true;
        Debug.Log($"KnockOut for {time} seconds");
    }
    
    public void EnableNavMesh() {
        agent.enabled = true;
    }

    public void DisableNavMesh() {
        agent.enabled = false;
    }
    
    public void UnKnockOut() {
        enterKnockOut = false;
    }
    
    [ContextMenu("Attack")]
    public void KnockOutPlayer()
    {
        Debug.Log("KnockOut");
        Collider[] col = Physics.OverlapSphere(transform.position, 5f);

        foreach (Collider coll in col)
        {
            // if (coll.TryGetComponent(out IKnockable knockable))
            // {
            //     knockable.KnockOut(2);
            // }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}