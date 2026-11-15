using UnityEngine;

public class BoidSteering : Agent
{
    [Header ("Stats")]
    
    [SerializeField] private float maxSteering;
    [SerializeField] private float slowingDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private float detectionRadius;
    [SerializeField] private float baitDetectionRadius = 8f;
    [SerializeField] private float respawnDelay = 5f;
    [SerializeField] private float consumeInterval = 1f;
    
    public float ConsumeInterval => consumeInterval;
    public float RespawnDelay => respawnDelay;

    [SerializeField] private GoatPen goatPen;    

    [SerializeField] private BoidSensor boidSensor;
    [SerializeField] private BoidFlocking boidFlocking;
    [SerializeField] private BoidVisual boidVisual;
    [SerializeField] private BoidMovement boidMovement;
    private BoidSpawner _boidSpawner;

    [SerializeField] private float evadeExitMultiplier = 1.3f;
    [SerializeField] private float evadeSteeringMultiplier = 3f;
    StateMachine _stateMachine;
    public int BoundaryCrossings { get; private set; }
    public bool IsRespawning { get; private set; }
    public BoidHealth Health { get; private set; }
    public bool IsDown => Health != null && Health.IsDown;

    private void Awake()
    {
        InitializeComponents();       
    }
    private void InitializeComponents()
    {
        Health = GetComponent<BoidHealth>();
        if (boidSensor == null)
        boidSensor = GetComponent<BoidSensor>();
        if (boidFlocking == null)
            boidFlocking = GetComponent<BoidFlocking>();
        if (boidVisual == null)
            boidVisual = GetComponent<BoidVisual>();
        if (boidMovement == null)
            boidMovement = GetComponent<BoidMovement>();
    }
    private void Start()
    {
        ResetMovement();
        InitializeStateMachine();
    }
    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine();
      
        _stateMachine.RegisterState(BoidStateType.Flocking, new BoidFlockingState(_stateMachine, this));
        _stateMachine.RegisterState(BoidStateType.Evade, new BoidEvadeState(_stateMachine, this));
        _stateMachine.RegisterState(BoidStateType.Arrive, new BoidArriveState(_stateMachine, this));
        _stateMachine.RegisterState(BoidStateType.Consume, new BoidConsumeState(_stateMachine, this));
        _stateMachine.RegisterState(BoidStateType.Dead, new BoidDeadState(_stateMachine, this));
        _stateMachine.RegisterState(BoidStateType.Respawn, new BoidRespawnState(_stateMachine, this));

        _stateMachine.ChangeState(BoidStateType.Flocking);
    }
    void Update()
    {

       _stateMachine.Update();

    }
    public bool IsThreatDetected(bool alreadyEvading)
    {
        float radius = alreadyEvading ? detectionRadius * evadeExitMultiplier : detectionRadius;

        return boidSensor.IsTargetWithinRange(radius);
    }
    public bool TryGetBait(out Transform bait)
    {
        return boidSensor.TryGetClosestBait(baitDetectionRadius,detectionRadius * evadeExitMultiplier,out bait);
    }
    public Vector3 EvadeSteering()
    {
        Agent target = boidSensor.TargetAgent;

        if (target == null)
            return Vector3.zero;

        return boidMovement.Evade(target,_velocity,maxSpeed,maxSteering) * evadeSteeringMultiplier;
    }
    public Vector3 ArriveSteering(Transform bait)
    {
        return boidMovement.Arrive(bait.position,_velocity,maxSpeed,maxSteering,minDistance,slowingDistance);
    }
    public bool IsAtBait(Transform bait)
    {
        return Vector3.Distance(transform.position, bait.position) <= minDistance;
    }
    public void EatBait(Transform bait)
    {
        if (bait == null) return;

        Bait component = bait.GetComponent<Bait>();
        if (component != null) component.TakeDamage(1);
    }
    public void Sleep()
    {
        StopMovement();

        if (Health != null)
            Health.ForceDown();
    }
    public void StopMovement()
    {
        _velocity = boidMovement.StopMovement();
    }
    public void ResetMovement()
    {
        _velocity = boidMovement.ResetMovement(maxSpeed);
    }
    public void Respawn()
    {
        if (Health == null)
            Health = GetComponent<BoidHealth>();

        if (_boidSpawner != null)
        {
            Transform spawnPoint = _boidSpawner.GetRandomSpawnPoint();

            if (spawnPoint != null)
                transform.position = spawnPoint.position;
        }

        if (Health != null)
            Health.ResetHealth();

        ResetMovement();
        IsRespawning = false;
    }
    public void Move(Vector3 steering)
    {
        _velocity = boidMovement.UpdateMovement(_velocity, steering, maxSpeed);

        boidMovement.UpdateRotation(_velocity);

        if (boidMovement.CheckBoundaries(goatPen))
            BoundaryCrossings++;
    }
    
    public void SetGoatPen(GoatPen pen)
    {
        goatPen = pen;
    }

    public void SetBoidSpawner(BoidSpawner spawner)
    {
        _boidSpawner = spawner;
    }

    public void StartRespawn()
    {
        if (_stateMachine == null || IsRespawning || !IsDown)
            return;

        IsRespawning = true;
    }
    public Vector3 Flocking()
    {
        return boidFlocking.CalculateFlocking();
    }
    public Vector3 CalculateSteering(Vector3 desired)
    {
        return boidMovement.CalculateSteering(desired,_velocity,maxSteering);
    }
    public Vector3 SeekForFlocking(Vector3 target)
    {
        return boidMovement.Seek(target,_velocity,maxSpeed,maxSteering);
    }


    public void SetVisible(bool visible)
    {
        boidVisual.SetVisible(visible);
    }
    public void UpdateStateText(string stateName)
    {
        boidVisual.UpdateStateText(stateName);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color= Color.black;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color= Color.white;
        Gizmos.DrawWireSphere(transform.position, baitDetectionRadius);
    }
}