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
    public EnemyAbilityController ability { get; private set; }
    
    private bool enterKnockOut = false;
    public float knockOutTime { get; private set; }
    public Vector3 knockOutForce { get; private set; }

    [HideInInspector]
    public bool isKnockOut;

    [SerializeField] private bool isInCar = false;

    [HideInInspector]
    public LineRenderer tongueRenderer;
    
    public Animator animator;
    public bool InCar { get; private set; }
    
    private void Awake() {
        Initialize();
        SetupStateMachine();
    }

    private void Initialize() {
        InCar = isInCar;
        
        if (!InCar) {
            if(TryGetComponent(out NavMeshAgent ag)) agent = ag;
            else Debug.LogError("No NavMeshAgent found");
            
            if(TryGetComponent(out Rigidbody rb)) rigidbody = rb;
            else Debug.LogError("No EnemyMovementController found");
            
            if(TryGetComponent(out EnemyMovementController move)) movement = move;
            else Debug.LogError("No EnemyMovementController found");
            
            movement.Initialize(agent, this);
            
            rigidbody.isKinematic = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        
        if(TryGetComponent(out EnemyMoneyScan mo)) money = mo;
        else Debug.LogError("No EnemyMoneyScan found");
        
        if(TryGetComponent(out EnemyAbilityController ab)) ability = ab;
        else Debug.LogError("No EnemyAbilityController found");
        
        if(TryGetComponent(out LineRenderer ln)) tongueRenderer = ln;
        else Debug.LogError("No LineRenderer found");
        
        money.Initialize(this);
        ability.Initialize(this);
        tongueRenderer.enabled = false;
    }

    private void Start() {
        GameManager.instance.enemyManager.RegisterEnemy(this);
    }

    private void SetupStateMachine() {
        stateMachine = new FiniteStateMachine();

        if (!InCar) {
            var waitState = new EnemyWaitState(this, animator);
            var pursuitState = new EnemyPursuitState(this, animator);
            var knockOutState = new EnemyKnockOutState(this, animator);
            var fleeState = new EnemyFleeState(this, animator);
            var abilityState = new EnemyAbilityState(this, animator);
            
            //Set At State
            At(waitState, pursuitState, new FuncPredicate(() => !money.HasBag &&  money.HasTargetBag));
            At(pursuitState, waitState, new FuncPredicate(() => !money.HasBag && !money.HasTargetBag));
            
            At(pursuitState, fleeState, new FuncPredicate(() => money.HasBag && !ability.triggerAbility && abilityState.IsStateFinished()));
            
            At(pursuitState, abilityState, new FuncPredicate(() => ability.triggerAbility));
            At(abilityState, pursuitState, new FuncPredicate(() => abilityState.IsStateFinished() && !ability.triggerAbility));
            
            At(knockOutState, waitState, new FuncPredicate(() => knockOutState.IsTimerFinished()));
            
            //Set Any State
            Any(knockOutState, new FuncPredicate(() => enterKnockOut));
            
            stateMachine.SetState(waitState);
            
            Debug.Log("Register Frog State");
        }
        else {
            var waitState     = new EnemyWaitState(this, animator);
            var abilityState  = new EnemyAbilityState(this, animator);
            var knockOutState = new EnemyKnockOutState(this, animator);
            
            //Set At State
            At(waitState, abilityState, new FuncPredicate(() => ability.triggerAbility));
            At(abilityState, waitState, new FuncPredicate(() => abilityState.IsStateFinished()));
            At(knockOutState, waitState, new FuncPredicate(() => knockOutState.IsTimerFinished()));
            
            //Set Any State
            Any(knockOutState, new FuncPredicate(() => enterKnockOut));
            
            stateMachine.SetState(waitState);
        }
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