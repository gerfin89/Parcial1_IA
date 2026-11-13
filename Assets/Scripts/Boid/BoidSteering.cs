using UnityEngine;
using System.Collections.Generic;
using TMPro;


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
    private Agent _targetAgent;
    public float ConsumeInterval => consumeInterval;
    public float RespawnDelay => respawnDelay;

    [Header("UI Estado Boid")]
    [SerializeField] private TextMeshPro stateText;

    [SerializeField] private float separationRadius;
    [SerializeField] private float alignmentRadius;
    [SerializeField] private float cohesionRadius;

    [SerializeField] private GoatPen goatPen;


    [SerializeField, Range(0f, 3f)] private float separationWeight = 1f;
    [SerializeField, Range(0f, 3f)] private float alignmentWeight = 1f;
    [SerializeField, Range(0f, 3f)] private float cohesionWeight = 1f;

    private BoidHealth _health;
    private Renderer[] _renderers;
    private Canvas[] _canvases;
    private List<BoidSteering> _nearbyBoids = new List<BoidSteering>();
    private List<Bait> _nearbyBaits = new List<Bait>();
    private BoidSpawner _boidSpawner;



    [SerializeField] private float evadeExitMultiplier = 1.3f;
    [SerializeField] private float evadeSteeringMultiplier = 3f;
    StateMachine _stateMachine;
    public int BoundaryCrossings { get; private set; }
    public bool IsRespawning { get; private set; }
    public BoidHealth Health { get; private set; }
    public bool IsDown => _health !=null && _health.IsDown;

    private void Awake()
    {
        Health = GetComponent<BoidHealth>();
        _health = Health;
        _renderers = GetComponentsInChildren<Renderer>(true);
        _canvases = GetComponentsInChildren<Canvas>(true);
    }
    private void Start()
    {
               
       
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        _velocity += randomDirection.normalized * maxSpeed;

        _stateMachine = new StateMachine();
        _stateMachine.RegisterState(BoidStateType.Flocking, new BoidFlockingState (_stateMachine, this));
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

    

    public bool IsThreatDetected (bool alreadyEvading)
    {
        if(_targetAgent == null) return false;

        float radius = alreadyEvading ? detectionRadius * evadeExitMultiplier : detectionRadius;
        return Vector3.Distance(transform.position, _targetAgent.transform.position) <= radius;
    }

    public bool TryGetBait (out Transform bait)
    {
        _nearbyBaits.RemoveAll(b => b == null);
        float minDist = Mathf.Infinity;
        bait = null;

        foreach (Bait candidate in _nearbyBaits)
        {
            if (_targetAgent != null)
            {
                float baitToHunter = Vector3.Distance(candidate.transform.position, _targetAgent.transform.position);
                if (baitToHunter <= detectionRadius * evadeExitMultiplier) continue;
            }

            float distance = Vector3.Distance(transform.position, candidate.transform.position);
            if (distance <= baitDetectionRadius && distance < minDist)
            {
                minDist = distance;
                bait = candidate.transform;
            }
        }
        return bait != null;
    }
    public Vector3 EvadeSteering()
    {
        if (_targetAgent == null)
            return Vector3.zero;

        return Evade(_targetAgent) * evadeSteeringMultiplier;
    }
    public Vector3 ArriveSteering(Transform bait) => Arrive(bait.position);

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
        _velocity = Vector3.zero;
        if(_health != null) _health.ForceDown();
    }

    public void StopMovement()
    {
        _velocity = Vector3.zero;
    }

    public void ResetMovement()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));

        _velocity = randomDirection.normalized * maxSpeed;
    }

    public void Respawn()
    {
        if (_health == null)
            _health = GetComponent<BoidHealth>();

        if (_boidSpawner != null)
        {
            Transform spawnPoint = _boidSpawner.GetRandomSpawnPoint();

            if (spawnPoint != null)
                transform.position = spawnPoint.position;
        }

        _health.ResetHealth();
        ResetMovement();
        IsRespawning = false;
    }

    public void Move(Vector3 steering)
    {
        _velocity += steering;
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
        transform.position += _velocity * Time.deltaTime;

        if (_velocity != Vector3.zero)
            transform.forward = _velocity;

        if (goatPen != null)
        {
            if (goatPen.IsOutside(transform.position))
            {
                BoundaryCrossings++;
            }

            transform.position = goatPen.OutOfGoatPen(transform.position);
        }
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
        if (_stateMachine == null || IsRespawning)
            return;

        IsRespawning = true;
        _stateMachine.ChangeState(BoidStateType.Respawn);
    }

    public Vector3 Flocking()
    {
       
        return CalculateSeparation(_nearbyBoids, separationRadius) * separationWeight
                + CalculateAlignment(_nearbyBoids, alignmentRadius) * alignmentWeight
                + CalculateCohesion(_nearbyBoids, cohesionRadius) * cohesionWeight;
    }

    private Vector3 CalculateSeparation(List<BoidSteering> list, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (var item in list)
        {
            if (item == null) continue;
            if (item == this) continue;

            BoidHealth health = item.Health;

            if (health != null && health.IsDown) continue;

            if ((item.transform.position - transform.position).sqrMagnitude <= radius * radius)
            {
                desired += (item.transform.position - transform.position);
                count++;
            }

        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return CalculateSteering(-desired.normalized * maxSpeed);
    }

    private Vector3 CalculateAlignment(List<BoidSteering> list, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (var item in list)
        {
            if (item == null) continue;
            if (item == this) continue;

            BoidHealth health = item.Health;

            if (health != null && health.IsDown) continue;

            if ((item.transform.position - transform.position).sqrMagnitude <= radius * radius)
            {
                desired += item.Velocity;
                count++;
            }

        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return CalculateSteering(desired.normalized * maxSpeed);
    }
    private Vector3 CalculateCohesion(List<BoidSteering> list, float radius)
    {
        Vector3 desired = default;
        int count = 0;

        foreach (var item in list)
        {
            if (item == null) continue;
            if (item == this) continue;

            BoidHealth health = item.Health;

            if (health != null && health.IsDown) continue;

            if ((item.transform.position - transform.position).sqrMagnitude <= radius * radius)
            {
                desired += item.transform.position;
                count++;
            }

        }
        if (count == 0) return Vector3.zero;
        desired /= count;
        return Seek(desired);
    }

    private Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;

        steering = Vector3.ClampMagnitude(steering, maxSteering * Time.deltaTime);
        return steering;

    }
    private Vector3 DesiredVector(Vector3 target)
    {
        Vector3 desired = (target - transform.position).normalized;
        desired *= maxSpeed;
        return desired;
    }

    private Vector3 Seek(Vector3 target)
    {
        var desired = DesiredVector(target);

        return CalculateSteering(desired);
    }

    private Vector3 Flee(Vector3 target)
    {
        var desired = DesiredVector(target);

        return CalculateSteering(-desired);
    }

    private Vector3 Arrive(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        float distance = direction.magnitude;

        if (distance < minDistance)
        
            return (Vector3.zero);        

        float targetSpeed = maxSpeed * (distance / slowingDistance);
        float desiredSpeed = Mathf.Min(targetSpeed, maxSpeed);

        Vector3 desired = direction.normalized * desiredSpeed;
        return CalculateSteering(desired);
    }

    private Vector3 CalculateFuture(Agent target)
    {
        Vector3 direccion = target.transform.position - transform.position;

        float distance = direccion.magnitude;

        float prediction = distance / (maxSpeed + target.Velocity.magnitude);

        Vector3 futurePosition = target.transform.position + target.Velocity * prediction;
        return (futurePosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        FSM_Hunter hunter = other.GetComponentInParent<FSM_Hunter>();

        if (hunter != null)
        {
            _targetAgent = hunter;
        }
        BoidSteering otherBoid = other.GetComponentInParent<BoidSteering>();

        if (otherBoid != null && otherBoid != this && !_nearbyBoids.Contains(otherBoid))
        {
            _nearbyBoids.Add(otherBoid);
        }

        Bait bait = other.GetComponentInParent<Bait>();

        if (bait != null && !_nearbyBaits.Contains(bait))
        {
            _nearbyBaits.Add(bait);
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FSM_Hunter hunter = other.GetComponentInParent<FSM_Hunter>();

        if (hunter != null && _targetAgent == hunter)
        {
            _targetAgent = null;
        }
        BoidSteering otherBoid = other.GetComponentInParent<BoidSteering>();

        if (otherBoid != null && otherBoid != this)
        {
            _nearbyBoids.Remove(otherBoid);
        }

        Bait bait = other.GetComponentInParent<Bait>();

        if (bait != null)
        {
            _nearbyBaits.Remove(bait);
        }
    }

    private Vector3 Evade(Agent target)
    {
        var futurePosition = CalculateFuture(target); 
        return Flee(futurePosition);
    }

    public void SetVisible(bool visible)
    {
        foreach (Renderer renderer in _renderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }

        foreach (Canvas canvas in _canvases)
        {
            if (canvas != null)
                canvas.enabled = visible;
        }
    }

    public void UpdateStateText(string stateName)
    {
        if (stateText != null)
        {
            stateText.text = stateName;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, cohesionRadius);

        Gizmos.color= Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alignmentRadius);

        Gizmos.color= Color.black;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color= Color.white;
        Gizmos.DrawWireSphere(transform.position, baitDetectionRadius);
    }


}